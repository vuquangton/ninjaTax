# AGENTS.md

Repository guidelines for autonomous agents working in `ninjaTax`.

## Overview
- ASP.NET Core MVC web application on .NET 10 (`net10.0`).
- C# nullable reference types and implicit usings enabled.

## Commands
- **Build**: `dotnet build`
- **Run**: `dotnet run` (listens on `http://localhost:5188`)
- **Clean**: `dotnet clean`
- **Test**: `dotnet test` (when test projects are present)

## Architecture & Layout
- `Controllers/`: ASP.NET Core MVC controllers.
- `Models/`: Domain entities, request/response models, and view models.
- `Views/`: Razor views (`.cshtml`) and layout templates.
- `wwwroot/`: Static assets (CSS, JS, vendor libraries).
- `Properties/launchSettings.json`: Profiles and local development port bindings.

## Code Standards
- Adhere to idiomatic C# and ASP.NET Core MVC patterns.
- Treat compiler warnings as errors; keep builds warning-free.
- Maintain nullable annotations accurately; avoid suppressing nullability (`!`) without invariant checks.
- Keep controllers thin; delegate business and calculation logic to service or domain layers in `Models/` or dedicated service classes.
- Use async/await for I/O and external operations.

## Agent Workflow
- Verify any code modification with `dotnet build` before reporting completion.
- Keep edits localized and preserve existing file structures and configurations.

## Skill Selection Matrix (5W1H)

| Who (Role) | What (Skill) | When (Trigger) | Where (Scope) | Why (Purpose) | How (Action) |
|---|---|---|---|---|---|
| **Domain Architect** | `domain-modeling` | Defining new TT99 entities, accounts, tax rules | `Models/Entities/`, ADRs | Prevent terminology drift, align VAS standards | Map ubiquitous terms, document invariants & boundaries |
| **Software Architect** | `codebase-design` | Structuring multi-DB seams, service interfaces | `Models/Services/`, `Data/` | Keep interfaces deep, implementations thin | Define contracts (`IButToanService`), isolate DB dialects |
| **Developer** | `tdd` | Building calculation logic, journals, VAT rules | `ninjaTax.Tests/`, `Models/` | Guarantee double-entry invariants (TongNo == TongCo) | Write failing unit test &rarr; pass &rarr; refactor |
| **UI Developer** | `generative_ui` | Visualizing T-accounts, ledger balances, reports | Inline chat, standalone HTML | Clear visual inspection of financial flow | Render interactive debit/credit diagrams and ledger charts |
| **Prototyper** | `prototype` | Testing complex tax formulas or UI layouts | `scratch/`, test views | Quick sanity check before full implementation | Build throwaway spike script or Razor mock |
| **Auditor / Reviewer** | `grilling` | Validating accounting flows, period closing rules | PR plans, architecture decisions | Expose edge cases (e.g. 911 usage, precision loss) | Relentlessly question assumptions and invariants |
| **Reviewer** | `code-review` | Before merging branch or completing major phase | Git diff, modified files | Verify repo standards (AGENTS.md) and TT99 spec | Run parallel Standards & Spec checks |
| **Debugger** | `diagnosing-bugs` | Balance mismatch, SQLite precision bug, 500 error | Logs, DB queries, controller actions | Find root cause without guessing | Form testable hypothesis &rarr; instrument &rarr; isolate bug |
| **Integrator** | `resolving-merge-conflicts`| Git rebase or merge failure | Git workspace | Clean history, preserve accounting logic | Inspect base, ours, theirs &rarr; resolve &rarr; verify build |
| **Researcher** | `research` | Checking obscure tax circulars or EF Core quirks | `docs/research/` | Authoritative source validation | Query official sources, write markdown finding summary |
| **Ops Guide** | `wizard` | Manual DB migration, production server setup | CLI / Shell scripts | Guide human through credentials & manual steps | Generate step-by-step interactive CLI script |
| **Prompt Engineer** | `writing-for-agents` | Updating `AGENTS.md`, `GEMINI.md`, or skills | `AGENTS.md`, `.gemini/skills/` | Optimize agent context load & instruction following | Apply progressive disclosure, leading words, positive rules |
| **Tool Configurator**| `agy-customizations`| Adding custom MCP, project-specific rules | `.gemini/antigravity/` | Extend agent capabilities for ninjaTax | Define rule files, hooks, or custom MCP servers |
| **Platform Guide** | `antigravity-guide` | Questions about Antigravity CLI, IDE, configs | Antigravity IDE / CLI | Correct tool configuration and workflow usage | Lookup commands, slash tools, and IDE settings |
| **Workflow Migrator**| `migrate-workflows` | Upgrading legacy workflow files to skills | `.gemini/workflows/` | Modernize agent configuration | Scan legacy scripts &rarr; convert to `SKILL.md` format |
| **Data Engineer** | `karpathy-guidelines`| Tax fraud heuristics, ML invoice OCR | Data pipelines, ML models | Avoid premature abstraction, verify data first | Build dumb baseline &rarr; overfit batch &rarr; measure |

