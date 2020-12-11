namespace AzureTackle

open System
open System.Collections.Generic
open Microsoft.WindowsAzure.Storage.Table
open Chia.Shared.Ids
type Props =
    | Float
    | INT32
    | BIGINT
    | STRING
    | DATETIMEOFFSET
    | BOOL
type AzureTackleRowEntity(entity: DynamicTableEntity) =
    // let columnDict = Dictionary<string, int>()
    let columnDict = Dictionary<string, EntityProperty>()

    do
        // Populate the names of the columns into a dictionary
        // such that each read doesn't need to loop through all columns
        // for fieldIndex in [ 0 .. entity.Properties.Count - 1 ] do
        //     let dict = entity.Properties |> Seq.map (fun keyPair -> keyPair.Key,keyPair.Value) |> Seq.toList
        //     let columnName = dict.[fieldIndex] |> fst
        //     let columnType = dict.[fieldIndex] |> snd |> string
        //     // printfn "count %A" entity.Properties.Count
        //     // printfn "columnName %A" columnName
        //     // printfn "columnType %A" columnType
        //     columnDict.Add(columnName, fieldIndex)
        //     columnTypes.Add(columnName,columnType)
        //     types.Add(columnType)
        //  anually add RowKey and PartitionKey

        entity.Properties
        |> Seq.iter (fun keyPair -> columnDict.Add(keyPair.Key, keyPair.Value))

    let failToRead (column: string) (columnType: Props) (exn:Exception) =
        let availableColumns =
            columnDict.Keys
            |> Seq.map (fun key -> sprintf "[%s]" key)
            |> String.concat ", "
        let columnTypStr =
            match columnType with
            | Float -> "float"
            | INT32 -> "int32"
            | STRING -> "string"
            | DATETIMEOFFSET -> "datetimeoffset"
            | BIGINT -> "int64"
            | BOOL -> "bool"
        failwithf "Could not read property '%s' as %s. Available columns are %s. Message: %s" column columnTypStr availableColumns  exn.Message
    let getProperty (propName : string) (columnType:Props) (entity : DynamicTableEntity) =
        let availableColumns =
            columnDict.Keys
            |> Seq.map (fun key -> sprintf "[%s]" key)
            |> String.concat ", "
        try
            entity.Properties.[propName]
        with exn ->
            failToRead availableColumns columnType exn

    let getOptionalProperty (propName : string) (entity : DynamicTableEntity) =
        match entity.Properties.TryGetValue propName with
        | true, v -> Some v
        | _ -> None

    with
        member __.rowKey: SortableRowKey = SortableRowKey entity.RowKey

        member __.partKey: string = entity.PartitionKey
        member __.int(column: string): int =
            try
                let prop = getProperty column INT32 entity
                prop.Int32Value.Value
            with exn ->
                failwithf
                    "Could not get INT32 value of property %s for entity %s %s. Message: %s"
                    column entity.PartitionKey entity.RowKey exn.Message

        member __.float(column: string): float =
            try
                let prop = getProperty column Float entity
                prop.DoubleValue.Value
            with exn ->
                failwithf
                    "Could not get Double value of property %s for entity %s %s. Message: %s"
                    column entity.PartitionKey entity.RowKey exn.Message
        member __.floatOrNone(column: string): float option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.DoubleValue.Value)
            with exn ->
                failwithf
                    "Could not get string value of property %s for entity %s %s. Message: %s"
                    column entity.PartitionKey entity.RowKey exn.Message
        member __.string(column: string): string =
            try
                let prop = getProperty column STRING entity
                prop.StringValue
            with exn ->
                failwithf
                    "Could not get String value of property %s for entity %s %s. Message: %s"
                    column entity.PartitionKey entity.RowKey exn.Message
        member __.stringOrNone(column: string): string option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.StringValue)
            with exn ->
                failwithf
                    "Could not get string value of property %s for entity %s %s. Message: %s"
                    column entity.PartitionKey entity.RowKey exn.Message
        member __.dateTimeOffset(column: string): DateTimeOffset =
            try
                let prop = getProperty column DATETIMEOFFSET entity
                prop.DateTimeOffsetValue.Value
            with exn ->
                failwithf
                    "Could not get DateTimeOffset value of property %s for entity %s %s. Message: %s"
                    column entity.PartitionKey entity.RowKey exn.Message
        member __.dateTimeOffsetOrNone(column: string): DateTimeOffset option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.DateTimeOffsetValue.Value)
            with exn ->
                failwithf
                    "Could not get dateTimeOffset value of property %s for entity %s %s. Message: %s"
                    column entity.PartitionKey entity.RowKey exn.Message
        member __.bigInt(column: string): int64 =
            try
                let prop = getProperty column BIGINT entity
                prop.Int64Value.Value
            with exn ->
                failwithf
                    "Could not get DateTimeOffset value of property %s for entity %s %s. Message: %s"
                    column entity.PartitionKey entity.RowKey exn.Message
        member __.bigIntOrNone(column: string): int64 option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.Int64Value.Value)
            with exn ->
                failwithf
                    "Could not get dateTimeOffset value of property %s for entity %s %s. Message: %s"
                    column entity.PartitionKey entity.RowKey exn.Message
        member __.bool(column: string): bool =
            try
                let prop = getProperty column BIGINT entity
                prop.BooleanValue.Value
            with exn ->
                failwithf
                    "Could not get bool value of property %s for entity %s %s. Message: %s"
                    column entity.PartitionKey entity.RowKey exn.Message
        member __.boolOrNone(column: string): bool option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.BooleanValue.Value)
            with exn ->
                failwithf
                    "Could not get bool value of property %s for entity %s %s. Message: %s"
                    column entity.PartitionKey entity.RowKey exn.Message

