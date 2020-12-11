namespace AzureTackle

open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open FSharp.Control.Tasks.ContextInsensitive
open TableReflection
open System.Threading.Tasks
module Table =
    type AzureAccount =
        { StorageAccount: CloudStorageAccount }
    type AzureConnection =
        | AzureConnection of string
        member this.Connect() =
            match this with
            | AzureConnection connectionString -> { StorageAccount = CloudStorageAccount.Parse connectionString }
    let getTable tableName (azConnection:AzureAccount) =
            task {
                let client = azConnection.StorageAccount.CreateCloudTableClient()

                let table =
                    try
                        client.GetTableReference tableName
                    with exn ->
                        let msg =
                            sprintf "Could not get TableReference %s" exn.Message
                        failwith msg
                /// Azure will temporarily lock table names after deleting and can take some time before the table name is made available again.
                let rec createTableSafe() =
                    task {
                        try
                            let! _ = table.CreateIfNotExistsAsync()
                            ()
                        with _ ->
                            do! Task.Delay 5000
                            return! createTableSafe()
                    }
                do! createTableSafe()
                return table
            }
            |> Async.AwaitTask
            |> Async.RunSynchronously

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

type AzureFilter =
    | Flt of string * float
    | Txt of string * string
    | Dtm of string * DateTime
    | DtmO of string * DateTimeOffset
    | PartKey of  string
    | SortableRowKey of  string

[<RequireQualifiedAccess>]
module AzureTable =
    open Table

    type TableProps =
        { Filters: AzureFilter list
          AzureTable: CloudTable option
          AzureAccount : AzureAccount option }

    let private defaultProps () = { Filters = []; AzureTable = None; AzureAccount = None}

    let connect (connectionString: string) =
        let connection =  AzureConnection connectionString
        { defaultProps () with
              AzureAccount = Some (connection.Connect()) }
    let table tableName (props: TableProps) =
        match props.AzureAccount with
        | Some azureAccount ->
            { props with
                AzureTable = Some (getTable tableName azureAccount) }
        | None -> failwith "please use connect to initialize the Azure connection"

    let filter (filters: AzureFilter list) (props: TableProps) = { props with Filters = filters }

    let appendFilters (filters: AzureFilter list) =

        filters
        |> List.fold (fun r s ->
            let filterString =
                match s with
                | Flt (fieldName, value) ->
                    TableQuery.GenerateFilterConditionForDouble(fieldName, QueryComparisons.Equal, value)
                | Txt (fieldName, value) -> TableQuery.GenerateFilterCondition(fieldName, QueryComparisons.Equal, value)
                | Dtm (fieldName, value) -> TableQuery.GenerateFilterConditionForDate(fieldName, QueryComparisons.Equal, (value |> DateTimeOffset))
                | DtmO (fieldName, value) -> TableQuery.GenerateFilterConditionForDate(fieldName, QueryComparisons.Equal, value)
                | PartKey value -> TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, value)
                | SortableRowKey value -> TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, value)

            if r = "" then r + filterString else r + " and " + filterString) ""
        |> Some

    let execute (read: AzureTackleRowEntity -> 't) (props: TableProps) =
        task {
            try
                let azureTable =
                    match props.AzureTable with
                    | Some table -> table
                    | None -> failwith "please add a table"

                let filter = appendFilters props.Filters
                let! results = getResultsRecursivly filter azureTable
                return Ok [| for result in results ->
                                let e = AzureTackleRowEntity(result)
                                read e |]
            with exn -> return Error exn
        }
    let executeWithReflection<'a> (props: TableProps) =
        task {
            try
                let azureTable =
                    match props.AzureTable with
                    | Some table -> table
                    | None -> failwith "please add a table"

                let filter = appendFilters props.Filters
                let! results = getResultsRecursivly filter azureTable
                return Ok [| for result in results ->
                                result |> buildRecordFromEntityNoCache<'a> |]
            with exn -> return Error exn
        }
