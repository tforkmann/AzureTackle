namespace AzureTackle

open System
open Azure.Data.Tables
open TableReflection
open System.Threading
open System.Threading.Tasks

[<AutoOpen>]
module Filter =
    type Operator =
        | LessThan
        | LessThanOrEqual
        | GreaterThan
        | GreaterThanOrEqual
        | Equal
        | NotEqual

    type AzureFilter =
        | Flt of string * Operator * float
        | Txt of string * Operator * string
        | Dtm of string * Operator * DateTime
        | DtmO of string * Operator * DateTimeOffset
        | PaKey of Operator * string
        | RoKey of Operator * string
        | TStmp of Operator * DateTimeOffset

module Table =
    type AzureAccount = { TableServiceClient: TableServiceClient }

    type AzureConnection =
        | AzureConnection of string
        | UseTableServiceClient of TableServiceClient
        member this.Connect() =
            match this with
            | AzureConnection connectionString -> { TableServiceClient = TableServiceClient(connectionString) }
            | UseTableServiceClient tableServiceClient -> { TableServiceClient = tableServiceClient }


    // let getTable tableName (azConnection: AzureAccount) =
    //     task {
    //         let client = azConnection.TableServiceClient

    //         let table =
    //             try
    //                 client.GetTableReference tableName
    //             with
    //             | exn ->
    //                 let msg =
    //                     sprintf "Could not get TableReference %s" exn.Message

    //                 failwith msg

    //         return table
    //     }
    //     |> Async.AwaitTask
    //     |> Async.RunSynchronously
    let tablesCreated = System.Collections.Concurrent.ConcurrentDictionary<string, TableClient>()

    let getTableClient (tableName) (azConnection: AzureAccount)= task {
        match tablesCreated.TryGetValue tableName with
        | true, tableClient ->
            return tableClient
        | _ ->
            let tableClient = azConnection.TableServiceClient.GetTableClient(tableName)
            let! _ = tableClient.CreateIfNotExistsAsync()
            tablesCreated.TryAdd(tableName, tableClient) |> ignore
            return tableClient
    }

    let getAndCreateTable tableName (azConnection: AzureAccount) =
        task {
            let client =
                azConnection.TableServiceClient

            let table =
                try
                    client.GetTableClient(tableName)
                with
                | exn ->

                    let msg =
                        sprintf "Could not get TableReference %s" exn.Message

                    printfn "Could not get TableReference %s" exn.Message
                    failwith msg
            // Azure will temporarily lock table names after deleting and can take some time before the table name is made available again.
            let mutable finished = false
            while not finished do
                try
                    client.CreateTableIfNotExistsAsync(tableName)
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                    |> ignore
                    finished <- true
                with
                | _ ->
                    Threading.Thread.Sleep 5000
            return table
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let receiveValue (partKey, rowKey) (table: TableClient) =
        task {
            let! response = table.GetEntityAsync(partKey, rowKey)
            let result = response.Value

            if isNull result then
                return None
            else
                return Some result
        }

    let query (filter: string option) (table: TableClient) token : Task<Azure.AsyncPageable<'a>>=
        task {

        match filter with
        | Some f ->
            return table.QueryAsync<'a>(f,Nullable(1500),[||],token)
        | None -> return table.QueryAsync<'a>("",Nullable(1500),[||],token)
        }

    // let getResultsRecursively (filter: string option) (table: CloudTable) =
    //     task {
    //         let rec getResults token =
    //             task {
    //                 let query =
    //                     match filter with
    //                     | Some f -> TableQuery().Where(f)
    //                     | None -> TableQuery()

    //                 let! result = table.ExecuteQuerySegmentedAsync(query, token)

    //                 let token = result.ContinuationToken
    //                 let result = result |> Seq.toList

    //                 if isNull token then
    //                     return result
    //                 else
    //                     let! others = getResults token
    //                     return result @ others
    //             }

    //         return! getResults null
    //     }

    // let getResultsRecursively (filter: string option) (table: TableClient) =
    //     task {
    //         let rec getResults token =
    //             task {

    //                 let! result  = query filter table token
    //                 let pages = result.AsPages().GetAsyncEnumerator()
    //                 let result = pages.Current.Values |> Seq.toList
    //                 let token = result.ContinuationToken

    //                 if isNull token then
    //                     return result
    //                 else
    //                     let! others = getResults token
    //                     return result @ others
    //             }

    //         return! getResults null
    //     }



[<RequireQualifiedAccess>]
module AzureTable =
    open Table

    type AzureTableConfig =
        { AzureTable: TableClient option
          TableName: string option
          AzureAccount: AzureAccount option }

    type StorageOption =
        { Stage: Stage option
          DevStorage: AzureTableConfig option
          ProdStorage: AzureTableConfig }

    type TableProps =
        { Filters: AzureFilter list
          FilterReceive: (string * string) option
          StorageOption: StorageOption option
          Token : CancellationToken }

    let private defaultAzConfig () =
        { AzureTable = None
          TableName = None
          AzureAccount = None }

    let private defaultProps () =
        { Filters = []
          FilterReceive = None
          StorageOption = None
          Token = CancellationToken.None}

    let connect (connectionString: string) =
        let connection = AzureConnection connectionString

        let initAzConfig =
            { defaultAzConfig () with AzureAccount = Some(connection.Connect()) }

        let initStorage =
            { Stage = None
              ProdStorage = initAzConfig
              DevStorage = None }

        { defaultProps () with StorageOption = Some initStorage }

    let connectWithStages (connectionStringProd: string, connectionStringDev: string, stage: Stage) =
        let connection = AzureConnection connectionStringProd
        let connectionBackUp = AzureConnection connectionStringDev

        let initAzConfig =
            { defaultAzConfig () with AzureAccount = Some(connection.Connect()) }

        let initAzConfigBackup =
            { defaultAzConfig () with AzureAccount = Some(connectionBackUp.Connect()) }

        { defaultProps () with
            StorageOption =
                Some
                    { Stage = Some stage
                      ProdStorage = initAzConfig
                      DevStorage = Some initAzConfigBackup } }

    let table tableName (props: TableProps) =
        try
            let newStorageOption =
                match props.StorageOption with
                | Some storageOption ->
                    match storageOption.DevStorage with
                    | Some backup ->
                        match backup.AzureAccount, storageOption.ProdStorage.AzureAccount with
                        | Some backUpAcc, Some normalAcc ->

                            { storageOption with
                                DevStorage =
                                    Some
                                        { backup with
                                            AzureTable = Some(getAndCreateTable tableName backUpAcc)
                                            TableName = Some tableName }
                                ProdStorage =
                                    { storageOption.ProdStorage with
                                        AzureTable = Some(getAndCreateTable tableName normalAcc)
                                        TableName = Some tableName } }
                        | _ ->

                            printfn "please use connect to initialize the Azure backup connection"
                            failwith "please use connect to initialize the Azure backup connection"
                    | None ->
                        match storageOption.ProdStorage.AzureAccount with
                        | Some normalAcc ->
                            { storageOption with
                                ProdStorage =
                                    { storageOption.ProdStorage with
                                        AzureTable = Some(getAndCreateTable tableName normalAcc)
                                        TableName = Some tableName } }
                        | _ ->
                            printfn "please use connect to initialize the Azure backup connection"
                            failwith "please use connect to initialize the Azure backup connection"

                | None ->
                    printfn "please use connect to initialize the Azure connection"
                    failwith "please use connect to initialize the Azure connection"

            { props with StorageOption = Some newStorageOption }
        with
        | exn -> failwithf "Could not get a table %s" exn.Message

    let filter (filters: AzureFilter list) (props: TableProps) = { props with Filters = filters }

    let filterReceive (partKey, rowKey) (props: TableProps) =
        { props with FilterReceive = Some(partKey, rowKey) }

    let appendFilters (filters: AzureFilter list) =
        let matchOperator operator =
            match operator with
            | LessThan -> "lt"
            | LessThanOrEqual -> "le"
            | GreaterThan -> "gt"
            | GreaterThanOrEqual -> "ge"
            | Equal -> "eq"
            | NotEqual -> "ne"

        filters
        |> List.fold
            (fun r s ->
                let filterString =
                    match s with
                    | Flt (fieldName, operator, value) ->
                        TableQuery.GenerateFilterConditionForDouble(fieldName, matchOperator operator, value)
                    | Txt (fieldName, operator, value) ->
                        TableQuery.GenerateFilterCondition(fieldName, matchOperator operator, value)
                    | Dtm (fieldName, operator, value) ->
                        TableQuery.GenerateFilterConditionForDate(
                            fieldName,
                            matchOperator operator,
                            (value |> DateTimeOffset)
                        )
                    | DtmO (fieldName, operator, value) ->
                        TableQuery.GenerateFilterConditionForDate(fieldName, matchOperator operator, value)
                    | PaKey (operator, value) ->
                        TableQuery.GenerateFilterCondition("PartitionKey", matchOperator operator, value)
                    | RoKey (operator, value) ->
                        TableQuery.GenerateFilterCondition("RowKey", matchOperator operator, value)
                    | TStmp (operator, value) ->
                        TableQuery.GenerateFilterConditionForDate("Timestamp", matchOperator operator, value)

                if r = "" then
                    r + filterString
                else
                    r + " and " + filterString)
            ""
        |> Some

    let getTable props =
        let storageOption =
            match props.StorageOption with
            | Some x -> x
            | _ -> failwith "please add a storage account"

        match storageOption.ProdStorage.AzureTable with
        | Some table -> table
        | None -> failwith "please add a table"

    let findDevTable props =
        match props.StorageOption with
        | Some storageOption ->
            match storageOption.DevStorage with
            | Some devStorage ->
                match devStorage.AzureTable with
                | Some table -> Some table
                | None -> None
            | _ -> None
        | _ -> None

    let receive (read: AzureTackleRowEntity -> 't) (props: TableProps) =
        task {
            let storageOption =
                match props.StorageOption with
                | Some x -> x
                | _ -> failwith "please add a storage account"

            let azureTable =
                match storageOption.ProdStorage.AzureTable with
                | Some table -> table
                | None -> failwith "please add a table"

            let keys =
                match props.FilterReceive with
                | Some keys -> keys
                | None -> failwith "please use filterReceive to set a the PartitionKey and RowKey"

            let! result = receiveValue keys azureTable

            return
                result
                |> Option.map AzureTackleRowEntity
                |> Option.map read
        }

    let execute (read: AzureTackleRowEntity -> 't) (props: TableProps) =
        task {
            try
                let azureTable = getTable props
                let filter = appendFilters props.Filters
                let! results = query filter azureTable token

                return
                    Ok [|
                        for result in results ->
                            let e = AzureTackleRowEntity(result)
                            read e
                    |]
            with
            | exn -> return Error exn
        }

    let executeDirect (read: AzureTackleRowEntity -> 't) (props: TableProps) =
        task {
            try
                let azureTable = getTable props

                let filter = appendFilters props.Filters
                let! results = getResultsRecursively filter azureTable

                return
                    [| for result in results ->
                           let e = AzureTackleRowEntity(result)
                           read e |]
            with
            | exn -> return failwithf "ExecuteDirect failed with exn: %s" exn.Message
        }

    let insert (partKey, rowKey: RowKey) (set: AzureTackleSetEntity -> TableEntity) cancellationToken (props: TableProps) =
        task {
            try
                let entity = AzureTackleSetEntity(partKey, rowKey.GetValue) |> set
                match props.StorageOption with
                | Some sOption ->
                    match sOption.Stage with
                    | Some stage ->
                        match stage with
                        | Dev ->
                            match findDevTable props with
                            | Some devTable ->
                                let! _ = devTable.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken)
                                return Ok()

                            | _ -> return Ok()

                        | Prod ->
                            let azureTable = getTable props

                            match findDevTable props with
                            | Some devTable ->
                                let! _ = devTable.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken)
                                return Ok()
                            | None ->
                                let! _ = azureTable.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken)

                                return Ok()
                    | None ->
                        let azureTable = getTable props
                        let! _ = azureTable.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken)
                        return Ok()
                | _ ->
                    printfn "please use connect to initialize the Azure connection"
                    return failwith "please use connect to initialize the Azure connection"
            with
            | exn ->
                return Error exn
        }

    let insertBatch (messages: 'a array)  (mapper: 'a -> TableEntity) (props: TableProps) =
        task {
            try
                match props.StorageOption with
                | Some sOption ->
                    match sOption.Stage with
                    | Some stage ->
                        match stage with
                        | Dev ->
                            match findDevTable props with
                            | Some devTable ->
                                let actions =
                                    messages
                                    |> Array.map mapper
                                    |> Array.map (fun entity -> TableTransactionAction(TableTransactionActionType.UpsertReplace, entity))
                                let! _ = devTable.SubmitTransactionAsync(actions)
                                return Ok()

                            | _ -> return Ok()

                        | Prod ->
                            let azureTable = getTable props

                            let actions =
                                messages
                                |> Array.map mapper
                                |> Array.map (fun entity -> TableTransactionAction(TableTransactionActionType.UpsertReplace, entity))
                            match findDevTable props with
                            | Some devTable ->
                                let! _ = devTable.SubmitTransactionAsync(actions)
                                return Ok()
                            | None ->
                                let! _ = azureTable.SubmitTransactionAsync(actions)
                                return Ok()
                    | None ->

                        let azureTable = getTable props
                        let actions =
                            messages
                            |> Array.map mapper
                            |> Array.map (fun entity -> TableTransactionAction(TableTransactionActionType.UpsertReplace, entity))
                        let! _ = azureTable.SubmitTransactionAsync(actions)
                        return Ok()
                | _ ->
                    printfn "please use connect to initialize the Azure connection"
                    return failwith "please use connect to initialize the Azure connection"
            with
            | exn ->
                return Error exn
        }

    let delete (partKey, rowKey) (props: TableProps) =
        task {
            try
                match props.StorageOption with
                | Some sOption ->
                    match sOption.Stage with
                    | Some stage ->
                        match stage with
                        | Dev ->
                            match findDevTable props with
                            | Some devTable ->
                                let! _ = devTable.DeleteEntityAsync(partKey, rowKey)
                                return Ok()

                            | _ -> return Ok()

                        | Prod ->
                            let azureTable = getTable props

                            match findDevTable props with
                            | Some devTable ->
                                let! _ = devTable.DeleteEntityAsync(partKey, rowKey)
                                return Ok()
                            | None ->
                                let! _ = azureTable.DeleteEntityAsync(partKey, rowKey)

                                return Ok()
                    | None ->
                        let azureTable = getTable props
                        let! _ = azureTable.DeleteEntityAsync(partKey, rowKey)
                        return Ok()
                | _ ->
                    printfn "please use connect to initialize the Azure connection"
                    return failwith "please use connect to initialize the Azure connection"
            with
            | exn ->
                return Error exn
        }

    let deleteBatch (messages: 'a array)  (mapper: 'a -> TableEntity) (props: TableProps) =
        task {
            try
                match props.StorageOption with
                | Some sOption ->
                    match sOption.Stage with
                    | Some stage ->
                        match stage with
                        | Dev ->
                            match findDevTable props with
                            | Some devTable ->
                                let actions =
                                    messages
                                    |> Array.map mapper
                                    |> Array.map (fun entity -> TableTransactionAction(TableTransactionActionType.Delete, entity))
                                let! _ = devTable.SubmitTransactionAsync(actions)
                                return Ok()

                            | _ -> return Ok()

                        | Prod ->
                            let azureTable = getTable props

                            let actions =
                                messages
                                |> Array.map mapper
                                |> Array.map (fun entity -> TableTransactionAction(TableTransactionActionType.Delete, entity))
                            match findDevTable props with
                            | Some devTable ->
                                let! _ = devTable.SubmitTransactionAsync(actions)
                                return Ok()
                            | None ->
                                let! _ = azureTable.SubmitTransactionAsync(actions)
                                return Ok()
                    | None ->

                        let azureTable = getTable props
                        let actions =
                            messages
                            |> Array.map mapper
                            |> Array.map (fun entity -> TableTransactionAction(TableTransactionActionType.Delete, entity))
                        let! _ = azureTable.SubmitTransactionAsync(actions)
                        return Ok()
                | _ ->
                    printfn "please use connect to initialize the Azure connection"
                    return failwith "please use connect to initialize the Azure connection"
            with
            | exn ->
                return Error exn
        }

    let executeWithReflection<'a> (props: TableProps) =
        task {
            try
                let azureTable = getTable props
                let filter = appendFilters props.Filters
                let! results = getResultsRecursively filter azureTable

                return
                    Ok [|
                        for result in results -> result |> buildRecordFromEntityNoCache<'a>
                    |]
            with
            | exn -> return Error exn
        }
