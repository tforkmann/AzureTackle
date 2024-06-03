module Docs.Pages.QueryTable

open Feliz
open Feliz.Bulma
open Docs.SharedView

let QueryTableView =
    Html.div [
        Bulma.title.h1 "AzureTackle - QueryTable"
        Html.hr []
        Bulma.content [
            Bulma.title.h4 "Connect to your database"
            Html.p [ prop.dangerouslySetInnerHTML "Get the connection from the environment" ]
            linedMockupCode """
            open AzureTackle
            let connectionString() = Env.getVar "app_db"""
        ]
    ]

let code =
    """
            let data =
                connectionString()
                |> AzureTackle.connect
                |> AzureTackle.query
                "
                SELECT * FROM Trades
                ORDER BY timestamp desc
                "
                |> AzureTackle.execute (fun read ->
                    { Symbol = read.string "Symbol"
                      Timestamp = read.dateTime "Timestamp"
                      Price = read.double "Price"
                      TradeSize = read.double "TradeSize" })
                |> function
                | Ok x -> x
                | otherwise ->
                    printfn "error %A" otherwise
                    fail () """

let title = Html.text "AzureTackle"

[<ReactComponent>]
let QueryTable () =
    Html.div [
        Bulma.content [
            codedView title code QueryTableView
        ]
        fixDocsView "QueryTable" false
    ]
