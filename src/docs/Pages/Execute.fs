module Docs.Pages.Execute

open Feliz
open Feliz.Bulma
open Docs.SharedView

let ExecuteView =
    Html.div [
        Bulma.title.h1 "AzureTackle - Execute"
        Html.hr []
        Bulma.content [
            Bulma.title.h4 "Query data from Azure Table Storage"
            Html.p [ prop.text "Use AzureTable.execute to query and map entities to your domain types." ]
        ]
    ]

let code =
    """
open AzureTackle

// Define your record type
type MyRecord = {
    PartKey: string
    RowKey: string
    ValidFrom: DateTimeOffset
    ValidTo: DateTimeOffset option
    Exists: bool
    Value: float
    Text: string
}

// Query all entities from a table
let! values =
    AzureTable.withConnectionString ("UseDevelopmentStorage=true", "MyTable")
    |> AzureTable.execute (fun entity ->
        {
            PartKey = entity.PartitionKey
            RowKey = entity.RowKey
            ValidFrom = entity.ReadDateTimeOffset "ValidFrom"
            ValidTo = entity.ReadOptionalDateTimeOffset "ValidTo"
            Exists = entity.ReadBoolean "Exists"
            Value = entity.ReadDouble "Value"
            Text = entity.ReadString "Text"
        })

// Query with filter
let! filtered =
    AzureTable.withConnectionString ("UseDevelopmentStorage=true", "MyTable")
    |> AzureTable.filter (PartKey "myPartition" + Equal "Exists" true)
    |> AzureTable.execute (fun entity ->
        { PartKey = entity.PartitionKey
          RowKey = entity.RowKey
          ValidFrom = entity.ReadDateTimeOffset "ValidFrom"
          ValidTo = entity.ReadOptionalDateTimeOffset "ValidTo"
          Exists = entity.ReadBoolean "Exists"
          Value = entity.ReadDouble "Value"
          Text = entity.ReadString "Text" })
    """

let title = Html.text "AzureTackle"

[<ReactComponent>]
let Execute () =
    Html.div [
        Bulma.content [
            codedView title code ExecuteView
        ]
        fixDocsView "Execute" false
    ]
