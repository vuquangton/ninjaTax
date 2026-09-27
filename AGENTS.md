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

### 1. Requirements, Design & Architecture
| Role | Skill | Trigger (When) | Scope (Where) | Purpose (Why) | Action (How) |
|---|---|---|---|---|---|
| **Product Designer** | `brainstorming` | Starting new feature, component, or behavior change | Requirements, UX flows | Explore user intent, uncover trade-offs | Interview user, evaluate alternatives, document spec |
| **Domain Architect** | `domain-modeling` | Defining new TT99 entities, accounts, tax rules | `Models/Entities/`, ADRs | Prevent terminology drift, align VAS standards | Map ubiquitous terms, document invariants & boundaries |
| **Software Architect** | `codebase-design` | Structuring multi-DB seams, service interfaces | `Models/Services/`, `Data/` | Keep interfaces deep, implementations thin | Define contracts (`IButToanService`, `IInventoryService`), isolate DB dialects |
| **System Refactorer** | `improve-codebase-architecture` | Code smells, tight coupling, bloated classes | Core domain, controllers | Eliminate architectural debt | Apply Fowler refactorings, extract services, decouple dependencies |
| **Auditor / Reviewer** | `grilling` / `grill-me` | Validating accounting flows, period closing rules | PR plans, architecture decisions | Expose edge cases (e.g. 911 usage, precision loss) | Relentlessly question assumptions and invariants |

### 2. Planning & Multi-Agent Execution
| Role | Skill | Trigger (When) | Scope (Where) | Purpose (Why) | Action (How) |
|---|---|---|---|---|---|
| **Planner** | `writing-plans` | Complex, multi-step task before touching code | `docs/plans/`, workspace | Produce checkable, phased roadmap | Break down tasks with exact file targets and verification steps |
| **Implementer** | `executing-plans` | Approved implementation plan ready for inline execution | Codebase | Ensure orderly execution | Execute step-by-step, verify after each slice |
| **Coordinator** | `subagent-driven-development` | Plan tasks are independent with clean boundaries | Subagent pool | Accelerate execution without context pollution | Dispatch subagents per task, review diffs, integrate |
| **Parallel Orchestrator**| `dispatching-parallel-agents` | 2+ independent research, review, or coding tasks | Multi-subagent pool | Maximize concurrency | Spawn parallel subagents, collect reports, synthesize |
| **Workspace Isolator** | `using-git-worktrees` | Feature work needing workspace isolation | Git worktrees | Prevent branch collision and dirty working tree | Create and manage isolated worktree branches |

### 3. Implementation & Testing
| Role | Skill | Trigger (When) | Scope (Where) | Purpose (Why) | Action (How) |
|---|---|---|---|---|---|
| **TDD Developer** | `tdd` / `test-driven-development` | Implementing new features, calculations, or bugfixes | `ninjaTax.Tests/`, `Models/` | Guarantee invariants (TongNo == TongCo, Anti-Negative Stock) | Write failing test &rarr; make pass &rarr; refactor cleanly |
| **Prototyper** | `prototype` | Testing complex tax formulas or UI layouts | `scratch/`, test views | Quick sanity check before full implementation | Build throwaway spike script or Razor mock |
| **UI Developer** | `generative_ui` | Visualizing T-accounts, ledger balances, reports | Inline chat, standalone HTML | Clear visual inspection of financial flow | Render interactive debit/credit diagrams and ledger charts |

### 4. Debugging & Conflict Resolution
| Role | Skill | Trigger (When) | Scope (Where) | Purpose (Why) | Action (How) |
|---|---|---|---|---|---|
| **Systematic Debugger**| `systematic-debugging` | Any bug, test failure, or unexpected behavior | Full stack | Isolate root cause before proposing fixes | Observe failure &rarr; form hypothesis &rarr; verify with test &rarr; fix |
| **Domain Debugger** | `diagnosing-bugs` | Balance mismatch, DB dialect quirks, 500 error | Logs, DB queries, controller actions | Trace financial discrepancies | Instrument ledger trails &rarr; pinpoint erroneous posting |
| **Integrator** | `resolving-merge-conflicts`| Git rebase or merge failure | Git workspace | Clean history, preserve accounting logic | Inspect base, ours, theirs &rarr; resolve &rarr; verify build |

### 5. Review & Verification
| Role | Skill | Trigger (When) | Scope (Where) | Purpose (Why) | Action (How) |
|---|---|---|---|---|---|
| **Verifier** | `verification-before-completion` | Before claiming work is done, fixed, or passing | CLI / Build runner | Evidence before assertions | Run `dotnet build` (0 warnings) and `dotnet test` (all pass) |
| **Code Reviewer** | `code-review` | Before merging branch or completing major phase | Git diff, modified files | Verify repo standards (AGENTS.md) and TT99 spec | Run parallel Standards & Spec checks |
| **Review Requester** | `requesting-code-review` | Completing a major feature or pull request | PR / Commit diff | Solicit structured critique | Format diff summary, highlight invariants, invoke reviewer |
| **Review Receiver** | `receiving-code-review` | Review feedback received | Review comments | Rigorous technical validation | Evaluate suggestions technically before blindly modifying code |
| **Release Finisher** | `finishing-a-development-branch`| All tests passing, work verified | Git branch / master | Clean merge and release | Decide integration strategy, rebase/merge, prune branch |

### 6. Operations, Research & Customization
| Role | Skill | Trigger (When) | Scope (Where) | Purpose (Why) | Action (How) |
|---|---|---|---|---|---|
| **Researcher** | `research` | Obscure tax circulars (TT 200, TT 99, VAS 02) | `docs/research/` | Authoritative source validation | Query official sources, write markdown finding summary |
| **Ops Guide** | `wizard` | Manual DB migration, production server setup | CLI / Shell scripts | Guide human through credentials & manual steps | Generate step-by-step interactive CLI script |
| **Prompt Engineer** | `writing-for-agents` | Updating `AGENTS.md`, `GEMINI.md`, or skills | `AGENTS.md`, `.gemini/skills/` | Optimize agent context load & instruction following | Apply progressive disclosure, leading words, positive rules |
| **Skill Author** | `writing-skills` | Creating or modifying reusable skills | `.gemini/skills/` | Standardize agent workflows | Author `SKILL.md`, define triggers, test execution |
| **Tool Configurator**| `agy-customizations`| Adding custom MCP, project-specific rules | `.gemini/antigravity/` | Extend agent capabilities for ninjaTax | Define rule files, hooks, or custom MCP servers |
| **Platform Guide** | `antigravity-guide` | Questions about Antigravity CLI, IDE, configs | Antigravity IDE / CLI | Correct tool configuration and workflow usage | Lookup commands, slash tools, and IDE settings |
| **Workflow Migrator**| `migrate-workflows` | Upgrading legacy workflow files to skills | `.gemini/workflows/` | Modernize agent configuration | Scan legacy scripts &rarr; convert to `SKILL.md` format |
| **Data Engineer** | `karpathy-guidelines`| Tax fraud heuristics, ML invoice OCR | Data pipelines, ML models | Avoid premature abstraction, verify data first | Build dumb baseline &rarr; overfit batch &rarr; measure |
