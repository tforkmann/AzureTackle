namespace AzureTackle

open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open FSharp.Control.Tasks.ContextInsensitive
open TableReflection
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

module Table =
    type AzureAccount = { StorageAccount: CloudStorageAccount }

    type AzureConnection =
        | AzureConnection of string
        member this.Connect() =
            match this with
            | AzureConnection connectionString -> { StorageAccount = CloudStorageAccount.Parse connectionString }

    let getTable tableName (azConnection: AzureAccount) =
        task {
            let client =
                azConnection.StorageAccount.CreateCloudTableClient()

            let table =
                try
                    client.GetTableReference tableName
                with
                | exn ->
                    let msg =
                        sprintf "Could not get TableReference %s" exn.Message

                    failwith msg

            return table
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let getAndCreateTable tableName (azConnection: AzureAccount) =
        task {
            let client =
                azConnection.StorageAccount.CreateCloudTableClient()

            let table =
                try
                    client.GetTableReference tableName
                with
                | exn ->

                    let msg =
                        sprintf "Could not get TableReference %s" exn.Message

                    printfn "Could not get TableReference %s" exn.Message
                    failwith msg
            /// Azure will temporarily lock table names after deleting and can take some time before the table name is made available again.
            let rec createTableSafe () =
                task {
                    try
                        let! _ = table.CreateIfNotExistsAsync()
                        ()
                    with
                    | exn ->
                        printfn "Error in creating %s" exn.Message
                        do! Task.Delay 5000
                        return! createTableSafe ()
                }

            do! createTableSafe ()
            printfn "created table..."
            return table
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let receiveValue (partKey, rowKey) (table: CloudTable) =
        task {
            let query = TableOperation.Retrieve(partKey, rowKey)
            let! r = table.ExecuteAsync(query)
            let result = r.Result :?> DynamicTableEntity

            if isNull result then
                return None
            else
                return Some result
        }

    let getResultsRecursivly (filter: string option) (table: CloudTable) =
        task {
            let rec getResults token =
                task {
                    let query =
                        match filter with
                        | Some f -> TableQuery().Where(f)
                        | None -> TableQuery()

                    let! result = table.ExecuteQuerySegmentedAsync(query, token)

                    let token = result.ContinuationToken
                    let result = result |> Seq.toList

                    if isNull token then
                        return result
                    else
                        let! others = getResults token
                        return result @ others
                }

            return! getResults null
        }



[<RequireQualifiedAccess>]
module AzureTable =
    open Table

    type AzureTableConfig =
        { AzureTable: CloudTable option
          TableName: string option
          AzureAccount: AzureAccount option }

    type StorageOption =
        { Normal: AzureTableConfig
          Backup: AzureTableConfig option }

    type TableProps =
        { Filters: AzureFilter list
          FilterReceive: (string * string) option
          StorageOption: StorageOption option }


    let private defaultAzConfig () =
        { AzureTable = None
          TableName = None
          AzureAccount = None }

    let private defaultProps () =
        { Filters = []
          FilterReceive = None
          StorageOption = None }

    let connect (connectionString: string) =
        let connection = AzureConnection connectionString

        let initAzConfig =
            { defaultAzConfig () with
                  AzureAccount = Some(connection.Connect()) }

        let initStorage = { Normal = initAzConfig; Backup = None }

        { defaultProps () with
              StorageOption = Some initStorage }

    let connectWithBackup (connectionString: string, connectionStringBackup: string) =
        let connection = AzureConnection connectionString
        let connectionBackUp = AzureConnection connectionStringBackup

        let initAzConfig =
            { defaultAzConfig () with
                  AzureAccount = Some(connection.Connect()) }

        let initAzConfigBackup =
            { defaultAzConfig () with
                  AzureAccount = Some(connectionBackUp.Connect()) }

        { defaultProps () with
              StorageOption =
                  Some
                      { Normal = initAzConfig
                        Backup = Some initAzConfigBackup } }

    let table tableName (props: TableProps) =
        try
            let newStorageOption =
                match props.StorageOption with
                | Some storageOption ->
                    match storageOption.Backup with
                    | Some backup ->
                        match backup.AzureAccount, storageOption.Normal.AzureAccount with
                        | Some backUpAcc, Some normalAcc ->
                            printfn "backupSome"

                            { storageOption with
                                  Backup =
                                      Some
                                          { backup with
                                                AzureTable = Some(getAndCreateTable tableName backUpAcc)
                                                TableName = Some tableName }
                                  Normal =
                                      { storageOption.Normal with
                                            AzureTable = Some(getAndCreateTable tableName normalAcc)
                                            TableName = Some tableName } }
                        | _ ->

                            printfn "please use connect to initialize the Azure backup connection"
                            failwith "please use connect to initialize the Azure backup connection"
                    | None ->
                        match storageOption.Normal.AzureAccount with
                        | Some normalAcc ->
                            { storageOption with
                                  Normal =
                                      { storageOption.Normal with
                                            AzureTable = Some(getAndCreateTable tableName normalAcc)
                                            TableName = Some tableName } }
                        | _ ->
                            printfn "please use connect to initialize the Azure backup connection"
                            failwith "please use connect to initialize the Azure backup connection"

                | None ->
                    printfn "please use connect to initialize the Azure backup connection"
                    failwith "please use connect to initialize the Azure connection"

            { props with
                  StorageOption = Some newStorageOption }
        with
        | exn -> failwithf "Could not get a table %s" exn.Message

    let filter (filters: AzureFilter list) (props: TableProps) = { props with Filters = filters }

    let filterReceive (partKey, rowKey) (props: TableProps) =
        { props with
              FilterReceive = Some(partKey, rowKey) }

    let appendFilters (filters: AzureFilter list) =
        let matchOperator operator =
            match operator with
            | LessThan -> QueryComparisons.LessThan
            | LessThanOrEqual -> QueryComparisons.LessThanOrEqual
            | GreaterThan -> QueryComparisons.GreaterThan
            | GreaterThanOrEqual -> QueryComparisons.GreaterThanOrEqual
            | Equal -> QueryComparisons.Equal
            | NotEqual -> QueryComparisons.NotEqual

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

        match storageOption.Normal.AzureTable with
        | Some table -> table
        | None -> failwith "please add a table"

    let findBackUpTable props =
        match props.StorageOption with
        | Some storageOption ->
            match storageOption.Backup with
            | Some backup ->
                match backup.AzureTable with
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
                match storageOption.Normal.AzureTable with
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
                let! results = getResultsRecursivly filter azureTable

                return
                    Ok [| for result in results ->
                              let e = AzureTackleRowEntity(result)
                              read e |]
            with
            | exn -> return Error exn
        }

    let executeDirect (read: AzureTackleRowEntity -> 't) (props: TableProps) =
        task {
            try
                let azureTable = getTable props

                let filter = appendFilters props.Filters
                let! results = getResultsRecursivly filter azureTable

                return
                    [| for result in results ->
                           let e = AzureTackleRowEntity(result)
                           read e |]
            with
            | exn -> return failwithf "ExecuteDirect failed with exn: %s" exn.Message
        }



    let insertOperation operation props () =
        task {
            let azureTable = getTable props

            let! _ =
                match findBackUpTable props with
                | Some backupTable ->
                    task {
                        let! _ = backupTable.ExecuteAsync operation
                        return printfn "added Values"
                    }

                | _ -> task { () }

            let! _ = azureTable.ExecuteAsync operation
            ()
        }

    let deleteTask (azureTable: CloudTable) (partKey, rowKey) () =
        task {
            let! retrieveOp =
                TableOperation.Retrieve(partKey, rowKey)
                |> azureTable.ExecuteAsync

            let result = retrieveOp.Result :?> DynamicTableEntity

            if isNull result then
                return Error(exn (sprintf "no entity existent for partKey %s rowKey %s" rowKey partKey))
            else

                let delete = TableOperation.Delete(result)
                let! _ = azureTable.ExecuteAsync(delete)
                return Ok()
        }

    let deleteOperation (partKey, rowKey) props () =
        task {
            let azureTable = getTable props

            let! _ =
                match findBackUpTable props with
                | Some backupTable ->
                    task {
                        let! _ = deleteTask backupTable (partKey, rowKey) ()
                        return ()
                    }

                | _ -> task { () }

            let! _ = deleteTask azureTable (partKey, rowKey) ()
            ()
        }

    let insert (partKey, rowKey: RowKey) (set: AzureTackleSetEntity -> DynamicTableEntity) (props: TableProps) =
        task {
            try
                let entity =
                    let e =
                        AzureTackleSetEntity(partKey, rowKey.GetValue)

                    set e

                let operation = TableOperation.InsertOrReplace entity
                do! insertOperation operation props ()
                return Ok()
            with
            | exn -> return Error exn
        }

    let insertCustomRowKey (partKey, rowKey) (set: AzureTackleSetEntity -> DynamicTableEntity) (props: TableProps) =
        task {
            try
                let entity =
                    let e = AzureTackleSetEntity(partKey, rowKey)
                    set e

                let operation = TableOperation.InsertOrReplace entity
                do! insertOperation operation props ()
                return Ok()
            with
            | exn -> return Error exn
        }

    let delete (partKey, rowKey) (props: TableProps) =
        task {
            try
                do! deleteOperation (partKey, rowKey) props ()
                return Ok()
            with
            | exn -> return Error exn
        }

    let executeWithReflection<'a> (props: TableProps) =
        task {
            try
                let azureTable = getTable props
                let filter = appendFilters props.Filters
                let! results = getResultsRecursivly filter azureTable
                return Ok [| for result in results -> result |> buildRecordFromEntityNoCache<'a> |]
            with
            | exn -> return Error exn
        }
