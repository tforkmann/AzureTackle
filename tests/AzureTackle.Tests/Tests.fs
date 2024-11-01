module Tests

open Expecto
open System
open AzureTackle
open Azure.Data.Tables
open System.Threading

printfn "Starting Tests"

[<Literal>]
let TestTable = "TestTable"

type TestData = {
    PartKey: string
    RowKey: string
    ValidFrom: DateTimeOffset
    ValidTo: DateTimeOffset option
    Exists: bool
    Value: float
    ValueDecimal: decimal
    Text: string
}

let tableProps =
    AzureTable.withConnectionString ("UseDevelopmentStorage=true", TestTable)

[<Tests>]
let simpleTest =
    testList "AzureTackle" [
        testTask "Insert test data to table and read data from the table" {

            let testData = {
                PartKey = "PartKey"
                RowKey = DateTime(2024, 1, 1) |> SortedRowKey.toSortedRowKey
                ValidFrom = DateTime(2024, 1, 1) |> DateTimeOffset
                ValidTo = None
                Value = 0.2
                Exists = true
                ValueDecimal = 0.2m
                Text = "isWorking"
            }

            do!
                tableProps
                |> AzureTable.upsertInline (testData.PartKey, testData.RowKey) (fun set ->
                    set.Add("ValidFrom", testData.ValidFrom)
                    set.Add("ValidTo", testData.ValidTo)
                    set.Add("Value", testData.Value)
                    set.Add("ValueDecimal", testData.ValueDecimal)
                    set.Add("Exists", testData.Exists)
                    set.Add("Text", testData.Text)
                    set)

            let! values =
                tableProps
                |> AzureTable.filter (RowKey testData.RowKey)
                |> AzureTable.execute (fun read -> {
                    PartKey = read.PartitionKey
                    RowKey = read.RowKey
                    ValidFrom = read.ReadDateTimeOffset "ValidFrom"
                    ValidTo = read.ReadOptionalDateTimeOffset "ValidTo"
                    Exists = read.ReadBoolean "Exists"
                    Value = read.ReadDouble "Value"
                    ValueDecimal = read.ReadDecimal "ValueDecimal"
                    Text = read.ReadString "Text"
                })

            let data = values |> Array.tryHead
            Expect.equal data (Some testData) "Insert test data is the same the readed testdata"
        }
        testTask "Insert test data to table and async read data from the table directly" {

            let testData = {
                PartKey = "PartKey"
                RowKey = DateTime(2024, 1, 1) |> SortedRowKey.toSortedRowKey
                ValidFrom = DateTime(2024, 1, 1) |> DateTimeOffset
                ValidTo = None
                Value = 0.2
                ValueDecimal = 0.2m
                Exists = true
                Text = "isWorking"
            }

            do!
                tableProps
                |> AzureTable.upsertInline (testData.PartKey, testData.RowKey) (fun set ->
                    set.Add("ValidFrom", testData.ValidFrom)
                    set.Add("ValidTo", testData.ValidTo)
                    set.Add("Value", testData.Value)
                    set.Add("ValueDecimal", testData.ValueDecimal)
                    set.Add("Exists", testData.Exists)
                    set.Add("Text", testData.Text)
                    set)

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

            let results = values |> Array.tryHead

            Expect.equal results (Some testData) "Insert test data is the same the readed testdata"
        }
        // testTask "Insert test data to table and async read data from the table directly" {

        //     let testData = {
        //         PartKey = "PartKey"
        //         RowKey = DateTime(2024, 1, 1) |> SortedRowKey.toSortedRowKey
        //         Date = DateTime(2024, 1, 1) |> DateTimeOffset
        //         Value = 0.2
        //         ValueDecimal = 0.2m
        //         Exists = true
        //         Text = "isWorking"
        //     }

        //     do!
        //         tableProps
        //         |> AzureTable.upsertInline (testData.PartKey, testData.RowKey) (fun set ->
        //             set.Add("Date", testData.Date)
        //             set.Add("Value", testData.Value)
        //             set.Add("ValueDecimal", testData.ValueDecimal)
        //             set.Add("Exists", testData.Exists)
        //             set.Add("Text", testData.Text)
        //             set)

        //     let mapper (tableEntity :TableEntity)=
        //         {
        //             PartKey = tableEntity.PartitionKey
        //             RowKey = tableEntity.RowKey
        //             Date =  tableEntity.GetDateTimeOffset("Date").Value
        //             Exists = tableEntity.GetBoolean("Exists").Value
        //             Value = tableEntity.GetDouble("Value").Value
        //             ValueDecimal = tableEntity.GetDouble("ValueDecimal").Value |> decimal
        //             Text = tableEntity.GetString("Text")
        //         }

        //     let! values =
        //         tableProps
        //         |> AzureTable.filter (RowKey testData.RowKey)
        //         |> AzureTable.withCancellationToken (CancellationToken.None)
        //         |> AzureTable.withMaxElements 150
        //         |> AzureTable.executeAsyncWithMapper mapper

        //     let results = values |> Array.tryHead

        //     Expect.equal results (Some testData) "Insert test data is the same the readed testdata"
        // }
        // testTask "Insert test data as batch to table and receive exactly one value from the table" {
        //     let rowKey = DateTime(2024, 1, 1) |> SortedRowKey.toSortedRowKey

        //     let testData = {
        //         PartKey = "PartKey"
        //         RowKey = rowKey
        //         ValidFrom = DateTime(2024, 1, 1) |> DateTimeOffset
        //         ValidTo = None
        //         Value = 0.2
        //         ValueDecimal = 0.2m
        //         Exists = true
        //         Text = "isWorking"
        //     }

        //     do!
        //         tableProps
        //         |> AzureTable.upsertInline (testData.PartKey, testData.RowKey) (fun set ->
        //             set.Add("ValidFrom", testData.ValidFrom)
        //             set.Add("ValidTo", testData.ValidTo)
        //             set.Add("Value", testData.Value)
        //             set.Add("ValueDecimal", testData.ValueDecimal)
        //             set.Add("Exists", testData.Exists)
        //             set.Add("Text", testData.Text)
        //             set)


        //     let! value =
        //         tableProps
        //         |> AzureTable.filterReceive ("PartKey", rowKey)
        //         |> AzureTable.tryGetValue rowKey (fun read -> {
        //             PartKey = read.PartitionKey
        //             RowKey = read.RowKey
        //             ValidFrom = read.ReadDateTimeOffset "ValidFrom"
        //             ValidTo = read.ReadOptionalDateTimeOffset "ValidTo"
        //             Exists = read.ReadBoolean "Exists"
        //             Value = read.ReadDouble "Value"
        //             ValueDecimal = read.ReadDecimal "ValueDecimal"
        //             Text = read.ReadString "Text"
        //         })

        //     Expect.equal value (Some testData) "Insert test data is the same the read testdata"
        // }

        // testTask "Insert test data to table and backup and receive exactly one value from the backup table" {
        //     let rowKey = DateTime(2024, 1, 1) |> SortedRowKey.toSortedRowKey

        //     let testData = {
        //         PartKey = "PartKey"
        //         RowKey = rowKey
        //         ValidFrom = DateTime(2024, 1, 1) |> DateTimeOffset
        //         ValidTo = None
        //         Value = 0.2
        //         ValueDecimal = 0.2m
        //         Exists = true
        //         Text = "isWorking"
        //     }

        //     let entity =
        //         TableEntity(testData.PartKey, testData.RowKey)
        //             .Append("ValidFrom", testData.ValidFrom)
        //             .AppendOptional("ValidTo", testData.ValidTo)
        //             .Append("Value", testData.Value)
        //             .Append("ValueDecimal", testData.ValueDecimal)
        //             .Append("Exists", testData.Exists)
        //             .Append("Text", testData.Text)

        //     do! tableProps |> AzureTable.upsert entity

        //     let! value =
        //         tableProps
        //         |> AzureTable.filterReceive ("PartKey", rowKey)
        //         |> AzureTable.receive (fun read -> {
        //             PartKey = read.PartitionKey
        //             RowKey = read.RowKey
        //             ValidFrom = read.ReadDateTimeOffset "ValidFrom"
        //             ValidTo = read.ReadOptionalDateTimeOffset "ValidTo"
        //             Exists = read.ReadBoolean "Exists"
        //             Value = read.ReadDouble "Value"
        //             ValueDecimal = read.ReadDecimal "ValueDecimal"
        //             Text = read.ReadString "Text"
        //         })

        //     Expect.equal value (Some testData) "Insert test data is the same the testdata"
        // }
        testTask "Insert test data to table and read timestamp from the table" {

            let testData = {
                PartKey = "PartKey"
                RowKey = DateTime(2024, 1, 1) |> SortedRowKey.toSortedRowKey
                ValidFrom = DateTime(2024, 1, 1) |> DateTimeOffset
                ValidTo = None
                Value = 0.2
                ValueDecimal = 0.2m
                Exists = true
                Text = "isWorking"
            }

            do!
                tableProps
                |> AzureTable.upsertInline (testData.PartKey, testData.RowKey) (fun set ->
                    set.Add("ValidFrom", testData.ValidFrom)
                    set.Add("ValidTo", testData.ValidTo)
                    set.Add("Value", testData.Value)
                    set.Add("ValueDecimal", testData.ValueDecimal)
                    set.Add("Exists", testData.Exists)
                    set.Add("Text", testData.Text)
                    set)

            let! timeStamps =
                tableProps
                |> AzureTable.filter (RowKey testData.RowKey)
                |> AzureTable.execute (fun read -> read.Timestamp)

            let data = timeStamps |> Array.tryHead |> Option.isSome

            Expect.isTrue data "Timestamp isn't there"
        }
        testTask "Insert test data as batch to table and read timestamp from the table" {

            let testData = [|
                {
                    PartKey = "PartKey"
                    RowKey = DateTime(2024, 1, 1) |> SortedRowKey.toSortedRowKey
                    ValidFrom = DateTime(2024, 1, 1) |> DateTimeOffset
                    ValidTo = None
                    Value = 0.2
                    ValueDecimal = 0.2m
                    Exists = true
                    Text = "isWorking"
                }
            |]

            let entities =
                testData
                |> Array.map (fun d ->
                    TableEntity(d.PartKey, d.RowKey)
                        .Append("ValidFrom", d.ValidFrom)
                        .AppendOptional("ValidTo", d.ValidTo)
                        .Append("Exists", d.Exists)
                        .Append("Text", d.Text)
                        .Append("Value", d.Value)
                        .Append("ValueDecimal", d.ValueDecimal))

            do! tableProps |> AzureTable.upsertBatch entities

            let! timeStamps =
                tableProps
                |> AzureTable.filter (RowKey testData.[0].RowKey)
                |> AzureTable.execute (fun read -> read.Timestamp)

            let data = timeStamps |> Array.tryHead |> Option.isSome

            Expect.isTrue data "Timestamp isn't there"
        }
        testTask "Insert test data as batch to table and read timestamp from the first page of the table" {

            let testData = [|
                {
                    PartKey = "PartKey"
                    RowKey = DateTime(2024, 1, 1) |> SortedRowKey.toSortedRowKey
                    ValidFrom = DateTime(2024, 1, 1) |> DateTimeOffset
                    ValidTo = None
                    Value = 0.2
                    ValueDecimal = 0.2m
                    Exists = true
                    Text = "isWorking"
                }
            |]

            let entities =
                testData
                |> Array.map (fun d ->
                    TableEntity(d.PartKey, d.RowKey)
                        .Append("ValidFrom", d.ValidFrom)
                        .AppendOptional("ValidTo", d.ValidTo)
                        .Append("Exists", d.Exists)
                        .Append("Text", d.Text)
                        .Append("Value", d.Value)
                        .Append("ValueDecimal", d.ValueDecimal))

            do! tableProps |> AzureTable.upsertBatch entities

            let! timeStamps =
                tableProps
                |> AzureTable.filter (RowKey testData.[0].RowKey)
                |> AzureTable.executeFirstPages 1 (fun read -> read.Timestamp)

            let data = timeStamps |> Array.tryHead |> Option.isSome

            Expect.isTrue data "Timestamp isn't there"
        }
        // testTask "Delete test data as batch" {
        //     let! values =
        //         tableProps
        //         |> AzureTable.executeDirect (fun read -> {
        //             PartKey = read.partKey
        //             RowKey = read.rowKey
        //             Date = read.dateTimeOffset "Date"
        //             Exists = read.bool "Exists"
        //             Value = read.float "Value"
        //             ValueDecimal = read.decimal "ValueDecimal"
        //             Text = read.string "Text"
        //         })

        //     do!
        //         tableProps
        //         |> AzureTable.deleteBatch values (fun d ->
        //             let set = TableEntity(d.PartKey, d.RowKey)
        //             set.Add("Date", d.Date)
        //             set.Add("Exists", d.Exists)
        //             set.Add("Text", d.Text)
        //             set.Add("Value", d.Value)
        //             set.Add("ValueDecimal", d.ValueDecimal)
        //             set)

        //     let! values =
        //         tableProps
        //         |> AzureTable.executeDirect (fun read -> {
        //             PartKey = read.partKey
        //             RowKey = read.rowKey
        //             Date = read.dateTimeOffset "Date"
        //             Exists = read.bool "Exists"
        //             Value = read.float "Value"
        //             ValueDecimal = read.decimal "ValueDecimal"
        //             Text = read.string "Text"
        //         })

        //     Expect.isEmpty values "Values should be empty"
        // }
    ]

let config = {
    defaultConfig with
        runInParallel = false
}

[<EntryPoint>]
let main argv = runTestsInAssemblyWithCLIArgs [] argv
