module ParameterizedQuery

open Feliz
open Feliz.Bulma
open Shared
let overview =
    Html.div [
        Bulma.title.h1 [
            Html.text "AzureTackle - Docs are WIP"
            Html.a [
                prop.href "https://www.nuget.org/packages/AzureTackle/"
                prop.children [
                    Html.img [
                        prop.src "https://img.shields.io/nuget/v/AzureTackle.svg?style=flat"
                    ]
                ]
            ]
        ]
        Bulma.subtitle.h2 [
            Html.text "Thin F# API for AzureTackle for easy data access to AzureTackle database with functional seasoning on top"
        ]
        Html.hr []
        Bulma.content [
            Html.p "dotnet add package AzureTackle"
            Html.p ".paket/paket.exe add AzureTackle --project path/to/project.fsproj"
        ]
        fixDocsView "ParameterizedQuery"

    ]
