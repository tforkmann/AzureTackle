module Tests

open Expecto
open System
open AzureTackle
open Azure.Data.Tables

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

/// Creates default test data with a specific date
let createTestData (date: DateTime) = {
    PartKey = "PartKey"
    RowKey = date |> SortedRowKey.toSortedRowKey
    ValidFrom = date |> DateTimeOffset
    ValidTo = None
    Value = 0.2
    Exists = true
    ValueDecimal = 0.2m
    Text = "isWorking"
}

/// Maps a TableEntity to TestData
let mapToTestData (entity: TableEntity) = {
    PartKey = entity.PartitionKey
    RowKey = entity.RowKey
    ValidFrom = entity.ReadDateTimeOffset "ValidFrom"
    ValidTo = entity.ReadOptionalDateTimeOffset "ValidTo"
    Exists = entity.ReadBoolean "Exists"
    Value = entity.ReadDouble "Value"
    ValueDecimal = entity.ReadDecimal "ValueDecimal"
    Text = entity.ReadString "Text"
}

/// Upserts test data to the table
let upsertTestData testData =
    tableProps
    |> AzureTable.upsertInline (testData.PartKey, testData.RowKey) (fun set ->
        set.Add("ValidFrom", testData.ValidFrom)
        set.Add("ValidTo", testData.ValidTo)
        set.Add("Value", testData.Value)
        set.Add("ValueDecimal", testData.ValueDecimal)
        set.Add("Exists", testData.Exists)
        set.Add("Text", testData.Text)
        set)

/// Converts TestData to TableEntity
let toTableEntity (data: TestData) =
    TableEntity(data.PartKey, data.RowKey)
        .Append("ValidFrom", data.ValidFrom)
        .AppendOptional("ValidTo", data.ValidTo)
        .Append("Exists", data.Exists)
        .Append("Text", data.Text)
        .Append("Value", data.Value)
        .Append("ValueDecimal", data.ValueDecimal)

[<Tests>]
let azureTackleTests =
    testList "AzureTackle" [
        testList "Query Operations" [
            testTask "Insert and read data with filter" {
                let testData = createTestData (DateTime(2024, 1, 1))

                do! upsertTestData testData

                let! values =
                    tableProps
                    |> AzureTable.filter (RowKey testData.RowKey)
                    |> AzureTable.execute mapToTestData

                let result = values |> Array.tryHead
                Expect.equal result (Some testData) "Inserted data should match read data"
            }

            testTask "Insert and read data without filter" {
                let testData = createTestData (DateTime(2024, 1, 1))

                do! upsertTestData testData

                let! values =
                    tableProps
                    |> AzureTable.execute mapToTestData

                let result = values |> Array.tryHead
                Expect.equal result (Some testData) "Inserted data should match read data"
            }

            testTask "Read timestamp from table" {
                let testData = createTestData (DateTime(2024, 1, 1))

                do! upsertTestData testData

                let! timestamps =
                    tableProps
                    |> AzureTable.filter (RowKey testData.RowKey)
                    |> AzureTable.execute (fun read -> read.Timestamp)

                let hasTimestamp = timestamps |> Array.tryHead |> Option.isSome
                Expect.isTrue hasTimestamp "Timestamp should be present"
            }
        ]

        testList "Batch Operations" [
            testTask "Insert batch and read timestamp" {
                let testData = [| createTestData (DateTime(2024, 1, 1)) |]
                let entities = testData |> Array.map toTableEntity

                do! tableProps |> AzureTable.upsertBatch entities

                let! timestamps =
                    tableProps
                    |> AzureTable.filter (RowKey testData.[0].RowKey)
                    |> AzureTable.execute (fun read -> read.Timestamp)

                let hasTimestamp = timestamps |> Array.tryHead |> Option.isSome
                Expect.isTrue hasTimestamp "Timestamp should be present"
            }

            testTask "Insert batch and read first page" {
                let testData = [| createTestData (DateTime(2024, 1, 1)) |]
                let entities = testData |> Array.map toTableEntity

                do! tableProps |> AzureTable.upsertBatch entities

                let! timestamps =
                    tableProps
                    |> AzureTable.filter (RowKey testData.[0].RowKey)
                    |> AzureTable.executeFirstPages 1 (fun read -> read.Timestamp)

                let hasTimestamp = timestamps |> Array.tryHead |> Option.isSome
                Expect.isTrue hasTimestamp "Timestamp should be present"
            }
        ]
    ]

let config = {
    defaultConfig with
        runInParallel = false
}

[<EntryPoint>]
let main argv = runTestsInAssemblyWithCLIArgs [] argv
