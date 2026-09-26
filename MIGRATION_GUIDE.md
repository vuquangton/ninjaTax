# EF Core Multi-Database Migration Guide

This guide describes how to manage and execute migrations for `ninjaTax` across SQLite, SQL Server, PostgreSQL, and MariaDB.

---

## 1. Prerequisites

The `dotnet-ef` CLI tool is configured locally via `.config/dotnet-tools.json` (or `dotnet-tools.json`). Restore tools if needed:

```bash
dotnet tool restore
```

Verify `dotnet-ef` availability:
```bash
dotnet ef --version
```

---

## 2. Generating Migrations

Due to dialect differences (e.g. `INTEGER` vs `BIGSERIAL`, `TEXT` vs `decimal(19,4)`), migrations should be targeted to provider directories using `--output-dir`.

### 2.1 SQLite (Default)
```bash
# 1. Ensure DatabaseProvider is set to Sqlite in appsettings.json
dotnet ef migrations add InitialCreate --project ninjaTax.csproj --output-dir Migrations/Sqlite
```

### 2.2 SQL Server
```bash
# Set provider temporarily for migration generation
$env:DatabaseProvider="SqlServer"
dotnet ef migrations add InitialCreate_SqlServer --project ninjaTax.csproj --output-dir Migrations/SqlServer
$env:DatabaseProvider=""
```

### 2.3 PostgreSQL
```bash
$env:DatabaseProvider="PostgreSql"
dotnet ef migrations add InitialCreate_Postgres --project ninjaTax.csproj --output-dir Migrations/PostgreSql
$env:DatabaseProvider=""
```

### 2.4 MariaDB
```bash
$env:DatabaseProvider="MariaDb"
dotnet ef migrations add InitialCreate_MariaDb --project ninjaTax.csproj --output-dir Migrations/MariaDb
$env:DatabaseProvider=""
```

---

## 3. Applying Migrations

### Apply to Current Provider:
```bash
dotnet ef database update --project ninjaTax.csproj
```

### Apply Specific Migration:
```bash
dotnet ef database update InitialCreate --project ninjaTax.csproj
```

---

## 4. Rollback & Removal

### Rollback Database Schema:
To roll back the database schema to a previous state, specify the target migration name:
```bash
# Roll back to InitialCreate
dotnet ef database update InitialCreate --project ninjaTax.csproj

# Roll back all migrations (clear database schema)
dotnet ef database update 0 --project ninjaTax.csproj
```

### Remove the Last Pending Migration:
*(Only works if the migration has not been applied to the database, or after rolling back)*
```bash
dotnet ef migrations remove --project ninjaTax.csproj
```

---

## 5. Generating Raw SQL Scripts (For Enterprise DBAs)

For production deployment where direct CLI access to the database is prohibited:

```bash
# Generate idempotent script for SQL Server
$env:DatabaseProvider="SqlServer"
dotnet ef migrations script --idempotent --output deploy_sqlserver.sql --project ninjaTax.csproj
$env:DatabaseProvider=""

# Generate idempotent script for PostgreSQL
$env:DatabaseProvider="PostgreSql"
dotnet ef migrations script --idempotent --output deploy_postgres.sql --project ninjaTax.csproj
$env:DatabaseProvider=""
```
