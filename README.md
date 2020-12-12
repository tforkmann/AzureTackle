# AzureTackle

Functional wrapper around WindowsAzure.Storage SKD `AzureTackle` to simplify data access when talking to Azure tables.

## Available Packages:

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

type User = { Id: int; Username: string }

let! values =
    AzureTable.connect connectionString()
    |> AzureTable.table testTable
    |> AzureTable.execute (fun read ->
    { PartKey = read.partKey
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

## Query a table inside a task with filter
```fs
open AzureTackle

// get the connection from the environment
let connectionString() = Env.getVar "app_db"

type User = { Id: int; Username: string }

let! values =
    AzureTable.connect connectionString()
    |> AzureTable.table testTable
    |> AzureTable.filter [DtmO ("Date",GreaterThenOrEqual, timeModel.DateStart);DtmO ("Date",LessThen, timeModel.DateEnd)]
    |> AzureTable.execute (fun read ->
    { PartKey = read.partKey
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
## Available Operator
```fs
type Operator =
    | LessThen
    | LessThenOrEqual
    | GreaterThen
    | GreaterThenOrEqual
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
    | PartKey of Operator * string
    | RowKey of Operator * string
````
