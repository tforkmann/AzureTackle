module AzureTackle

open Feliz
open Feliz.Bulma
open Shared
let overview =
    Html.div [
        Bulma.title.h1 [
            Html.text "AzureTackle - Overview"
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
        Bulma.title.h2 "Installation"
        Html.hr []
        Bulma.content [
            code "dotnet add package AzureTackle"
            code ".paket/paket.exe add AzureTackle --project path/to/project.fsproj"
        ]
        fixDocsView "AzureTackle"
    ]

