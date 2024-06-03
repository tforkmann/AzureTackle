module Docs.Pages.AzureTackle

open Feliz
open Feliz.Bulma
open Feliz.ChartJS
open Docs.SharedView
[<ReactComponent>]
let ChartJSBarChart () =
    ChartJS.bar [
        bar.options [
            option.responsive true
            option.scales [
                scale.x [ axes.stacked true ]
                scale.y [ axes.stacked true ]
            ]
            option.plugins [
                plugin.legend [ legend.position Position.Top ]
                plugin.title [
                    title.display true
                    title.text "Chart.js Bar Chart"
                ]
                plugin.datalabels [
                    datalabels.display true
                    datalabels.align Position.Bottom
                    datalabels.borderRadius 3
                    datalabels.color "red"
                    datalabels.backgroundColor "green"
                    // datalabels.labels [
                    //     labels.value {|color="blue"|}
                    // ]
                    // datalabels.formatter renderCustomLabel
                    ]
            ]
        ]
        bar.data [
            barData.labels [|
                "January"
                "Feburary"
            |]
            barData.datasets [|
                barData.dataset [
                    barDataSet.label "My First Dataset"
                    barDataSet.borderColor "blue"
                    barDataSet.backgroundColor "rgba(53, 162, 235, 0.5)"
                    barDataSet.borderSkipped false
                    barDataSet.borderWidth 2
                    barDataSet.borderRadius 50
                    barDataSet.data [| "1"; "2" |]
                ]
                barData.dataset [
                    barDataSet.label "My Second Dataset"
                    barDataSet.borderColor "green"
                    barDataSet.backgroundColor "rgba(53, 162, 235, 0.5)"
                    barDataSet.borderSkipped false
                    barDataSet.borderWidth 2
                    barDataSet.borderRadius 50
                    barDataSet.data [| "1"; "2" |]
                ]
            |]
        ]
    ]

let QueryTable =
    Bulma.title.h1 "AzureTackle - QueryTable"
        Html.hr []
        Bulma.content [
            Bulma.title.h4 "Connect to your database"
            Html.p [ prop.dangerouslySetInnerHTML "Get the connection from the environment" ]
            code """
            open AzureTackle
            let connectionString() = Env.getVar "app_db"""
        ]

let code =
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

let title = Html.text "AzureTackle"

[<ReactComponent>]
let BarChartView () =
    Html.div [
        Bulma.content [
            codedView title code ChartJSChart
        ]
        fixDocsView "AzureTackle" false
    ]
