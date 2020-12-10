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
.paket/paket.exe add AzureTackle --project path/to/project.fsproj
```

## Query a table
```fs
open AzureTackle

// get the connection from the environment
let connectionString() = Env.getVar "app_db"

type User = { Id: int; Username: string }

let getUsers() : Result<User list, exn> =
    connectionString()
    |> AzureTackle.connect
    |> AzureTackle.query "SELECT * FROM dbo.[Users]"
    |> AzureTackle.execute (fun read ->
        {
            Id = read.int "user_id"
            Username = read.string "username"
        })
```

## Handle null values from table columns:
```fs
open AzureTackle

// get the connection from the environment
let connectionString() = Env.getVar "app_db"

type User = { Id: int; Username: string; LastModified : Option<DateTime> }

let getUsers() : Result<User list, exn> =
    connectionString()
    |> AzureTackle.connect
    |> AzureTackle.query "SELECT * FROM dbo.[users]"
    |> AzureTackle.execute(fun read ->
        {
            Id = read.int "user_id"
            Username = read.string "username"
            // Notice here using `orNone` reader variants
            LastModified = read.dateTimeOrNone "last_modified"
        })
```
## Providing default values for null columns:
```fs
open AzureTackle

// get the connection from the environment
let connectionString() = Env.getVar "app_db"

type User = { Id: int; Username: string; Biography : string }

let getUsers() : Result<User list, exn> =
    connectionString()
    |> AzureTackle.connect
    |> AzureTackle.query "select * from dbo.[users]"
    |> AzureTackle.execute (fun read ->
        {
            Id = read.int "user_id";
            Username = read.string "username"
            Biography = defaultArg (read.stringOrNone "bio") ""
        })
```
## Execute a parameterized query
```fs
open AzureTackle

// get the connection from the environment
let connectionString() = Env.getVar "app_db"

// get product names by category
let productsByCategory (category: string) : Result<string list, exn> =
    connectionString()
    |> AzureTackle.connect
    |> AzureTackle.query "SELECT name FROM dbo.[Products] where category = @category"
    |> AzureTackle.parameters [ "@category", AzureTackle.string category ]
    |> AzureTackle.execute (fun read -> read.string "name")
```
### Executing a stored procedure with parameters
```fs
open AzureTackle

// get the connection from the environment
let connectionString() = Env.getVar "app_db"

// check whether a user exists or not
let userExists (username: string) : Async<Result<bool, exn>> =
    async {
        return!
            connectionString()
            |> AzureTackle.connect
            |> AzureTackle.storedProcedure "user_exists"
            |> AzureTackle.parameters [ "@username", AzureTackle.string username ]
            |> AzureTackle.execute (fun read -> read.bool 0)
            |> function
                | Ok [ result ] -> Ok result
                | Error error -> Error error
                | unexpected -> failwithf "Expected result %A"  unexpected
    }
```
### Executing a stored procedure with table-valued parameters
```fs
open AzureTackle
open System.Data

// get the connection from the environment
let connectionString() = Env.getVar "app_db"

let executeMyStoredProcedure () : Async<int> =
    // create a table-valued parameter
    let customSqlTypeName = "MyCustomSqlTypeName"
    let dataTable = new DataTable()
    dataTable.Columns.Add "FirstName" |> ignore
    dataTable.Columns.Add "LastName"  |> ignore
    // add rows to the table parameter
    dataTable.Rows.Add("John", "Doe") |> ignore
    dataTable.Rows.Add("Jane", "Doe") |> ignore
    dataTable.Rows.Add("Fred", "Doe") |> ignore

    connectionString()
    |> AzureTackle.connect
    |> AzureTackle.storedProcedure "my_stored_proc"
    |> AzureTackle.parameters
        [ "@foo", AzureTackle.int 1
          "@people", AzureTackle.table (customSqlTypeName, dataTable) ]
    |> AzureTackle.executeNonQueryAsync
```

## Running Tests locally

You only need a working local AzureTackle. The tests will create databases when required and dispose of them at the end of the each test

