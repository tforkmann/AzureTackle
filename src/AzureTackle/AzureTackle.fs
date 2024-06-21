namespace AzureTackle

open System
open Azure.Data.Tables
open System.Threading
open System.Threading.Tasks
open FSharp.Control
open System.Text
open System.Globalization
open System.Runtime.CompilerServices

[<AutoOpen>]
module Filter =

    module Keys =
        [<Literal>]
        let PartitionKey = "PartitionKey"

        [<Literal>]
        let RowKey = "RowKey"

    type ColumnComparison =
        | LessThan of obj
        | LessThanOrEqual of obj
        | GreaterThan of obj
        | GreaterThanOrEqual of obj
        | Equal of obj
        | NotEqual of obj

    type BinaryOperation =
        | And
        | Or

    type UnaryOperation = | Not

    type AzureFilter =
        | Empty
        | Column of string * ColumnComparison
        | Binary of AzureFilter * BinaryOperation * AzureFilter
        | Unary of UnaryOperation * AzureFilter

        static member (+)(a, b) = Binary(a, And, b)
        static member (*)(a, b) = Binary(a, Or, b)
        static member (!!) a = Unary(Not, a)

    module private ColumnComparison =
        let comparison =
            function
            | LessThan _ -> "lt"
            | LessThanOrEqual _ -> "le"
            | GreaterThan _ -> "gt"
            | GreaterThanOrEqual _ -> "ge"
            | Equal _ -> "eq"
            | NotEqual _ -> "ne"

        let value =
            function
            | LessThan v -> v
            | LessThanOrEqual v -> v
            | GreaterThan v -> v
            | GreaterThanOrEqual v -> v
            | Equal v -> v
            | NotEqual v -> v

    module private BinaryOperation =
        let operation =
            function
            | And -> "and"
            | Or -> "or"

    module private UnaryOperation =
        let operation =
            function
            | Not -> "not"


    module private StringValue =
        let private formatProvider = CultureInfo.InvariantCulture

        let forBinary (value: byte[]) =
            let sb = StringBuilder()

            for num in value do
                sb.AppendFormat("{0:x2}", num :> obj) |> ignore

            String.Format(formatProvider, "X'{0}'", sb.ToString())

        let forBool (value: bool) = if value then "true" else "false"

        let forDateTimeOffset (value: DateTimeOffset) =
            let v = value.UtcDateTime.ToString("o", formatProvider)

            sprintf "datetime'%s'" v

        let forDateTime (value: DateTime) =
            DateTimeOffset(value) |> forDateTimeOffset

        let forDouble (value: double) = Convert.ToString(value, formatProvider)

        let forGuid (value: Guid) = sprintf "guid'%s'" (value.ToString())

        let forInt (value: int) = Convert.ToString(value, formatProvider)

        let forLong (value: int64) =
            Convert.ToString(value, formatProvider) |> sprintf "%sL"

        let forAny (v: obj) =
            Convert.ToString(v, formatProvider) |> sprintf "'%s'"

    let private getColumnComparison field comp =
        let stringValue =
            match ColumnComparison.value comp with
            | :? (byte[]) as v -> v |> StringValue.forBinary
            | :? bool as v -> v |> StringValue.forBool
            | :? DateTime as v -> v |> StringValue.forDateTime
            | :? DateTimeOffset as v -> v |> StringValue.forDateTimeOffset
            | :? double as v -> v |> StringValue.forDouble
            | :? Guid as v -> v |> StringValue.forGuid
            | :? int as v -> v |> StringValue.forInt
            | :? int64 as v -> v |> StringValue.forLong
            | v -> v |> StringValue.forAny

        sprintf "%s %s %s" field (ColumnComparison.comparison comp) stringValue

    let rec private _toQuery (f: AzureFilter) =
        match f with
        | Empty -> ""
        | Column(field, comp) -> getColumnComparison field comp
        | Binary(w1, op, w2) ->
            match _toQuery w1, _toQuery w2 with
            | "", fq
            | fq, "" -> fq
            | fq1, fq2 -> sprintf "(%s) %s %s" fq1 (BinaryOperation.operation op) fq2
        | Unary(op, w) ->
            match _toQuery w with
            | "" -> ""
            | v -> sprintf "%s (%s)" (UnaryOperation.operation op) v

    let toQuery (f: AzureFilter) =
        match f |> _toQuery with
        | "" -> None
        | x -> Some x

    /// Creates FILTER condition for column
    let column name whereComp = AzureFilter.Column(name, whereComp)
    /// FILTER PK equals
    let PartKey (value: string) = column Keys.PartitionKey (Equal value)
    /// FILTER RK equals
    let RowKey (value: string) = column Keys.RowKey (Equal value)
    /// FILTER column value equals to
    let Equal name (o: obj) = column name (Equal o)
    /// FILTER column value not equals to
    let NOT name (o: obj) = column name (NotEqual o)
    /// FILTER column value greater than
    let GreaterThan name (o: obj) = column name (GreaterThan o)
    /// FILTER column value lower than
    let LessThan name (o: obj) = column name (LessThan o)
    /// FILTER column value greater/equals than
    let GreaterThanOrEqual name (o: obj) = column name (GreaterThanOrEqual o)
    /// FILTER column value lower/equals than
    let LessThanOrEqual name (o: obj) = column name (LessThanOrEqual o)

