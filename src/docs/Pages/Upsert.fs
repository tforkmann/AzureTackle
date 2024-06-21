module Docs.Pages.Upsert

open Feliz
open Feliz.Bulma
open Docs.SharedView

let UpsertView =
    Html.div [
        Bulma.title.h1 "AzureTackle - Upsert"
        Html.hr []
        Bulma.content [
            Bulma.title.h4 "Connect to your database"
            Html.p [ prop.dangerouslySetInnerHTML "Get the connection from the environment" ]
            linedMockupCode """
                open AzureTackle
                let connectionString() = Env.getVar "app_db
                """
        ]
    ]

let code =
    """
    let tableProps = AzureTable.withConnectionString ("UseDevelopmentStorage=true", TestTable)
    let mapper = fun (tableEntity :TableEntity) ->
        {
            PartKey = tableEntity.PartitionKey
            RowKey = tableEntity.RowKey
            ValidFrom = tableEntity.ReadDateTimeOffset("ValidFrom")
            ValidTo = tableEntity.ReadOptionalDateTimeOffset("ValidTo")
            Exists = tableEntity.ReadBoolean("Exists")
            Value = tableEntity.GetDouble("Value").Value
            ValueDecimal = tableEntity.ReadDecimal("ValueDecimal")
            Text = tableEntity.GetString("Text")
        }

    let! values =
        tableProps
        |> AzureTable.execute (fun read ->
            {
                PartKey = read.PartitionKey
                RowKey = read.RowKey
                ValidFrom = read.ReadDateTimeOffset("ValidFrom")
                ValidTo = read.ReadOptionalDateTimeOffset "ValidTo"
                Exists = read.ReadBoolean "Exists"
                Value = read.ReadDouble "Value"
                ValueDecimal = read.ReadDecimal "ValueDecimal"
                Text = read.ReadString "Text"
            })
        """

let title = Html.text "AzureTackle"

[<ReactComponent>]
let Upsert () =
    Html.div [
        Bulma.content [
            codedView title code UpsertView
        ]
        fixDocsView "Upsert" false
    ]
