# AzureTackle

Functional wrapper around plain old `AzureTackle` to simplify data access when talking to AzureTackle databases.

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

## Query a table
```fs
open AzureTackle

// get the connection from the environment
let connectionString() = Env.getVar "app_db"

type User = { Id: int; Username: string }

let! values =
    AzureTable.connect connectionString()
    AzureTable.table testTable
    |> AzureTable.execute (fun read ->
    { PartKey = read.partKey
        RowKey = read.rowKey
        Date = read.dateTimeOffset "Date"
        Value = read.float "Value"
        Text = read.string "Text" })
```

