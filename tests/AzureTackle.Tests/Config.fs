module Config

open Microsoft.Extensions.Configuration
open System.IO

let config =
    ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build()

let connectionStringProd = config.["ConnectionStringProd"]
let connectionStringDev = config.["ConnectionStringDev"]

