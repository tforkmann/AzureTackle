module QueryTable

open Feliz
open Feliz.Bulma
open Shared
let overview =
    Html.div [
        Bulma.title.h1 "AzureTackle - QueryTable"
        Html.hr []
        Bulma.content [
            Bulma.title.h4 "Connect to your database"
            Html.p [ prop.dangerouslySetInnerHTML "Get the connection from the environment" ]
            code """
            open AzureTackle
            let connectionString() = Env.getVar "app_db"""
        ]
        Html.hr []
        Bulma.content [
            Bulma.title.h4 "Query data from to your database"
            code """
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
        ]
        fixDocsView "QueryTable"

    ]


// type User = { Id: int; Username: string }

// let getUsers() : Result<User list, exn> =
//     connectionString()
//     |> AzureTackle.connect
//     |> AzureTackle.query "SELECT * FROM dbo.[Users]"
//     |> AzureTackle.execute (fun read ->
//         {
//             Id = read.int "user_id"
//             Username = read.string "username"
//         })
// ```
