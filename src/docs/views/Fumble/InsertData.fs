module InsertData

open Feliz
open Feliz.Bulma
open Shared

let overview =
    Html.div [
        Bulma.title.h1 "AzureTackle - InsertData"
        Bulma.content [
            Bulma.title.h4 "Connect to your database"
            Html.p [ prop.dangerouslySetInnerHTML "Get the connection from the environment" ]
            code """
            open AzureTackle
            let connectionString() = Env.getVar "app_db"""
        ]
        Html.hr []
        Bulma.content [
            Bulma.title.h4 "Insert trade data into database with commandInsert"
            code """
            connectionString()
            |> AzureTackle.connect
            |> AzureTackle.commandInsert<Trades>("Trades,trades)
            |> AzureTackle.insertData trades
            |> function
            | Ok x ->
                printfn "rows affected %A" x.Length
                pass ()
            | otherwise ->
                printfn "error %A" otherwise
                fail () """
        ]
        fixDocsView "InsertData"

    ]

