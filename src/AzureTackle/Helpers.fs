namespace AzureTackle

module EnviromentHelper =
    open System

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
        tryGetEnv "status" |> matchEnvironVarToStage
