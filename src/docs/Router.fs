module Router

open Feliz.Router

type Page =
    | AzureTackle
    | QueryTable
    | HandleNullValues
    | ProvidingDefaultValues
    | ParameterizedQuery
    | InsertData

let defaultPage = AzureTackle

let parseUrl =
    function
    | [ "" ] -> AzureTackle
    | [ "querytable" ] -> QueryTable
    | [ "handlenullvalues" ] -> HandleNullValues
    | [ "providingdefaultvalues" ] -> ProvidingDefaultValues
    | [ "parameterizedquery" ] -> ParameterizedQuery
    | [ "insertdata" ] -> InsertData
    | _ -> defaultPage

let getHref =
    function
    | AzureTackle -> Router.format ("")
    | QueryTable -> Router.format ("querytable")
    | HandleNullValues -> Router.format ("handlenullvalues")
    | ProvidingDefaultValues -> Router.format ("providingdefaultvalues")
    | ParameterizedQuery -> Router.format ("parameterizedquery")
    | InsertData -> Router.format ("insertdata")
