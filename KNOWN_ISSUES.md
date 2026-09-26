# Known Issues & Provider-Specific Workarounds

This document outlines known limitations, database provider quirks, and implemented architectural workarounds in `ninjaTax`.

---

## 1. SQLite: Decimal Precision Loss (Floating-Point Drift)

### Problem
SQLite does not possess a native arbitrary-precision `DECIMAL` datatype. Standard EF Core configurations without column mapping might default to SQLite's `REAL` (8-byte IEEE 754 floating-point). In Vietnamese double-entry accounting (TT99), precision loss (e.g. `1000000.005` storing as `1000000.0049999999`) produces cumulative trial balance discrepancies (`TongNo != TongCo`).

### Workaround Implemented
In `AppDbContext.OnModelCreating`, when `Database.IsSqlite()` is active, all monetary properties are explicitly bound to `TEXT`:
```csharp
entity.Property(e => e.TongTien).HasPrecision(19, 4);
if (Database.IsSqlite())
{
    entity.Property(e => e.TongTien).HasColumnType("TEXT");
}
```
EF Core transparently handles serialization and deserialization between C# `decimal` and SQLite `TEXT`, guaranteeing 100% exact numerical fidelity without precision drift.

---

## 2. MariaDB / Pomelo: Server Version Auto-Detection During Offline Tooling

### Problem
`Pomelo.EntityFrameworkCore.MySql` typically recommends `ServerVersion.AutoDetect(connectionString)`. However, if the MariaDB instance is unavailable (e.g., during offline `dotnet ef migrations add` or continuous integration builds), `AutoDetect` throws a connection exception and halts execution.

### Workaround Implemented
In `DatabaseServiceExtensions.cs`, a explicit fallback `MariaDbServerVersion` is supplied:
```csharp
var serverVersion = new MariaDbServerVersion(new Version(11, 0, 0));
options.UseMySql(connectionString, serverVersion);
```
This enables static migration generation, design-time tooling, and test runs without requiring a live MariaDB instance.

---

## 3. PostgreSQL: Identifier Quoting & Case Sensitivity

### Problem
PostgreSQL folds unquoted identifiers to lowercase. When entities use PascalCase (e.g. `TaiKhoan`, `SoChungTu`), unquoted SQL queries will search for `taikhoan` and fail if tables were created with quotes.

### Workaround Implemented
All entities and table names are explicitly configured in `AppDbContext.OnModelCreating` using `.ToTable("TaiKhoan")` and explicit property mappings, ensuring EF Core consistently generates quoted identifiers (`"TaiKhoan"`, `"SoChungTu"`) across all queries and migrations.

---

## 4. SQLite: File Locking in Concurrent Test Runs

### Problem
SQLite file-based databases can encounter `database is locked` errors during parallel test executions or rapid connection teardown.

### Workaround Implemented
In unit and integration tests (`ninjaTax.Tests/MultiDatabaseTests.cs`), tests utilize SQLite In-Memory connections with persistent open handles (`Data Source=:memory:`), ensuring complete test isolation, zero disk I/O contention, and deterministic execution speed (< 600ms total test run).

---

## 5. Nested Project Compilation in Root .NET SDK

### Problem
Because `ninjaTax.Tests` resides inside the repository root alongside `ninjaTax.csproj`, the root SDK-style project recursively matches `**/*.cs` within the subfolder, causing duplicate assembly attribute collisions and missing reference errors.

### Workaround Implemented
In `ninjaTax.csproj`, the test subdirectory is explicitly excluded from compilation:
```xml
<ItemGroup>
  <Compile Remove="ninjaTax.Tests\**" />
  <Content Remove="ninjaTax.Tests\**" />
  <EmbeddedResource Remove="ninjaTax.Tests\**" />
  <None Remove="ninjaTax.Tests\**" />
</ItemGroup>
```
Both projects are registered cleanly in `ninjaTax.sln`.