module Table =
    type AzureAccount = {
        TableServiceClient: TableServiceClient
    }

    type AzureConnection =
        | AzureConnection of string
        | UseTableServiceClient of TableServiceClient

        member this.Connect() =
            match this with
            | AzureConnection connectionString -> {
                TableServiceClient = TableServiceClient(connectionString)
              }
            | UseTableServiceClient tableServiceClient -> {
                TableServiceClient = tableServiceClient
              }

    let tablesCreated =
        System.Collections.Concurrent.ConcurrentDictionary<string, TableClient>()

    let getTableClient (tableName) (azConnection: AzureAccount) =
        match tablesCreated.TryGetValue tableName with
        | true, tableClient -> tableClient
        | _ ->
            let tableClient = azConnection.TableServiceClient.GetTableClient(tableName)

            tableClient.CreateIfNotExists() |> ignore

            tablesCreated.TryAdd(tableName, tableClient) |> ignore


            tableClient


    let getAndCreateTable tableName (azConnection: AzureAccount) =
        task {
            let client = azConnection.TableServiceClient

            let table =
                try
                    client.GetTableClient(tableName)
                with exn ->

                    let msg = sprintf "Could not get TableReference %s" exn.Message

                    printfn "Could not get TableReference %s" exn.Message
                    failwith msg
            // Azure will temporarily lock table names after deleting and can take some time before the table name is made available again.
            let mutable finished = false

            while not finished do
                try
                    client.CreateTableIfNotExists(tableName) |> ignore
                    finished <- true
                with _ ->
                    Threading.Thread.Sleep 5000

            return table
        }

