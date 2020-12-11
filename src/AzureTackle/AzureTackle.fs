namespace AzureTackle

open System
open System.Data
open System.Threading

open Microsoft.WindowsAzure.Storage.Table
open FSharp.Control.Tasks.ContextInsensitive
open TableReflection
module GetTableEntry =

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
    open GetTableEntry

    type TableProps =
        { Filters: AzureFilter list
          AzureTable: CloudTable option }

    let private defaultProps () = { Filters = []; AzureTable = None }

    let table (azureTable: CloudTable) =
        { defaultProps () with
              AzureTable = Some azureTable }

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
