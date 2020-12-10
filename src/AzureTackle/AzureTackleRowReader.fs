namespace AzureTackle

open System
open System.Collections.Generic
open Microsoft.WindowsAzure.Storage.Table
open Chia.Shared.Ids



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

    let failToRead (column: string) (columnType: string) =
        let availableColumns =
            columnDict.Keys
            |> Seq.map (fun key -> sprintf "[%s]" key)
            |> String.concat ", "

        printfn "columnDict %A" columnDict
        failwithf "Could not read column '%s' as %s. Available columns are %s" column columnType availableColumns

    with

        // member __.int(columnIndex: int): int =
        //     printfn "index %i int" columnIndex
        //     let dict = entity.Properties |> Seq.map (fun keyPair -> keyPair.Key,keyPair.Value) |> Seq.toList
        //     let value = dict.[columnIndex] |> snd
        //     value.Int32Value.Value
        member __.int(column: string): int =
            match columnDict.TryGetValue(column) with
            | true, value -> value.Int32Value.Value
            | false, _ -> failToRead column "int"

        member __.float(column: string): float =
            match columnDict.TryGetValue(column) with
            | true, value -> value.DoubleValue.Value
            | false, _ -> failToRead column "float"

        member __.string(column: string): string =
            match columnDict.TryGetValue(column) with
            | true, value -> value.StringValue
            | false, _ -> failToRead column "string"
        // member __.stringOrNone(column: string): string option =
        //     match columnDict.TryGetValue(column) with
        //     | true, value -> value.StringValue
        //     | false, _ -> failToRead column "string"

        member __.rowKey: SortableRowKey = SortableRowKey entity.RowKey

        member __.partKey: string = entity.PartitionKey
        /// Gets the value of the specified column as a System.DateTime object.
        member __.dateTimeOffset(column: string): DateTimeOffset =
            match columnDict.TryGetValue(column) with
            | true, value -> value.DateTimeOffsetValue.Value
            | false, _ -> failToRead column "dateTimeOffset"
// member __.intOrNone(column: string): int option =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> if entity.IsDBNull(columnIndex) then None else Some(__.int columnIndex)
//     | false, _ -> failToRead column "int"

// member __.intOrNone(columnIndex: int) =
//     if entity.IsDBNull(columnIndex) then None else Some(__.int (columnIndex))

// member __.tinyint(column: string) =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> entity.GetByte(columnIndex)
//     | false, _ -> failToRead column "tinyint"

// member __.tinyintOrNone(column: string) =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> if entity.IsDBNull(columnIndex) then None else Some(entity.GetByte(columnIndex))
//     | false, _ -> failToRead column "tinyint"

// member __.int16(columnIndex: int) =
//     if types.[columnIndex] = "tinyint" then
//         int16 (entity.GetByte(columnIndex))
//     else
//         entity.GetInt16(columnIndex)

// member __.int16(column: string): int16 =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> __.int16 (columnIndex)
//     | false, _ -> failToRead column "int16"

// member __.int16OrNone(column: string): int16 option =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> if entity.IsDBNull(columnIndex) then None else Some(__.int16 (columnIndex))
//     | false, _ -> failToRead column "int16"

// member __.int16OrNone(columnIndex: int) =
//     if entity.IsDBNull(columnIndex) then None else Some(__.int16 (columnIndex))

// member __.int64(columnIndex: int): int64 =
//     if types.[columnIndex] = "tinyint"
//        || types.[columnIndex] = "smallint"
//        || types.[columnIndex] = "int" then
//         Convert.ToInt64(entity.GetValue(columnIndex))
//     else
//         entity.GetInt64(columnIndex)

// member __.int64(column: string): int64 =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> __.int64 (columnIndex)
//     | false, _ -> failToRead column "int64"

// member __.int64OrNone(column: string): int64 option =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> if entity.IsDBNull(columnIndex) then None else Some(__.int64 columnIndex)
//     | false, _ -> failToRead column "int64"

// member __.int64OrNone(columnIndex: int): int64 option =
//     if entity.IsDBNull(columnIndex) then None else Some(__.int64 (columnIndex))
// member __.stringOrNone(column: string): string option =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> if entity.IsDBNull(columnIndex) then None else Some(entity.GetString(columnIndex))
//     | false, _ -> failToRead column "string"