[<RequireQualifiedAccess>]
module AzureTable =
    open Table

    type AzureTableConfig = {
        AzureTable: TableClient option
        TableName: string option
        AzureAccount: AzureAccount option
    }

    type TableProps = {
        Filter: AzureFilter option
        CancellationToken: CancellationToken option
        MaxElements: int option
        AzureTableConfig: AzureTableConfig option
    }

    let private defaultAzConfig () = {
        AzureTable = None
        TableName = None
        AzureAccount = None
    }

    let private defaultProps () = {
        Filter = None
        MaxElements = None
        CancellationToken = None
        AzureTableConfig = None
    }

    let private table tableName (props: TableProps) =
        try
            let newAzureTableConfig =
                match props.AzureTableConfig with
                | Some azureTableConfig ->
                    match azureTableConfig.AzureAccount with
                    | Some azureAccount ->

                        let azureTable = getTableClient tableName azureAccount

                        {
                            azureTableConfig with
                                AzureTable = Some azureTable
                                TableName = Some tableName
                        }

                    | None ->

                        printfn "please use connect to initialize the Azure connection"
                        failwith "please use connect to initialize the Azure connection"


                | None ->
                    printfn "please use connect to initialize the Azure connection"
                    failwith "please use connect to initialize the Azure connection"



            {
                props with
                    AzureTableConfig = Some newAzureTableConfig
            }
        with exn ->
            failwithf "Could not get a table %s" exn.Message


    let withTableClient (tableServiceClient: TableServiceClient, tableName: string) =
        let connection = UseTableServiceClient tableServiceClient

        let initAzConfig = {
            defaultAzConfig () with
                AzureAccount = Some(connection.Connect())
        }

        let props = {
            defaultProps () with
                AzureTableConfig = Some initAzConfig
        }

        table tableName props

    let withConnectionString (connectionString: string, tableName: string) =
        let connection = AzureConnection connectionString

        let initAzConfig = {
            defaultAzConfig () with
                AzureAccount = Some(connection.Connect())
        }

        let props = {
            defaultProps () with
                AzureTableConfig = Some initAzConfig
        }

        table tableName props

    let filter (filter: AzureFilter) (props: TableProps) = { props with Filter = Some filter }

    let withCancellationToken (cancellationToken: CancellationToken) (props: TableProps) = {
        props with
            CancellationToken = Some cancellationToken
    }

    let withMaxElements (maxElements: int) (props: TableProps) = {
        props with
            MaxElements = Some maxElements
    }

    let getTable props =
        let azureTableConfig =
            match props.AzureTableConfig with
            | Some x -> x
            | _ -> failwith "please add a storage account"

        match azureTableConfig.AzureTable with
        | Some table -> table
        | None -> failwith "please add a table"

    let tryGetValue partitionKey rowKey (mapF: TableEntity -> 't) (props: TableProps) =
        task {
            let azureTable = getTable props

            let! response = azureTable.GetEntityIfExistsAsync(partitionKey, rowKey)
            if response.HasValue then
                return None
            else
                return Some (mapF response.Value)
        }

    let execute (mapF: TableEntity -> 't) (props: TableProps) =
        task {
            try
                let azureTable = getTable props

                let filter =
                    match props.Filter with
                    | Some filters -> filters |> toQuery |> Option.defaultValue ""
                    | None -> ""

                let maxElements =
                    match props.MaxElements with
                    | Some x -> min x 1000
                    | None -> 1000

                let pageAble =
                    match props.CancellationToken with
                    | Some t -> azureTable.QueryAsync(filter = filter, maxPerPage = maxElements, cancellationToken = t)
                    | None -> azureTable.QueryAsync(filter = filter, maxPerPage = maxElements)

                let resultEnum = pageAble.AsPages().GetAsyncEnumerator()

                let mutable hasValue = true
                let mutable allValues = System.Collections.Generic.List()

                while hasValue do
                    let! page = resultEnum.MoveNextAsync()

                    match page with
                    | true -> allValues.AddRange(resultEnum.Current.Values)
                    | false -> hasValue <- false

                return
                    allValues.ToArray()
                    |> Array.map mapF
            with exn ->
                return failwithf "ExecuteAsync failed with exn: %s" exn.Message
        }

    let upsertInline (partKey, rowKey) (set: TableEntity -> TableEntity) (props: TableProps) =
        task {
            try
                let azureTable = getTable props

                let entity = TableEntity(partKey, rowKey) |> set

                let! _ =
                    azureTable.UpsertEntityAsync(
                        entity,
                        TableUpdateMode.Replace,
                        props.CancellationToken |> Option.defaultValue CancellationToken.None
                    )

                return ()
            with exn ->
                return failwithf "Upsert failed with exn: %s" exn.Message
        }

    let upsert (entity: TableEntity) (props: TableProps) =
        task {
            try
                let azureTable = getTable props

                let! _ =
                    azureTable.UpsertEntityAsync(
                        entity,
                        TableUpdateMode.Replace,
                        props.CancellationToken |> Option.defaultValue CancellationToken.None
                    )

                return ()
            with exn ->
                return failwithf "Upsert failed with exn: %s" exn.Message
        }

    let upsertBatchInline (messages: 'a array) (mapper: 'a -> TableEntity) (props: TableProps) =
        task {
            try
                let azureTable = getTable props

                let chunks =
                    messages
                    |> Array.map mapper
                    |> Array.chunkBySize 100
                for chunks in chunks do
                    let actions =
                        chunks
                        |> Array.map (fun entity ->
                            TableTransactionAction(TableTransactionActionType.UpsertReplace, entity))

                    let! _ = azureTable.SubmitTransactionAsync(actions)
                    return ()
            with exn ->
                return failwithf "UpsertBatch failed with exn: %s" exn.Message
        }

    let upsertBatch (entities: TableEntity array) (props: TableProps) =
        task {
            try
                let azureTable = getTable props

                for _key, entities in entities |> Array.groupBy (fun x -> x.PartitionKey) do
                    let chunks =
                        entities
                        |> Array.chunkBySize 100

                    for chunks in chunks do
                        let actions =
                            chunks
                            |> Array.map (fun entity ->
                                TableTransactionAction(TableTransactionActionType.UpsertReplace, entity))

                        let! _ = azureTable.SubmitTransactionAsync(actions)
                        ()
            with exn ->
                return failwithf "UpsertBatch failed with exn: %s" exn.Message
        }

    let delete (partKey, rowKey) (props: TableProps) =
        task {
            try
                let azureTable = getTable props

                let! _ = azureTable.DeleteEntityAsync(partKey, rowKey)
                return ()
            with exn ->
                return failwithf "Delete failed with exn: %s" exn.Message
        }

    let deleteBatch (messages: 'a array) (mapper: 'a -> TableEntity) (props: TableProps) =
        task {
            try
                let azureTable = getTable props

                let actions =
                    messages
                    |> Array.map mapper
                    |> Array.map (fun entity -> TableTransactionAction(TableTransactionActionType.Delete, entity))

                let! _ = azureTable.SubmitTransactionAsync(actions)
                return ()

            with exn ->
                return failwithf "DeleteBatch failed with exn: %s" exn.Message
        }

[<Extension>]
type TableEntityExtensions =
    /// Adds a property to the entity to allow chaining.
    ///
    /// # Parameters
    /// - `entity`: The entity to add the property to.
    /// - `key`: The key of the property.
    /// - `value`: The value of the property.
    [<Extension>]
    static member inline Append(entity: TableEntity, key: string, value: obj) =
        entity.Add(key, value)
        entity
    [<Extension>]
    static member inline Append(entity: TableEntity, key: string, value: float) =
        entity.Add(key, value)
        entity

    [<Extension>]
    static member inline Append(entity: TableEntity, key: string, value: decimal) =
        entity.Add(key, value)
        entity

    [<Extension>]
    static member inline AppendOptional(entity: TableEntity, key: string, value: 'a option) =
        match value with
        | Some value -> entity.Add(key, value)
        | None -> ()

        entity
    [<Extension>]
    static member inline AppendOptional(entity: TableEntity, key: string, value: float option) =
        match value with
        | Some value -> entity.Add(key, value)
        | None -> ()

        entity
    [<Extension>]
    static member inline AppendOptional(entity: TableEntity, key: string, value: decimal option) =
        match value with
        | Some value -> entity.Add(key, value)
        | None -> ()

        entity

    [<Extension>]
    static member ReadString (entity:TableEntity,propName:string) =
        entity.GetString(propName)

    [<Extension>]
    static member inline ReadOptionalString (entity:TableEntity,propName:string) =
        if entity.ContainsKey propName then
            Some(entity.GetString(propName))
        else
            None
    [<Extension>]
    static member ReadBoolean (entity:TableEntity,propName:string) =
        entity.GetBoolean(propName).Value

    [<Extension>]
    static member inline ReadOptionalBoolean (entity:TableEntity,propName:string) =
        if entity.ContainsKey propName then
            Some(entity.GetBoolean(propName).Value)
        else
            None

    [<Extension>]
    static member ReadInt (entity:TableEntity,propName:string) =
        try
            entity.GetInt32(propName).Value
        with
        | _ -> (entity.GetInt32(propName).Value)

    [<Extension>]
    static member inline ReadOptionalInt (entity:TableEntity,propName:string) =
        if entity.ContainsKey propName then
            Some(entity.ReadInt(propName))
        else
            None

    [<Extension>]
    static member ReadBigInt (entity:TableEntity,propName:string) =
        try
            entity.GetInt64(propName).Value
        with
        | _ -> (entity.GetInt64(propName).Value)

    [<Extension>]
    static member inline ReadOptionalBigInt (entity:TableEntity,propName:string) =
        if entity.ContainsKey propName then
            Some(entity.ReadBigInt(propName))
        else
            None
    [<Extension>]
    static member ReadBinary (entity:TableEntity,propName:string) =
        entity.GetBinary(propName)

    [<Extension>]
    static member inline ReadOptionalBinary (entity:TableEntity,propName:string) =
        if entity.ContainsKey propName then
            Some(entity.ReadBinary(propName))
        else
            None

    [<Extension>]
    static member ReadDouble (entity:TableEntity,propName:string) =
        try
            entity.GetDouble(propName).Value
        with
        | _ -> double (entity.GetInt32(propName).Value)

    [<Extension>]
    static member inline ReadOptionalDouble (entity:TableEntity,propName:string) =
        if entity.ContainsKey propName then
            Some(entity.ReadDouble(propName))
        else
            None
    [<Extension>]
    static member ReadDecimal (entity:TableEntity,propName:string) =
        try
            entity.GetDouble(propName).Value |> decimal
        with
        | _ -> entity.GetInt32(propName).Value |> decimal

    [<Extension>]
    static member inline ReadOptionalDecimal (entity:TableEntity,propName:string) =
        if entity.ContainsKey propName then
            Some(entity.ReadDecimal(propName))
        else
            None

    [<Extension>]
    static member ReadDateTime (entity:TableEntity,propName:string) =
        entity.GetDateTime(propName).Value

    [<Extension>]
    static member inline ReadOptionalDateTime (entity:TableEntity,propName:string) =
        if entity.ContainsKey propName then
            Some(entity.GetDateTime(propName).Value)
        else
            None
    [<Extension>]
    static member ReadDateTimeOffset (entity:TableEntity,propName:string) =
        entity.GetDateTimeOffset(propName).Value

    [<Extension>]
    static member inline ReadOptionalDateTimeOffset (entity:TableEntity,propName:string) =
        if entity.ContainsKey propName then
            Some(entity.GetDateTimeOffset(propName).Value)
        else
            None
