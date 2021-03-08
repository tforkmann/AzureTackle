module Config

open Microsoft.Extensions.Configuration
open System.IO

let config =
    ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build()

let connectionString = config.["ConnectionString"]
let connectionStringBackup = config.["ConnectionStringBackup"]

