module Docs.Router

open Browser.Types
open Feliz.Router
open Fable.Core.JsInterop

type Page =
    | AzureTackle
    | QueryTable
    | HandleNullValues
    | ProvidingDefaultValues
    | ParameterizedQuery
    | InsertData

[<RequireQualifiedAccess>]
module Page =
    let defaultPage = AzureTackle

    let parseFromUrlSegments =
        function
        | [ "" ] -> AzureTackle
        | [ "querytable" ] -> QueryTable
        | [ "handlenullvalues" ] -> HandleNullValues
        | [ "providingdefaultvalues" ] -> ProvidingDefaultValues
        | [ "parameterizedquery" ] -> ParameterizedQuery
        | [ "insertdata" ] -> InsertData
        | _ -> defaultPage

    let noQueryString segments : string list * (string * string) list = segments, []

    let toUrlSegments =
        function
        | AzureTackle -> [] |> noQueryString
        | QueryTable -> ["querytable"] |> noQueryString
        | HandleNullValues -> ["handlenullvalues"] |> noQueryString
        | ProvidingDefaultValues -> ["providingdefaultvalues"] |> noQueryString
        | ParameterizedQuery -> ["parameterizedquery"] |> noQueryString
        | InsertData -> [ "insertdata" ] |> noQueryString

[<RequireQualifiedAccess>]
module Router =
    let goToUrl (e: MouseEvent) =
        e.preventDefault ()
        let href: string = !!e.currentTarget?attributes?href?value
        Router.navigate href

    let navigatePage (p: Page) =
        p |> Page.toUrlSegments |> Router.navigate

[<RequireQualifiedAccess>]
module Cmd =
    let navigatePage (p: Page) = p |> Page.toUrlSegments |> Cmd.navigate
