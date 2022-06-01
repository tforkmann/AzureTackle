module Tests

open Expecto
open System
open AzureTackle
open Microsoft.WindowsAzure.Storage.Table

printfn "Starting Tests"

[<Literal>]
let TestTable = "TestTable"

type TestData =
    { PartKey: string
      RowKey: RowKey
      Date: DateTimeOffset
      Exists: bool
      Value: float
      Text: string }

let azureCon =
    ("UseDevelopmentStorage=true", "UseDevelopmentStorage=true", Prod)
    |> AzureTable.connectWithStages

[<Tests>]
let simpleTest =
    testList
        "AzureTackle"
        [ testTask "Insert test data to table and read data from the table" {

            let testData =
                { PartKey = "PartKey"
                  RowKey = DateTime.UtcNow |> RowKey.toRowKey
                  Date = DateTime.UtcNow |> DateTimeOffset
                  Value = 0.2
                  Exists = true
                  Text = "isWorking" }

            do!
                azureCon
                |> AzureTable.table TestTable
                |> AzureTable.insert (testData.PartKey, testData.RowKey) (fun set ->
                    set.dateTimeOffset "Date" testData.Date
                    set.float "Value" testData.Value
                    set.bool "Exists" testData.Exists
                    set.string "Text" testData.Text
                    set.returnEntity)

            let! values =
                azureCon
                |> AzureTable.table TestTable
                |> AzureTable.filter [
                    RoKey(Equal, testData.RowKey.GetValue)
                   ]
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

            Expect.equal data (Some testData) "Insert test data is the same the readed testdata"
          }
          testTask "Insert test data to table and read data from the table directly" {

              let azureCon = "UseDevelopmentStorage=true" |> AzureTable.connect

              let testData =
                  { PartKey = "PartKey"
                    RowKey = DateTime.UtcNow |> RowKey.toRowKey
                    Date = DateTime.UtcNow |> DateTimeOffset
                    Value = 0.2
                    Exists = true
                    Text = "isWorking" }

              do!
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.insert (testData.PartKey, testData.RowKey) (fun set ->
                      set.dateTimeOffset "Date" testData.Date
                      set.float "Value" testData.Value
                      set.bool "Exists" testData.Exists
                      set.string "Text" testData.Text
                      set.returnEntity)

              let! values =
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.executeDirect (fun read ->
                      { PartKey = read.partKey
                        RowKey = read.rowKey
                        Date = read.dateTimeOffset "Date"
                        Exists = read.bool "Exists"
                        Value = read.float "Value"
                        Text = read.string "Text" })

              let results = values |> Array.tryHead

              Expect.equal results (Some testData) "Insert test data is the same the readed testdata"
          }
          testTask "Insert test data as batch to table and receive exactly one value from the table" {
              let rowKey = DateTime.UtcNow |> RowKey.toRowKey

              let testData =
                  { PartKey = "PartKey"
                    RowKey = rowKey
                    Date = DateTime.UtcNow |> System.DateTimeOffset
                    Value = 0.2
                    Exists = true
                    Text = "isWorking" }

              do!
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.insert (testData.PartKey, testData.RowKey) (fun set ->
                      set.dateTimeOffset "Date" testData.Date
                      set.float "Value" testData.Value
                      set.bool "Exists" testData.Exists
                      set.string "Text" testData.Text
                      set.returnEntity)


              let! value =
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.filterReceive ("PartKey", rowKey.GetValue)
                  |> AzureTable.receive (fun read ->
                      { PartKey = read.partKey
                        RowKey = read.rowKey
                        Date = read.dateTimeOffset "Date"
                        Exists = read.bool "Exists"
                        Value = read.float "Value"
                        Text = read.string "Text" })

              Expect.equal value (Some testData) "Insert test data is the same the readed testdata"
          }
          testTask "Insert test data to table and backup and receive exactly one value from the backup table" {
              let rowKey = DateTime.UtcNow |> RowKey.toRowKey

              let testData =
                  { PartKey = "PartKey"
                    RowKey = rowKey
                    Date = DateTime.UtcNow |> System.DateTimeOffset
                    Value = 0.2
                    Exists = true
                    Text = "isWorking" }

              do!
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.insert (testData.PartKey, testData.RowKey) (fun set ->
                      set.dateTimeOffset "Date" testData.Date
                      set.float "Value" testData.Value
                      set.bool "Exists" testData.Exists
                      set.string "Text" testData.Text
                      set.returnEntity)


              let! value =
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.filterReceive ("PartKey", rowKey.GetValue)
                  |> AzureTable.receive (fun read ->
                      { PartKey = read.partKey
                        RowKey = read.rowKey
                        Date = read.dateTimeOffset "Date"
                        Exists = read.bool "Exists"
                        Value = read.float "Value"
                        Text = read.string "Text" })

              Expect.equal value (Some testData) "Insert test data is the same the readed testdata"
          }
          testTask "Insert test data to table and read timestamp from the table" {

              let testData =
                  { PartKey = "PartKey"
                    RowKey = DateTime.UtcNow |> RowKey.toRowKey
                    Date = DateTime.UtcNow |> System.DateTimeOffset
                    Value = 0.2
                    Exists = true
                    Text = "isWorking" }

              do!
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.insert (testData.PartKey, testData.RowKey) (fun set ->
                      set.dateTimeOffset "Date" testData.Date
                      set.float "Value" testData.Value
                      set.bool "Exists" testData.Exists
                      set.string "Text" testData.Text
                      set.returnEntity)

              let! timeStamps =
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.filter [
                      RoKey(Equal, testData.RowKey.GetValue)
                     ]
                  |> AzureTable.execute (fun read -> read.timeStamp)

              let data =
                  timeStamps
                  |> function
                      | Ok r ->
                          printfn "%A" r
                          (r |> Array.tryHead).IsSome
                      | Error (exn: Exception) ->
                          printfn "no data exn :%s" exn.Message
                          false

              Expect.isTrue data "Timestamp isn't there"
          }
          testTask "Insert test data as batch to table and read timestamp from the table" {

                let testData =
                  [| {  PartKey = "PartKey"
                        RowKey = DateTime.UtcNow |> RowKey.toRowKey
                        Date = DateTime.UtcNow |> System.DateTimeOffset
                        Value = 0.2
                        Exists = true
                        Text = "isWorking" }|]
                do!
                    azureCon
                    |> AzureTable.table TestTable
                    |> AzureTable.insertBatch testData (fun d ->
                        let set = AzureTackleSetEntity(d.PartKey, d.RowKey.GetValue)
                        set.dateTimeOffset "Date" d.Date
                        set.bool "Exists" d.Exists
                        set.string "Text" d.Text
                        set.float "Value" d.Value
                        set.returnEntity)
                let! timeStamps =
                    azureCon
                    |> AzureTable.table TestTable
                    |> AzureTable.filter [
                        RoKey(Equal, testData.[0].RowKey.GetValue)
                        ]
                    |> AzureTable.execute (fun read -> read.timeStamp)

                let data =
                    timeStamps
                    |> function
                        | Ok r ->
                            printfn "%A" r
                            (r |> Array.tryHead).IsSome
                        | Error (exn: Exception) ->
                            printfn "no data exn :%s" exn.Message
                            false

                Expect.isTrue data "Timestamp isn't there"
          }   ]

let config =
    { defaultConfig with runInParallel = false }

[<EntryPoint>]
let main argv = runTestsInAssembly config argv
