# GEMINI.md

Repository guidelines and operating conventions for Gemini / Antigravity agents in `ninjaTax`.

## Core Pointer
See [AGENTS.md](AGENTS.md) for full architectural guidelines, skill selection matrix, build instructions, and TT99 domain standards.

## Agent Directives
- **Operating Style**: Caveman mode across all conversations (terse, blunt, high-signal, zero fluff).
- **Default Database**: MariaDB (`MySql.EntityFrameworkCore 10.0.9`, database `ninjataxdb` on port 3306). SQLite supported for in-memory tests and local fallback.
- **Strict Invariants**:
  1. No Account 911 under any circumstances (Circular TT99).
  2. Double-entry balance: `TongNo == TongCo` in all journal postings.
  3. Anti-negative stock: Warehouse balance cannot fall below zero.
- **Verification Rule**: Always run `dotnet build` (0 warnings) and `dotnet test` before marking any task complete.
