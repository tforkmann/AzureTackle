# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AzureTackle is a functional F# wrapper library around the Azure.Data.Tables SDK for Azure Table Storage. It provides a fluent, pipeline-based API for table operations with F# functional programming patterns.

## Build Commands

```bash
# Restore tools first (required before running FAKE commands)
dotnet tool restore

# Build the library
dotnet run -- Build

# Run unit tests (requires Azurite emulator running)
dotnet run -- UnitTests

# Pack NuGet packages
dotnet run -- Pack

# Push to NuGet (will prompt for API key)
dotnet run -- Push

# Prepare a release (commits and tags)
dotnet run -- PrepareRelease

# Documentation commands
dotnet run -- InstallDocs  # Install npm dependencies
dotnet run -- RunDocs      # Run docs dev server
dotnet run -- PublishDocs  # Build production docs
```

## Testing

Tests use the Expecto framework and require Azurite (Azure Storage Emulator) running locally:

```bash
# Start Azurite first, then:
dotnet run -- UnitTests

# Or run tests directly:
cd tests/AzureTackle.Tests && dotnet run
```

Connection string for local development: `"UseDevelopmentStorage=true"`

## Architecture

### Source Structure

- **src/AzureTackle/** - Main library with Azure Table Storage operations
- **src/AzureTackle.Shared/** - Fable-compatible shared utilities (SortedRowKey)
- **tests/AzureTackle.Tests/** - Expecto test suite
- **src/docs/** - Documentation site (Fable + React + Vite)

### Core Modules (src/AzureTackle/AzureTackle.fs)

1. **Filter module** - Query DSL with composable filters
   - `AzureFilter` type with operators: `+` (AND), `*` (OR), `!!` (NOT)
   - Helper functions: `PartKey`, `RowKey`, `Equal`, `NOT`, `GreaterThan`, `LessThan`, etc.

2. **Table module** - Connection management
   - `AzureConnection` - Connection string or TableServiceClient wrapper
   - Table creation caching via `ConcurrentDictionary`

3. **AzureTable module** (RequireQualifiedAccess) - Main API
   - Connection: `withConnectionString`, `withTableClient`
   - Query: `execute`, `executeFirstPages`, `tryGetValue`
   - Mutation: `upsert`, `upsertInline`, `upsertBatch`, `delete`, `deleteBatch`
   - Configuration: `filter`, `withCancellationToken`, `withMaxElements`

4. **TableEntityExtensions** - Extension methods for TableEntity
   - `Append`/`AppendOptional` for fluent entity building
   - `Read*` methods for type-safe property access

### Usage Pattern

```fsharp
AzureTable.withConnectionString (connectionString, "TableName")
|> AzureTable.filter (PartKey "pk" + RowKey "rk")
|> AzureTable.execute (fun entity ->
    { Field1 = entity.ReadString "Field1" })
```

### SortedRowKey (Shared module)

Creates sortable row keys from DateTime using inverted ticks pattern:
```fsharp
DateTime.MaxValue.Ticks - dateTime.Ticks
```
This ensures newer entries sort first in Azure Table Storage.

## Package Management

Uses Paket for dependency management. Key dependencies:
- `Azure.Data.Tables` - Azure Table Storage SDK
- `FSharp.Core`

Build system uses FAKE (F# Make) via Build.fs.
