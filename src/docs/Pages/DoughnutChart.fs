module Docs.Pages.DoughnutChart

open Feliz
open Feliz.Bulma
open Feliz.ChartJS
open Docs.SharedView

[<ReactComponent>]
let ChartJSDoughnutChart () =
    ChartJS.doughnut [
        doughnut.options [
            option.responsive true
            option.plugins [
                plugin.legend [ legend.position Position.Top ]
                plugin.title [
                    title.display true
                    title.text "Chart.js Doughnut Chart"
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
        doughnut.data [
            doughnutData.labels [|
                "Red"
                "Blue"
                "Yellow"
                "Green"
                "Purple"
                "Orange"
            |]
            doughnutData.datasets [|
                doughnutData.dataset [
                    doughnutDataSet.label "# of Votes"
                    doughnutDataSet.borderColor [|
                        "rgba(255, 99, 132, 1)"
                        "rgba(54, 162, 235, 1)"
                        "rgba(255, 206, 86, 1)"
                        "rgba(75, 192, 192, 1)"
                        "rgba(153, 102, 255, 1)"
                        "rgba(255, 159, 64, 1)"
                    |]
                    doughnutDataSet.borderWidth 1
                    doughnutDataSet.backgroundColor [|
                        "rgba(255, 99, 132, 0.2)"
                        "rgba(54, 162, 235, 0.2)"
                        "rgba(255, 206, 86, 0.2)"
                        "rgba(75, 192, 192, 0.2)"
                        "rgba(153, 102, 255, 0.2)"
                        "rgba(255, 159, 64, 0.2)"
                    |]
                    doughnutDataSet.data [|
                        12
                        19
                        3
                        5
                        2
                        3
                    |]
                    doughnutDataSet.datalabels [|
                        datalabel.anchor "end"
                    |]
                ]
            |]
        ]
    ]

let ChartJSChart =
    Html.div [
        prop.style [ style.height 800 ]
        prop.children [
            ChartJSDoughnutChart()
        ]
    ]

let code =
    """
    ChartJS.doughnut [
        doughnut.options [
            option.responsive true
            option.plugins [
                plugin.legend [ legend.position Top ]
                plugin.title [
                    title.display true
                    title.text "Chart.js Doughnut Chart"
                ]
                plugin.datalabels [
                    datalabels.display true
                    datalabels.align Bottom
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
        doughnut.data [
            doughnutData.labels [|
                "Red"
                "Blue"
                "Yellow"
                "Green"
                "Purple"
                "Orange"
            |]
            doughnutData.datasets [|
                doughnutData.dataset [
                    doughnutDataSet.label "# of Votes"
                    doughnutDataSet.borderColor [|
                        "rgba(255, 99, 132, 1)"
                        "rgba(54, 162, 235, 1)"
                        "rgba(255, 206, 86, 1)"
                        "rgba(75, 192, 192, 1)"
                        "rgba(153, 102, 255, 1)"
                        "rgba(255, 159, 64, 1)"
                    |]
                    doughnutDataSet.borderWidth 1
                    doughnutDataSet.backgroundColor [|
                        "rgba(255, 99, 132, 0.2)"
                        "rgba(54, 162, 235, 0.2)"
                        "rgba(255, 206, 86, 0.2)"
                        "rgba(75, 192, 192, 0.2)"
                        "rgba(153, 102, 255, 0.2)"
                        "rgba(255, 159, 64, 0.2)"
                    |]
                    doughnutDataSet.data [|
                        12
                        19
                        3
                        5
                        2
                        3
                    |]
                    doughnutDataSet.datalabels [|
                        datalabel.anchor "end"
                    |]
                ]
            |]
        ]
    ]
    """

let title = Html.text "Doughnut Chart"

[<ReactComponent>]
let DoughnutChartView () =
    Html.div [
        Bulma.content [
            codedView title code ChartJSChart
        ]
        fixDocsView "LineChart" false
    ]