# Multi-Database Configuration Guide (ninjaTax)

`ninjaTax` implements a flexible Multi-Database architecture powered by Entity Framework Core, supporting:
1. **SQLite (Default)**: Embedded, file-based, zero configuration needed.
2. **SQL Server / SQL Express (2012+)**: Enterprise deployment on Windows / Azure.
3. **PostgreSQL (Npgsql)**: Cloud-native Linux container deployments.
4. **MariaDB / MySQL (Pomelo)**: Open-source relational workloads.

---

## 1. Provider Switching via `appsettings.json`

Set the `DatabaseProvider` key in `appsettings.json` (or `appsettings.Development.json` / `appsettings.Production.json`):

```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "Sqlite": "Data Source=ninjatax.db",
    "SqlServer": "Server=localhost;Database=ninjaTaxDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
    "PostgreSql": "Host=localhost;Port=5432;Database=ninjataxdb;Username=postgres;Password=YourPassword",
    "MariaDb": "Server=localhost;Port=3306;Database=ninjataxdb;User=root;Password=YourPassword;TreatTinyAsBoolean=true"
  }
}
```

### Supported Values for `DatabaseProvider`
- `Sqlite` (Case-insensitive)
- `SqlServer` (or `mssql`)
- `PostgreSql` (or `postgres`, `npgsql`)
- `MariaDb` (or `mysql`)

*Note: If `DatabaseProvider` is empty, missing, or unrecognized, the application logs a warning and automatically falls back to `Sqlite`.*

---

## 2. Environment Variable Overrides (Production & CI/CD)

To avoid storing credentials in source control, override configuration using ASP.NET Core environment variables:

### Linux / Docker:
```bash
# Select Provider
export DatabaseProvider="PostgreSql"

# Set Connection String
export ConnectionStrings__PostgreSql="Host=postgres-prod.internal;Port=5432;Database=ninjatax;Username=tax_app;Password=MySecretPassword"
```

### Windows PowerShell:
```powershell
$env:DatabaseProvider = "SqlServer"
$env:ConnectionStrings__SqlServer = "Server=sqlserver.corp.local;Database=ninjaTax_Prod;User ID=ninja_sa;Password=MySecretPassword;Encrypt=True;TrustServerCertificate=False"
```

### Docker Compose Example:
```yaml
services:
  ninjatax-web:
    image: ninjatax:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DatabaseProvider=PostgreSql
      - ConnectionStrings__PostgreSql=Host=db;Port=5432;Database=ninjatax;Username=postgres;Password=secret
    ports:
      - "5188:8080"
```

---

## 3. Decimal Precision & Financial Integrity

All monetary decimal properties (`TongTien`, `TongNo`, `TongCo`, `SoTien`) are strictly configured with `.HasPrecision(19, 4)`:
- **SQLite**: Persisted as `TEXT` to eliminate IEEE 754 floating-point drift.
- **SQL Server**: `decimal(19, 4)`.
- **PostgreSQL**: `numeric(19, 4)`.
- **MariaDB**: `decimal(19, 4)`.

---

## 4. Verification Command
To verify database startup without running the full web server:
```bash
dotnet test --filter "FullyQualifiedName~MultiDatabaseTests"
```