// member __.bool(columnIndex: int) = entity.GetBoolean(columnIndex)

// member __.bool(column: string): bool =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> entity.GetBoolean(columnIndex)
//     | false, _ -> failToRead column "bool"

// member __.boolOrNone(column: string): bool option =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> if entity.IsDBNull(columnIndex) then None else Some(entity.GetBoolean(columnIndex))
//     | false, _ -> failToRead column "bool"

// member __.decimal(columnIndex: int) = entity.GetDecimal(columnIndex)

// member __.decimal(column: string): decimal =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex ->
//         let columnType = types.[columnIndex]
//         if List.contains columnType [ "int"; "int64"; "int16"; "float" ]
//         then Convert.ToDecimal(entity.GetValue(columnIndex))
//         else entity.GetDecimal(columnIndex)
//     | false, _ -> failToRead column "decimal"

// member __.decimalOrNone(column: string): decimal option =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> if entity.IsDBNull(columnIndex) then None else Some(entity.GetDecimal(columnIndex))
//     | false, _ -> failToRead column "decimal"

// member __.double(columnIndex: int) = entity.GetDouble(columnIndex)

// member __.double(column: string): double =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> entity.GetDouble(columnIndex)
//     | false, _ -> failToRead column "double"

// member __.doubleOrNone(column: string): double option =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> if entity.IsDBNull(columnIndex) then None else Some(entity.GetDouble(columnIndex))
//     | false, _ -> failToRead column "double"

// member __.entity = entity

// member __.uniqueidentifier(columnIndex: int) = entity.GetGuid(columnIndex)

// /// Gets the value of the specified column as a globally-unique identifier (GUID).
// member __.uniqueidentifier(column: string): Guid =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> entity.GetGuid(columnIndex)
//     | false, _ -> failToRead column "uniqueidentifier"

// member __.uniqueidentifierOrNone(column: string): Guid option =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> if entity.IsDBNull(columnIndex) then None else Some(entity.GetGuid(columnIndex))
//     | false, _ -> failToRead column "uniqueidentifier"


// /// Gets the value of the specified column as a System.DateTime object.
// member __.dateTimeOffsetOrNone(column: string): DateTimeOffset option =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex ->
//         if entity.IsDBNull(columnIndex)
//         then None
//         else Some(entity.GetDateTimeOffset(columnIndex))
//     | false, _ -> failToRead column "dateTimeOffset"

// member __.dateTime(columnIndex: int) = entity.GetDateTime(columnIndex)

// /// Gets the value of the specified column as a System.DateTime object.
// member __.dateTime(column: string): DateTime =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> entity.GetDateTime(columnIndex)
//     | false, _ -> failToRead column "datetime"

// /// Gets the value of the specified column as a System.DateTime object.
// member __.dateTimeOrNone(column: string): DateTime option =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> if entity.IsDBNull(columnIndex) then None else Some(entity.GetDateTime(columnIndex))
//     | false, _ -> failToRead column "datetime"

// member __.bytes(columnIndex: int) = entity.GetFieldValue<byte []>(columnIndex)

// /// Reads the specified column as `byte[]`
// member __.bytes(column: string): byte [] =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> entity.GetFieldValue<byte []>(columnIndex)
//     | false, _ -> failToRead column "byte[]"

// /// Reads the specified column as `byte[]`
// member __.bytesOrNone(column: string): byte [] option =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex ->
//         if entity.IsDBNull(columnIndex)
//         then None
//         else Some(entity.GetFieldValue<byte []>(columnIndex))
//     | false, _ -> failToRead column "byte[]"

// member __.float(columnIndex: int) = entity.GetFloat(columnIndex)

// /// Gets the value of the specified column as a `System.Single` object.
// member __.float(column: string): float32 =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> entity.GetFloat(columnIndex)
//     | false, _ -> failToRead column "float"

// /// Gets the value of the specified column as a `System.Single` object.
// member __.floatOrNone(column: string): float32 option =
//     match columnDict.TryGetValue(column) with
//     | true, columnIndex -> if entity.IsDBNull(columnIndex) then None else Some(entity.GetFloat(columnIndex))
//     | false, _ -> failToRead column "float"
