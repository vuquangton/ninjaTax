# AGENTS.md

Repository guidelines for autonomous agents working in `ninjaTax`.

## Overview
- ASP.NET Core MVC web application on .NET 10 (`net10.0`).
- C# nullable reference types and implicit usings enabled.
- Default Database Provider: **MariaDB 12+** via `MySql.EntityFrameworkCore 10.0.9` (`ninjataxdb` on `localhost:3306`).
- Multi-DB Architecture: Dynamic provider seam supporting MariaDB, SQLite, PostgreSQL, and SQL Server.

## Commands
- **Build**: `dotnet build` (warnings treated as errors; must remain 0 warnings).
- **Test**: `dotnet test` (all tests in `ninjaTax.Tests/` must pass).
- **Run**: `dotnet run` (listens on `http://localhost:5188`).
- **Clean**: `dotnet clean`.

## Database Guidelines
- **MariaDB (Default)**: Connection string in `appsettings.json` / `appsettings.Development.json`. Auto-initializes schema via `EnsureCreatedAsync()`.
- **SQLite**: Dedicated migrations stored in `Migrations/Sqlite/`. Used for local dev fallback and in-memory unit tests.
- **Provider Switching**: Set `"DatabaseProvider": "MariaDb" | "Sqlite" | "PostgreSql" | "SqlServer"` in config.

## Core Accounting Invariants (Circular TT99 & VAS)
- **Account 911 Prohibition**: Strict ban. Account 911 must never appear in any entity, seed data, journal entry, or posting routine.
- **Double-Entry Equality**: Every journal voucher must satisfy `TongNo == TongCo`.
- **Anti-Negative Stock (VAS 02)**: Outward warehouse vouchers and inward slip cancellations cannot cause warehouse balance to drop below zero.
- **Cost Segmentation (Phase 7)**: Department `PhongBanId` attached to expense lines (TK 154, 6421, 6422) for segment P&L.
- **Inventory S10-DN Reconciliation (Phase 8)**: Total closing stock value must reconcile against GL inventory accounts (152, 155, 1561).

## Architecture & Layout
- `Controllers/`: ASP.NET Core MVC controllers. Keep controllers thin; delegate logic to service layer.
- `Models/Entities/`: TT99 domain entities (`Kho`, `PhieuNhapKho`, `PhieuXuatKho`, `PhongBan`, `ButToan`, etc.).
- `Models/Services/`: Deep domain services (`IInventoryService`, `IDepartmentService`, `IButToanService`, etc.).
- `Models/ViewModels/`: Strictly typed view models for MVC views and statutory reports.
- `Views/`: Razor views (`.cshtml`) and layout templates.
- `Data/`: `AppDbContext`, `DatabaseServiceExtensions`, `DbInitializer`.
- `ninjaTax.Tests/`: Unit and integration test suites.

## Agent Workflow
- Verify any code modification with `dotnet build` and `dotnet test` before reporting completion.
- Keep edits localized and preserve existing file structures and configurations.

## Skill Selection Matrix (5W1H)

| Who (Role) | What (Skill) | When (Trigger) | Where (Scope) | Why (Purpose) | How (Action) |
|---|---|---|---|---|---|
| **Domain Architect** | `domain-modeling` | Defining new TT99 entities, accounts, tax rules | `Models/Entities/`, ADRs | Prevent terminology drift, align VAS standards | Map ubiquitous terms, document invariants & boundaries |
| **Software Architect** | `codebase-design` | Structuring multi-DB seams, service interfaces | `Models/Services/`, `Data/` | Keep interfaces deep, implementations thin | Define contracts (`IButToanService`, `IInventoryService`), isolate DB dialects |
| **Developer** | `tdd` | Building calculation logic, journals, VAT rules | `ninjaTax.Tests/`, `Models/` | Guarantee invariants (TongNo == TongCo, Anti-Negative Stock) | Write failing unit test &rarr; pass &rarr; refactor |
| **UI Developer** | `generative_ui` | Visualizing T-accounts, ledger balances, reports | Inline chat, standalone HTML | Clear visual inspection of financial flow | Render interactive debit/credit diagrams and ledger charts |
| **Prototyper** | `prototype` | Testing complex tax formulas or UI layouts | `scratch/`, test views | Quick sanity check before full implementation | Build throwaway spike script or Razor mock |
| **Auditor / Reviewer** | `grilling` | Validating accounting flows, period closing rules | PR plans, architecture decisions | Expose edge cases (e.g. 911 usage, precision loss) | Relentlessly question assumptions and invariants |
| **Reviewer** | `code-review` | Before merging branch or completing major phase | Git diff, modified files | Verify repo standards (AGENTS.md) and TT99 spec | Run parallel Standards & Spec checks |
| **Debugger** | `diagnosing-bugs` | Balance mismatch, DB dialect quirks, 500 error | Logs, DB queries, controller actions | Find root cause without guessing | Form testable hypothesis &rarr; instrument &rarr; isolate bug |
| **Integrator** | `resolving-merge-conflicts`| Git rebase or merge failure | Git workspace | Clean history, preserve accounting logic | Inspect base, ours, theirs &rarr; resolve &rarr; verify build |
| **Researcher** | `research` | Checking obscure tax circulars or EF Core quirks | `docs/research/` | Authoritative source validation | Query official sources, write markdown finding summary |
| **Ops Guide** | `wizard` | Manual DB migration, production server setup | CLI / Shell scripts | Guide human through credentials & manual steps | Generate step-by-step interactive CLI script |
| **Prompt Engineer** | `writing-for-agents` | Updating `AGENTS.md`, `GEMINI.md`, or skills | `AGENTS.md`, `.gemini/skills/` | Optimize agent context load & instruction following | Apply progressive disclosure, leading words, positive rules |
| **Tool Configurator**| `agy-customizations`| Adding custom MCP, project-specific rules | `.gemini/antigravity/` | Extend agent capabilities for ninjaTax | Define rule files, hooks, or custom MCP servers |
| **Platform Guide** | `antigravity-guide` | Questions about Antigravity CLI, IDE, configs | Antigravity IDE / CLI | Correct tool configuration and workflow usage | Lookup commands, slash tools, and IDE settings |
| **Workflow Migrator**| `migrate-workflows` | Upgrading legacy workflow files to skills | `.gemini/workflows/` | Modernize agent configuration | Scan legacy scripts &rarr; convert to `SKILL.md` format |
| **Data Engineer** | `karpathy-guidelines`| Tax fraud heuristics, ML invoice OCR | Data pipelines, ML models | Avoid premature abstraction, verify data first | Build dumb baseline &rarr; overfit batch &rarr; measure |
