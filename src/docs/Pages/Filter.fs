module Docs.Pages.Filter

open Feliz
open Feliz.Bulma
open Docs.SharedView

let FilterView =
    Html.div [
        Bulma.title.h1 "AzureTackle - Filter DSL"
        Html.hr []
        Bulma.content [
            Bulma.title.h4 "Build type-safe queries"
            Html.p [ prop.text "AzureTackle provides a composable filter DSL for Azure Table Storage queries." ]
        ]
    ]

let basicCode =
    """
open AzureTackle

// Filter by PartitionKey
let partFilter = PartKey "myPartition"

// Filter by RowKey
let rowFilter = RowKey "myRow"

// Combine with AND (+)
let combined = PartKey "myPartition" + RowKey "myRow"
    """

let operatorsCode =
    """
// AND operator (+)
let andFilter = PartKey "pk" + RowKey "rk"

// OR operator (*)
let orFilter = PartKey "pk1" * PartKey "pk2"

// NOT operator (!!)
let notFilter = !! (RowKey "excluded")

// Complex combinations
let complex =
    (PartKey "users" + Equal "Status" "active")
    * (PartKey "admins" + Equal "Status" "active")
    """

let comparisonCode =
    """
// Equal
let eq = Equal "Status" "active"

// Not Equal
let neq = NOT "Status" "deleted"

// Greater Than
let gt = GreaterThan "Age" 18

// Less Than
let lt = LessThan "Price" 100.0

// Greater Than Or Equal
let gte = GreaterThanOrEqual "Count" 5

// Less Than Or Equal
let lte = LessThanOrEqual "Score" 100
    """

let executeCode =
    """
// Use filter in query
let! results =
    AzureTable.withConnectionString (connectionString, "Users")
    |> AzureTable.filter (PartKey "active" + GreaterThan "Age" 21)
    |> AzureTable.execute (fun e ->
        { Name = e.ReadString "Name"
          Age = e.ReadInt "Age" })
    """

let title = Html.text "Filter DSL"

[<ReactComponent>]
let Filter () =
    Html.div [
        Bulma.content [
            codedView (Html.text "Basic Filters") basicCode FilterView
        ]
        Bulma.content [
            codedNoExampleView (Html.text "Operators: AND (+), OR (*), NOT (!!)") operatorsCode
        ]
        Bulma.content [
            codedNoExampleView (Html.text "Comparison Functions") comparisonCode
        ]
        Bulma.content [
            codedNoExampleView (Html.text "Execute with Filter") executeCode
        ]
        fixDocsView "Filter" false
    ]
