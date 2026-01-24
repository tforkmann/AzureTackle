module Docs.Pages.Upsert

open Feliz
open Feliz.Bulma
open Docs.SharedView

let UpsertView =
    Html.div [
        Bulma.title.h1 "AzureTackle - Upsert"
        Html.hr []
        Bulma.content [
            Bulma.title.h4 "Insert or update entities"
            Html.p [ prop.text "Use AzureTable.upsert or upsertInline to insert or update entities in Azure Table Storage." ]
        ]
    ]

let code =
    """
open AzureTackle
open Azure.Data.Tables

// Single upsert with TableEntity
let entity = TableEntity("myPartition", "myRow")
entity.Add("Name", "John")
entity.Add("Age", 30)

do! AzureTable.withConnectionString ("UseDevelopmentStorage=true", "Users")
    |> AzureTable.upsert entity

// Fluent upsert with Append
let entity =
    TableEntity("myPartition", "myRow")
        .Append("Name", "John")
        .Append("Age", 30)
        .Append("Active", true)
        .AppendOptional("Email", Some "john@example.com")

do! AzureTable.withConnectionString ("UseDevelopmentStorage=true", "Users")
    |> AzureTable.upsert entity

// Inline upsert (creates entity for you)
do! AzureTable.withConnectionString ("UseDevelopmentStorage=true", "Users")
    |> AzureTable.upsertInline ("myPartition", "myRow") (fun e ->
        e.Append("Name", "Jane")
         .Append("Age", 25)
         .Append("Active", true))

// Batch upsert (up to 100 entities per partition)
let entities = [|
    TableEntity("pk1", "rk1").Append("Value", 1)
    TableEntity("pk1", "rk2").Append("Value", 2)
    TableEntity("pk1", "rk3").Append("Value", 3)
|]

do! AzureTable.withConnectionString ("UseDevelopmentStorage=true", "Data")
    |> AzureTable.upsertBatch entities
    """

let title = Html.text "AzureTackle"

[<ReactComponent>]
let Upsert () =
    Html.div [
        Bulma.content [
            codedView title code UpsertView
        ]
        fixDocsView "Upsert" false
    ]
