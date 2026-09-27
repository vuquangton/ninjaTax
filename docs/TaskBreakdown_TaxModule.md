# Task Breakdown – Tax Module (ninjaTax)

## Work Breakdown Structure (WBS)
1. **Context Engineering**
   - Ingest AGENTS.md & GEMINI.md (done).
2. **Task Planning**
   - Create detailed WBS & timeline (this file).
   - Define verification checklist.
3. **Source‑Driven Development**
   - Fetch all external URLs (list below).
   - Store raw HTML/text in `docs/RawSources/<domain>/`.
   - Generate manifest `docs/RawSources/manifest.json`.
4. **Spec‑Driven Development**
   - Draft BRD (`docs/BRD_TaxModule.md`).
   - Draft Specs (`docs/Specs_TaxModule.md`).
   - Create Process Flows (`docs/Flows_TaxModule.md`).
   - Produce UI Wireframes (`docs/UI_Wireframes.md`).
   - Assemble Implementation Roadmap (`docs/ImplementationRoadmap.md`).
5. **Review & Verification**
   - Run `dotnet build` & `dotnet test` after each doc.
   - Code‑review docs for standards & spec compliance.
6. **Commit & Sync**
   - Add, commit, push.
   - CodeGraph sync.

## Timeline (2‑week sprints)
- **Sprint 1** (Days 1‑5): Phase 1‑2 complete, start Phase 3 (source fetch).
- **Sprint 2** (Days 6‑10): Complete Phase 3, begin Phase 4 (draft BRD & Specs).
- **Sprint 3** (Days 11‑15): Finish Specs, Flows, Wireframes.
- **Sprint 4** (Days 16‑20): Review, verification, commit, sync.

## Verification Checklist
- Build passes (`dotnet build`, 0 warnings).
- Tests pass (`dotnet test`).
- Legal cross‑check: each regulation cited from latest source.
- Invariants documented (911 ban, double‑entry, anti‑negative, cost seg, S10‑DN).
- ASCII diagrams render.

## URLs to Fetch
- https://ketoanthienung.net
- https://ketoanleanh.edu.vn
- https://webketoan.com
- https://vbpl.vn
- https://mof.gov.vn
- https://thuedientu.gdt.gov.vn
- https://customs.gov.vn
- https://baohiemxahoi.gov.vn
- https://www.reddit.com
- https://dev.to
- https://stackoverflow.com
- https://dichvucong.gov.vn
- https://www.ey.com/vn
- https://www.pwc.com/vn
- https://www.deloitte.com/vn
- https://home.kpmg/vn
- https://www.gdt.gov.vn
- https://www.vacpa.org.vn
- http://vaa.net.vn
- http://www.ifrs.org

**Next**: spawn parallel subagents to fetch URLs.
