namespace AzureTackle

open System
open System.Collections.Generic
open Azure.Data.Tables

type Props =
    | FLT
    | DEC
    | INT32
    | BIGINT
    | TXT
    | DT
    | DTO
    | BOOL
    | BINARY

type AzureTackleRowEntity(entity: TableEntity) =

    let failToRead (column: string) (columnType: Props) (exn: Exception) =
        let columnTypStr =
            match columnType with
            | FLT -> "float"
            | DEC -> "decimal"
            | INT32 -> "int32"
            | TXT -> "string"
            | DTO -> "datetimeoffset"
            | BIGINT -> "int64"
            | BOOL -> "bool"
            | DT -> "datetime"
            | BINARY -> "binary"

        failwithf
            "Could not read property '%s' as %s. Message: %s"
            column
            columnTypStr
            exn.Message

    let getProperty (propName: string) (columnType: Props) (entity: TableEntity) =

        try
            match entity.TryGetValue propName with
            | false, _ -> failwithf "Property %s not found in entity" propName
            | true, v -> v
        with exn -> failToRead propName columnType exn

    let getOptionalProperty (propName: string) (columnType: Props) (entity: TableEntity) =

        try
            match entity.TryGetValue propName with
            | true, v -> Some v
            | _ -> None
        with exn -> failToRead propName columnType exn
    with
        member __.rowKey = entity.RowKey

        member __.eTag: Azure.ETag = entity.ETag

        member __.partKey: string = entity.PartitionKey

        member __.timeStamp: DateTimeOffset Nullable = entity.Timestamp

        member __.int(column: string)  =
            try
                let prop = getProperty column INT32 entity
                (box prop) :?> int
            with exn ->
                failwithf
                    "Could not get INT32 value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.intOrNone(column: string): int option =
            try
                getOptionalProperty column INT32 entity
                |> Option.map (fun prop -> (box prop) :?> int)
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
                (box prop) :?> float
            with exn ->
                failwithf
                    "Could not get float value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.floatOrNone(column: string): float option =
            try
                getOptionalProperty column FLT entity
                |> Option.map (fun prop -> (box prop) :?> float)
            with exn ->
                failwithf
                    "Could not get float value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.decimal(column: string): decimal =
            try
                let prop = getProperty column DEC entity
                let e = (box prop) :?> float
                e |> decimal
            with exn ->
                failwithf
                    "Could not get decimal value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.decimalOrNone(column: string): decimal option =
            try
                getOptionalProperty column DEC entity
                |> Option.map (fun prop -> (box prop) :?> float |> decimal)
            with exn ->
                failwithf
                    "Could not get decimal value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.string(column: string): string =
            try
                let prop = getProperty column TXT entity
                (box prop) :?> string
            with exn ->
                failwithf
                    "Could not get string value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.stringOrNone(column: string): string option =
            try
                getOptionalProperty column TXT entity
                |> Option.map (fun prop -> (box prop) :?> string)
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
                (box prop) :?> DateTime
            with exn ->
                failwithf
                    "Could not get datetime value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.dateTimeOrNone(column: string): DateTime option =
            try
                getOptionalProperty column DTO entity
                |> Option.map (fun prop -> (box prop) :?> DateTime)
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
                (box prop) :?> DateTimeOffset
            with exn ->
                failwithf
                    "Could not get dateTimeOffset value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.dateTimeOffsetOrNone(column: string): DateTimeOffset option =
            try
                getOptionalProperty column DTO entity
                |> Option.map (fun prop -> (box prop) :?> DateTimeOffset)
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
                (box prop) :?> int64
            with exn ->
                failwithf
                    "Could not get dateTimeOffset value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.bigIntOrNone(column: string): int64 option =
            try
                getOptionalProperty column BIGINT entity
                |> Option.map (fun prop -> (box prop) :?> int64)
            with exn ->
                failwithf
                    "Could not get dateTimeOffset value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.bool(column: string): bool =
            try
                let prop = getProperty column BOOL entity
                (box prop) :?> bool
            with exn ->
                failwithf
                    "Could not get bool value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.boolOrNone(column: string): bool option =
            try
                getOptionalProperty column BOOL entity
                |> Option.map (fun prop -> (box prop) :?> bool)
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
                (box prop) :?> byte array
            with exn ->
                failwithf
                    "Could not get binary value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message

        member __.binaryOrNone(column: string): byte array option =
            try
                getOptionalProperty column BINARY entity
                |> Option.map (fun prop -> (box prop) :?> byte array)
            with exn ->
                failwithf
                    "Could not get binary value of property %s for entity %s %s. Message: %s"
                    column
                    entity.PartitionKey
                    entity.RowKey
                    exn.Message
