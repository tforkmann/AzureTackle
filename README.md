# AzureTackle

Functional wrapper around WindowsAzure.Storage SDK. `AzureTackle` simplifies the data access when talking to Azure tables.

## Available Packages

| Library  | Version |
| ------------- | ------------- |
| AzureTackle  | [![nuget - AzureTackle](https://img.shields.io/nuget/v/AzureTackle.svg?colorB=green)](hhttps://www.nuget.org/packages/AzureTackle/) |

## Install

```bash
# nuget client
dotnet add package AzureTackle
  
# or using paket
paket add AzureTackle --project path/to/project.fsproj
```

## Query a table inside a task

```fs
open AzureTackle

// get the connection from the environment
let connectionString() = Env.getVar "app_db"
type TestData =
    { PartKey: string
      RowKey: RowKey
      Date: DateTimeOffset
      Exists: bool
      Value: float
      Text: string }

let! values =
    AzureTable.connect connectionString()
    |> AzureTable.table testTable
    |> AzureTable.execute (fun read ->
    {   PartKey = read.partKey
        RowKey = read.rowKey
        Date = read.dateTimeOffset "Date"
        Value = read.float "Value"
        Text = read.string "Text" })
let data =
    values
    |> function
    | Ok r -> r |> Array.tryHead
    | Error (exn:Exception) ->
        failwithf "no data exn :%s" exn.Message        
```

## Query a table inside a task and directly receive the results

```fs
open AzureTackle

// get the connection from the environment
let connectionString() = Env.getVar "app_db"

type TestData =
    { PartKey: string
      RowKey: RowKey
      Date: DateTimeOffset
      Exists: bool
      Value: float
      Text: string }

let! values =
    AzureTable.connect connectionString()
    |> AzureTable.table testTable
    |> AzureTable.executeDirect (fun read ->
    {   PartKey = read.partKey
        RowKey = read.rowKey
        Date = read.dateTimeOffset "Date"
        Value = read.float "Value"
        Text = read.string "Text" })
```

## Query a table inside a task and receive one value

```fs
open AzureTackle

// get the connection from the environment
let connectionString() = Env.getVar "app_db"

type TestData =
    { PartKey: string
      RowKey: RowKey
      Date: DateTimeOffset
      Exists: bool
      Value: float
      Text: string }

let! value =
    connectionString()
    |> AzureTable.connect 
    |> AzureTable.table testTable
    |> AzureTable.filterReceive ("PartKey","RowKey")
    |> AzureTable.receive (fun read ->
    {   PartKey = read.partKey
        RowKey = read.rowKey
        Date = read.dateTimeOffset "Date"
        Value = read.float "Value"
        Text = read.string "Text" })
```

## Query a table inside a task with filter

```fs
open AzureTackle

// get the connection from the environment
let connectionString() = Env.getVar "app_db"

type TestData =
    { PartKey: string
      RowKey: RowKey
      Date: DateTimeOffset
      Exists: bool
      Value: float
      Text: string }

let! values =
    connectionString()
    |> AzureTable.connect 
    |> AzureTable.table testTable
    |> AzureTable.filter [DtmO ("Date",GreaterThenOrEqual, timeModel.DateStart);DtmO ("Date",LessThen, timeModel.DateEnd)]
    |> AzureTable.execute (fun read ->
    {   PartKey = read.partKey
        RowKey = read.rowKey
        Date = read.dateTimeOffset "Date"
        Value = read.float "Value"
        Text = read.string "Text" })
let data =
    values
    |> function
    | Ok r -> r |> Array.tryHead
    | Error (exn:Exception) ->
        failwithf "no data exn :%s" exn.Message        
```

## Insert testData into a table

```fs
    let testData =
        {   PartKey = "PartKey"
            RowKey = DateTime.UtcNow |> RowKey.toRowKey
            Date = DateTime.UtcNow |> System.DateTimeOffset
            Value = 0.2
            Exists = true
            Text = "isWorking" }

    do!
    connectionString()
    |> AzureTable.connect 
    |> AzureTable.table TestTable
    |> AzureTable.insert
    (testData.PartKey, testData.RowKey)
        (fun set ->
            set.dateTimeOffset ("Date",testData.Date) |> ignore
            set.float ("Value",testData.Value) |> ignore
            set.bool ("Exists",testData.Exists) |> ignore
            set.string ("Text",testData.Text)
            )
```

## Delete one entity from a table

```fs

    do!
        connectionString()
        |> AzureTable.connect 
        |> AzureTable.table TestTable
        |> AzureTable.delete ("partKey", "rowKey")
```

## Available Operator

```fs
type Operator =
    | LessThan
    | LessThanOrEqual
    | GreaterThan
    | GreaterThanOrEqual
    | Equal
    | NotEqual
```

## Available AzureFilter

```fs
type AzureFilter =
    | Flt of string * Operator * float
    | Txt of string * Operator * string
    | Dtm of string * Operator * DateTime
    | DtmO of string * Operator *  DateTimeOffset
    | PaKey of Operator * string
    | RoKey of Operator * string
    | TStmp of Operator * DateTimeOffset
```

## Backup your data

The only thing you have to do is to create a new storage account and connect it like this.

```fs
let azureCon =
    (connectionString,connectionStringBackup)
    |> AzureTable.connectWithBackup
```

All your Azure table operations will then be mirrowed to the backup storage account
