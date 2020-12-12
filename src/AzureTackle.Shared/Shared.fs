namespace AzureTackle

[<AutoOpen>]
module Shared =
    open System
    type RowKey =
        | RowKey of string
        member this.GetValue = (fun (RowKey id) -> id) this

    module RowKey =
        let toRowKey (dateTime: DateTime) =
            String.Format("{0:D19}", DateTime.MaxValue.Ticks - dateTime.Ticks)
            |> RowKey

        let toDate (RowKey ticks) =
            DateTime(DateTime.MaxValue.Ticks - int64 ticks)
