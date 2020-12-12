module Tests

open Expecto
open System
open Chia.Infrastructure
open Chia.CreateTable
open Chia.TableStorage
open AzureTackle
open Chia.InitBuilder
open Chia.Shared.Config
open Chia.Shared.Logging
open Microsoft.WindowsAzure.Storage.Table

let fileWriterConfig =
    initWriter {
        devStatus Development
        companyInitials "dp"
        projectName "TestChia"
        devOption (Azure "aiKey")
    }

printfn "Starting Tests"

let connectionString =
    "DefaultEndpointsProtocol=https;AccountName=dptestchiadev;AccountKey=Vd64M6dPKvW/yRQ32xvptAJWV0GGlaeZJxkArJ8ZGJEKWx/aZH5KAxMMPHJkeL/gMiJb65krq8S5yRxCK67p8w==;BlobEndpoint=https://dptestchiadev.blob.core.windows.net/;QueueEndpoint=https://dptestchiadev.queue.core.windows.net/;TableEndpoint=https://dptestchiadev.table.core.windows.net/;FileEndpoint=https://dptestchiadev.file.core.windows.net/;"

let azAccount =
    azConnectionExisting fileWriterConfig connectionString

[<Literal>]
let TestTable = "TestTable"

type TestData =
    { PartKey: string
      RowKey: RowKey
      Date: DateTimeOffset
      Exists: bool
      Value: float
      Text: string }

[<Tests>]
let simpleTest =
    testList
        "AzureTackle"
        [ testTask "Insert test data as batch to table and read data from the table" {
              let testTable = getTable TestTable azAccount

              let testData =
                  [| { PartKey = "PartKey"
                       RowKey = DateTime.UtcNow |> RowKey.toRowKey
                       Date = DateTime.UtcNow |> System.DateTimeOffset
                       Value = 0.2
                       Exists = true
                       Text = "isWorking" } |]

              let tableMapper (testData: TestData) =
                  DynamicTableEntity(testData.PartKey, testData.RowKey.GetValue)
                  |> setDateTimeOffsetProperty "Date" testData.Date
                  |> setDoubleProperty "Value" testData.Value
                  |> setStringProperty "Text" testData.Text
                  |> setBoolProperty "Exists" testData.Exists

              let! _ = saveDataArrayBatch tableMapper testTable fileWriterConfig testData

              let! values =
                  AzureTable.connect connectionString
                  |> AzureTable.table TestTable
                  |> AzureTable.execute (fun read ->
                      { PartKey = read.partKey
                        RowKey = read.rowKey
                        Date = read.dateTimeOffset "Date"
                        Exists = read.bool "Exists"
                        Value = read.float "Value"
                        Text = read.string "Text" })

              let data =
                  values
                  |> function
                  | Ok r -> r |> Array.tryHead
                  | Error (exn: Exception) ->
                      printfn "no data exn :%s" exn.Message
                      failwithf "no data exn :%s" exn.Message

              Expect.equal data (Some testData.[0]) "Insert test data is the same the readed testdata"
          }
          testTask "Insert test data as batch to table and read data from the table directly" {
              let testTable = getTable TestTable azAccount

              let testData =
                  [| { PartKey = "PartKey"
                       RowKey = DateTime.UtcNow |> RowKey.toRowKey
                       Date = DateTime.UtcNow |> System.DateTimeOffset
                       Value = 0.2
                       Exists = true
                       Text = "isWorking" } |]

              let tableMapper (testData: TestData) =
                  DynamicTableEntity(testData.PartKey, testData.RowKey.GetValue)
                  |> setDateTimeOffsetProperty "Date" testData.Date
                  |> setDoubleProperty "Value" testData.Value
                  |> setStringProperty "Text" testData.Text
                  |> setBoolProperty "Exists" testData.Exists

              let! _ = saveDataArrayBatch tableMapper testTable fileWriterConfig testData

              let! values =
                  AzureTable.connect connectionString
                  |> AzureTable.table TestTable
                  |> AzureTable.executeDirect (fun read ->
                      { PartKey = read.partKey
                        RowKey = read.rowKey
                        Date = read.dateTimeOffset "Date"
                        Exists = read.bool "Exists"
                        Value = read.float "Value"
                        Text = read.string "Text" })

              let results = values |> Array.tryHead

              Expect.equal results (Some testData.[0]) "Insert test data is the same the readed testdata"
          }
          testTask "Insert test data as batch to table and receive exactly one value from the table" {
              let testTable = getTable TestTable azAccount
              let rowKey = DateTime.UtcNow |> RowKey.toRowKey

              let testData =
                  [| { PartKey = "PartKey"
                       RowKey = rowKey
                       Date = DateTime.UtcNow |> System.DateTimeOffset
                       Value = 0.2
                       Exists = true
                       Text = "isWorking" } |]

              let tableMapper (testData: TestData) =
                  DynamicTableEntity(testData.PartKey, testData.RowKey.GetValue)
                  |> setDateTimeOffsetProperty "Date" testData.Date
                  |> setDoubleProperty "Value" testData.Value
                  |> setStringProperty "Text" testData.Text
                  |> setBoolProperty "Exists" testData.Exists

              let! _ = saveDataArrayBatch tableMapper testTable fileWriterConfig testData

              let! value =
                  AzureTable.connect connectionString
                  |> AzureTable.table TestTable
                  |> AzureTable.filterReceive ("PartKey", rowKey.GetValue)
                  |> AzureTable.receive (fun read ->
                      { PartKey = read.partKey
                        RowKey = read.rowKey
                        Date = read.dateTimeOffset "Date"
                        Exists = read.bool "Exists"
                        Value = read.float "Value"
                        Text = read.string "Text" })

              Expect.equal value (Some testData.[0]) "Insert test data is the same the readed testdata"
          } ]

let config =
    { defaultConfig with
          runInParallel = false }

[<EntryPoint>]
let main argv = runTestsInAssembly config argv
