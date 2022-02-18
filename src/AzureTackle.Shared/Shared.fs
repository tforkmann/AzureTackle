namespace AzureTackle

[<AutoOpen>]
module Shared =
    open System

    type RowKey =
        | RowKey of string
        member this.GetValue = (fun (RowKey id) -> id) this

    type Stage =
        | Dev
        | Prod

    module RowKey =
        let toRowKey (dateTime: DateTime) =
            String.Format("{0:D19}", DateTime.MaxValue.Ticks - dateTime.Ticks)
            |> RowKey

        let toDate (RowKey ticks) =
            DateTime(DateTime.MaxValue.Ticks - int64 ticks)
    module EnviromentHelper =
        let tryGetEnv key =
            match Environment.GetEnvironmentVariable key with
            | x when String.IsNullOrWhiteSpace x -> None
            | x -> Some x

        let matchEnvironVarToStage stage =
            match stage with
            | Some "Dev" -> Dev
            | Some "Prod" -> Prod
            | environVar ->
                printfn "unmatched EnvironVar %A, please choose between Dev and Prod " environVar
                failwithf "unmatched EnvironVar %A, please choose between Test and Productive " environVar

        let getDevStatusFromEnv =
            tryGetEnv "status"
            |> matchEnvironVarToStage
