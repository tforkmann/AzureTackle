namespace AzureTackle

open System
open System.Collections.Generic
open Microsoft.Azure.Cosmos.Table

type Props =
    | FLT
    | INT32
    | BIGINT
    | TXT
    | DT
    | DTO
    | BOOL
    | BINARY

type AzureTackleRowEntity(entity: DynamicTableEntity) =
    let columnDict = Dictionary<string, EntityProperty>()

    do

        entity.Properties
        |> Seq.iter (fun keyPair -> columnDict.Add(keyPair.Key, keyPair.Value))

    let failToRead (column: string) (columnType: Props) (exn: Exception) =
        let availableColumns =
            columnDict.Keys
            |> Seq.map (fun key -> sprintf "[%s]" key)
            |> String.concat ", "

        let columnTypStr =
            match columnType with
            | FLT -> "float"
            | INT32 -> "int32"
            | TXT -> "string"
            | DTO -> "datetimeoffset"
            | BIGINT -> "int64"
            | BOOL -> "bool"
            | DT -> "datetime"
            | BINARY -> "binary"

        failwithf
            "Could not read property '%s' as %s. Available columns are %s. Message: %s"
            column
            columnTypStr
            availableColumns
            exn.Message

    let getProperty (propName: string) (columnType: Props) (entity: DynamicTableEntity) =
        let availableColumns =
            columnDict.Keys
            |> Seq.map (fun key -> sprintf "[%s]" key)
            |> String.concat ", "

        try
            entity.Properties.[propName]
        with exn -> failToRead availableColumns columnType exn

    let getOptionalProperty (propName: string) (entity: DynamicTableEntity) =
        match entity.Properties.TryGetValue propName with
        | true, v -> Some v
        | _ -> None
    with
        member __.rowKey: RowKey = RowKey entity.RowKey

        member __.partKey: string = entity.PartitionKey

        member __.timeStamp: DateTimeOffset = entity.Timestamp

        member __.int(column: string): int =
            try
                let prop = getProperty column INT32 entity
                prop.Int32Value.Value
            with exn ->
                failwithf
                    "Could not get INT32 value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.intOrNone(column: string): int option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.Int32Value.Value)
            with exn ->
                failwithf
                    "Could not get INT32 value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.float(column: string): float =
            try
                let prop = getProperty column FLT entity
                prop.DoubleValue.Value
            with exn ->
                failwithf
                    "Could not get float value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.floatOrNone(column: string): float option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.DoubleValue.Value)
            with exn ->
                failwithf
                    "Could not get float value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.string(column: string): string =
            try
                let prop = getProperty column TXT entity
                prop.StringValue
            with exn ->
                failwithf
                    "Could not get string value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.stringOrNone(column: string): string option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.StringValue)
            with exn ->
                failwithf
                    "Could not get string value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.dateTime(column: string): DateTime =
            try
                let prop = getProperty column DTO entity
                prop.DateTime.Value
            with exn ->
                failwithf
                    "Could not get datetime value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.dateTimeOrNone(column: string): DateTime option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.DateTime.Value)
            with exn ->
                failwithf
                    "Could not get datetime value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.dateTimeOffset(column: string): DateTimeOffset =
            try
                let prop = getProperty column DTO entity
                prop.DateTimeOffsetValue.Value
            with exn ->
                failwithf
                    "Could not get dateTimeOffset value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.dateTimeOffsetOrNone(column: string): DateTimeOffset option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.DateTimeOffsetValue.Value)
            with exn ->
                failwithf
                    "Could not get dateTimeOffset value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.bigInt(column: string): int64 =
            try
                let prop = getProperty column BIGINT entity
                prop.Int64Value.Value
            with exn ->
                failwithf
                    "Could not get dateTimeOffset value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.bigIntOrNone(column: string): int64 option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.Int64Value.Value)
            with exn ->
                failwithf
                    "Could not get dateTimeOffset value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.bool(column: string): bool =
            try
                let prop = getProperty column BIGINT entity
                prop.BooleanValue.Value
            with exn ->
                failwithf
                    "Could not get bool value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.boolOrNone(column: string): bool option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.BooleanValue.Value)
            with exn ->
                failwithf
                    "Could not get bool value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.binary(column: string): byte array =
            try
                let prop = getProperty column BINARY entity
                prop.BinaryValue
            with exn ->
                failwithf
                    "Could not get binary value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.binaryOrNone(column: string): byte array option =
            try
                getOptionalProperty column entity
                |> Option.map (fun prop -> prop.BinaryValue)
            with exn ->
                failwithf
                    "Could not get binary value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

type AzureTackleSetEntity(partKey, rowKey: string) =
    let entity = DynamicTableEntity(partKey, rowKey)

    member __.returnEntity = entity

    member __.string (propName: string) (value: string) =
        entity.Properties.[propName] <- EntityProperty.GeneratePropertyForString value

    member this.stringOrNone (propName: string) (value: string option) =
        match value with
        | Some x -> this.string propName x
        | None -> ()

    member __.int (propName: string) (value: int) =
        entity.Properties.[propName] <- EntityProperty.GeneratePropertyForInt value
        ()

    member this.intOrNone (propName: string) (value: int option) =
        match value with
        | Some x -> this.int propName x
        | None -> ()

    member __.float (propName: string) (value: float) =
        entity.Properties.[propName] <- EntityProperty.GeneratePropertyForDouble value

    member this.floatOrNone (propName: string) (value: float option) =
        match value with
        | Some x -> this.float propName x
        | None -> ()

    member __.dateTimeOffset (propName: string) (value: DateTimeOffset) =
        entity.Properties.[propName] <- EntityProperty.GeneratePropertyForDateTimeOffset value

    member this.dateTimeOffsetOrNone (propName: string) (value: DateTimeOffset option) =
        match value with
        | Some x -> this.dateTimeOffset propName x
        | None -> ()

    member __.bool (propName: string) (value: bool) =
        entity.Properties.[propName] <- EntityProperty.GeneratePropertyForBool value

    member this.boolOrNone (propName: string) (value: bool option) =
        match value with
        | Some x -> this.bool propName x
        | None -> ()
