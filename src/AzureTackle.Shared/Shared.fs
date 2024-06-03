namespace AzureTackle

[<AutoOpen>]
module Shared =
    open System

    module SortedRowKey =
        let toSortedRowKey (dateTime: DateTime) =
            String.Format("{0:D19}", DateTime.MaxValue.Ticks - dateTime.Ticks)

        let toDate (ticks) =
            DateTime(DateTime.MaxValue.Ticks - int64 ticks)
