module Tests

open Expecto
open System
open AzureTackle
open Azure.Data.Tables
open System.Threading

printfn "Starting Tests"

[<Literal>]
let TestTable = "TestTable"

type TestData =
    { PartKey: string
      RowKey: string
      Date: DateTimeOffset
      Exists: bool
      Value: float
      ValueDecimal  : decimal
      Text: string }

let azureCon =
    ("UseDevelopmentStorage=true")
    |> AzureTable.connect

[<Tests>]
let simpleTest =
    testList
        "AzureTackle"
        [ testTask "Insert test data to table and read data from the table" {

            let testData =
                { PartKey = "PartKey"
                  RowKey = DateTime.UtcNow |> SortedRowKey.toSortedRowKey
                  Date = DateTime.UtcNow |> DateTimeOffset
                  Value = 0.2
                  Exists = true
                  ValueDecimal = 0.2m
                  Text = "isWorking" }

            do!
                azureCon
                |> AzureTable.table TestTable
                |> AzureTable.upsert (testData.PartKey, testData.RowKey) (fun set ->
                    set.add "Date" testData.Date
                    set.add "Value" testData.Value
                    set.add "ValueDecimal" testData.ValueDecimal
                    set.add "Exists" testData.Exists
                    set.add "Text" testData.Text
                    set.returnEntity)

            let! values =
                azureCon
                |> AzureTable.table TestTable
                |> AzureTable.filter (RowKey testData.RowKey)
                |> AzureTable.execute (fun read ->
                    { PartKey = read.partKey
                      RowKey = read.rowKey
                      Date = read.dateTimeOffset "Date"
                      Exists = read.bool "Exists"
                      Value = read.float "Value"
                      ValueDecimal = read.decimal "ValueDecimal"
                      Text = read.string "Text" })

            let data =  values |> Array.tryHead
            Expect.equal data (Some testData) "Insert test data is the same the readed testdata"
          }
          testTask "Insert test data to table and read data from the table directly" {

              let azureCon = ("UseDevelopmentStorage=true") |> AzureTable.connect

              let testData =
                  { PartKey = "PartKey"
                    RowKey = DateTime.UtcNow |> SortedRowKey.toSortedRowKey
                    Date = DateTime.UtcNow |> DateTimeOffset
                    Value = 0.2
                    ValueDecimal = 0.2m
                    Exists = true
                    Text = "isWorking" }

              do!
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.upsert (testData.PartKey, testData.RowKey) (fun set ->
                      set.add "Date" testData.Date
                      set.add "Value" testData.Value
                      set.add "ValueDecimal" testData.ValueDecimal
                      set.add "Exists" testData.Exists
                      set.add "Text" testData.Text
                      set.returnEntity)

              let! values =
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.execute (fun read ->
                      { PartKey = read.partKey
                        RowKey = read.rowKey
                        Date = read.dateTimeOffset "Date"
                        Exists = read.bool "Exists"
                        Value = read.float "Value"
                        ValueDecimal = read.decimal "ValueDecimal"
                        Text = read.string "Text" })

              let results = values |> Array.tryHead

              Expect.equal results (Some testData) "Insert test data is the same the readed testdata"
          }
          testTask "Insert test data as batch to table and receive exactly one value from the table" {
              let rowKey = DateTime.UtcNow |> SortedRowKey.toSortedRowKey

              let testData =
                  { PartKey = "PartKey"
                    RowKey = rowKey
                    Date = DateTime.UtcNow |> System.DateTimeOffset
                    Value = 0.2
                    ValueDecimal = 0.2m
                    Exists = true
                    Text = "isWorking" }

              do!
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.upsert (testData.PartKey, testData.RowKey) (fun set ->
                      set.add "Date" testData.Date
                      set.add "Value" testData.Value
                      set.add "ValueDecimal" testData.ValueDecimal
                      set.add "Exists" testData.Exists
                      set.add "Text" testData.Text
                      set.returnEntity)


              let! value =
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.filterReceive ("PartKey", rowKey)
                  |> AzureTable.receive (fun read ->
                      { PartKey = read.partKey
                        RowKey = read.rowKey
                        Date = read.dateTimeOffset "Date"
                        Exists = read.bool "Exists"
                        Value = read.float "Value"
                        ValueDecimal = read.decimal "ValueDecimal"
                        Text = read.string "Text" })

              Expect.equal value (Some testData) "Insert test data is the same the readed testdata"
          }
          testTask "Insert test data to table and backup and receive exactly one value from the backup table" {
              let rowKey = DateTime.UtcNow |> SortedRowKey.toSortedRowKey

              let testData =
                  { PartKey = "PartKey"
                    RowKey = rowKey
                    Date = DateTime.UtcNow |> System.DateTimeOffset
                    Value = 0.2
                    ValueDecimal = 0.2m
                    Exists = true
                    Text = "isWorking" }

              do!
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.upsert (testData.PartKey, testData.RowKey) (fun set ->
                      set.add "Date" testData.Date
                      set.add "Value" testData.Value
                      set.add "ValueDecimal" testData.ValueDecimal
                      set.add "Exists" testData.Exists
                      set.add "Text" testData.Text
                      set.returnEntity)


              let! value =
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.filterReceive ("PartKey", rowKey)
                  |> AzureTable.receive (fun read ->
                      { PartKey = read.partKey
                        RowKey = read.rowKey
                        Date = read.dateTimeOffset "Date"
                        Exists = read.bool "Exists"
                        Value = read.float "Value"
                        ValueDecimal = read.decimal "ValueDecimal"
                        Text = read.string "Text" })

              Expect.equal value (Some testData) "Insert test data is the same the readed testdata"
          }
          testTask "Insert test data to table and read timestamp from the table" {

              let testData =
                  { PartKey = "PartKey"
                    RowKey = DateTime.UtcNow |> SortedRowKey.toSortedRowKey
                    Date = DateTime.UtcNow |> System.DateTimeOffset
                    Value = 0.2
                    ValueDecimal = 0.2m
                    Exists = true
                    Text = "isWorking" }

              do!
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.upsert (testData.PartKey, testData.RowKey) (fun set ->
                      set.add "Date" testData.Date
                      set.add "Value" testData.Value
                      set.add "ValueDecimal" testData.ValueDecimal
                      set.add "Exists" testData.Exists
                      set.add "Text" testData.Text
                      set.returnEntity)

              let! timeStamps =
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.filter (RowKey testData.RowKey)
                  |> AzureTable.execute (fun read -> read.timeStamp)

              let data =
                  timeStamps |> Array.tryHead |> Option.isSome

              Expect.isTrue data "Timestamp isn't there"
          }
          testTask "Insert test data as batch to table and read timestamp from the table" {

                let testData =
                  [| {  PartKey = "PartKey"
                        RowKey = DateTime.UtcNow |> SortedRowKey.toSortedRowKey
                        Date = DateTime.UtcNow |> System.DateTimeOffset
                        Value = 0.2
                        ValueDecimal = 0.2m
                        Exists = true
                        Text = "isWorking" }|]
                do!
                    azureCon
                    |> AzureTable.table TestTable
                    |> AzureTable.upsertBatch testData (fun d ->
                        let set = AzureTackleSetEntity(d.PartKey, d.RowKey)
                        set.add "Date" d.Date
                        set.add "Exists" d.Exists
                        set.add "Text" d.Text
                        set.add "Value" d.Value
                        set.add "ValueDecimal" d.ValueDecimal
                        set.returnEntity)
                let! timeStamps =
                    azureCon
                    |> AzureTable.table TestTable
                    |> AzureTable.filter (RowKey testData.[0].RowKey)
                    |> AzureTable.execute (fun read -> read.timeStamp)

                let data = timeStamps |> Array.tryHead |> Option.isSome

                Expect.isTrue data "Timestamp isn't there"
          }
          testTask "Delete test data as batch" {

                let! values =
                  azureCon
                  |> AzureTable.table TestTable
                  |> AzureTable.executeDirect (fun read ->
                      { PartKey = read.partKey
                        RowKey = read.rowKey
                        Date = read.dateTimeOffset "Date"
                        Exists = read.bool "Exists"
                        Value = read.float "Value"
                        ValueDecimal = read.decimal "ValueDecimal"
                        Text = read.string "Text" })

                do!
                    azureCon
                    |> AzureTable.table TestTable
                    |> AzureTable.deleteBatch values (fun d ->
                        let set = AzureTackleSetEntity(d.PartKey, d.RowKey)
                        set.add "Date" d.Date
                        set.add "Exists" d.Exists
                        set.add "Text" d.Text
                        set.add "Value" d.Value
                        set.add "ValueDecimal" d.ValueDecimal
                        set.returnEntity)
                let! values =
                    azureCon
                    |> AzureTable.table TestTable
                    |> AzureTable.executeDirect (fun read ->
                        {   PartKey = read.partKey
                            RowKey = read.rowKey
                            Date = read.dateTimeOffset "Date"
                            Exists = read.bool "Exists"
                            Value = read.float "Value"
                            ValueDecimal = read.decimal "ValueDecimal"
                            Text = read.string "Text" })
                Expect.isEmpty values "Values should be empty"
          }   ]

let config =
    { defaultConfig with runInParallel = false }

[<EntryPoint>]
let main argv = runTestsInAssemblyWithCLIArgs [] argv
