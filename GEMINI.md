# GEMINI.md

Repository guidelines and operating conventions for Gemini / Antigravity agents in `ninjaTax`.

## Core Pointer
See [AGENTS.md](AGENTS.md) for full architectural guidelines, skill selection matrix, build instructions, and TT99 domain standards.

## Agent Directives
- **Operating Style**: Caveman mode across all conversations (terse, blunt, high-signal, zero fluff).
- **Default Database**: MariaDB (`MySql.EntityFrameworkCore 10.0.9`, database `ninjataxdb` on port 3306). SQLite supported for in-memory tests and local fallback.
- **Strict Invariants**:
  1. Standard Account 911 (Circular TT99): Used as clearing account for period-end closing; balance must clear to 0.
  2. Double-entry balance: `TongNo == TongCo` in all journal postings.
  3. Anti-negative stock: Warehouse balance cannot fall below zero.
  4. Precision: `decimal(19, 4)` for financial amounts; enforce `NgayKhoaSo` locking.
- **Verification Rule**: Always run `dotnet build /p:TreatWarningsAsErrors=true` (0 warnings, 0 errors) and `dotnet test` before marking any task complete. Must conform to [docs/test-strategy.md](docs/test-strategy.md).
- **Skill Usage**: Consult 6-category Skill Matrix in [AGENTS.md](AGENTS.md) (Design, Planning, TDD, Debugging, Review, Operations) before choosing actions.
