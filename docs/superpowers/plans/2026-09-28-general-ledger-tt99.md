# General Ledger (GL) TT99 Module Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement an end-to-end General Ledger (GL) module strictly complying with Circular TT99/2025/TT-BTC, featuring automated period closing without Account 911, strict fiscal lock enforcement, General Journal (S03a-DN), General Ledger (S03b-DN), Trial Balance (8 columns), and subledger reconciliation.

**Architecture:** Domain service layer design with `IPeriodClosingService`, `IGeneralLedgerService`, and `ISubledgerReconciliationService` backed by EF Core multi-DB repositories, consumed by `GeneralLedgerController` with AG Grid views, guarded by strict invariants.

**Tech Stack:** ASP.NET Core MVC .NET 10, C# 14, EF Core 10.0.9 (MariaDB/SQLite), AG Grid Community 33.1.1, xUnit, FluentAssertions.

**Spec:** [docs/PHASE9_GENERAL_LEDGER_TT99_BRD_SPEC.md](file:///d:/petProjs/ninjaTax/docs/PHASE9_GENERAL_LEDGER_TT99_BRD_SPEC.md)

## Global Constraints
- Absolute ban on Account 911 (`TK 911`).
- Double-entry balance invariant: `TongNo == TongCo` in every voucher.
- Zero decimal drift: All monetary amounts represented as `decimal(19, 4)`.
- Zero compiler warnings (`TreatWarningsAsErrors = true`).
- 100% test pass rate across entire suite.

## Review Focus
1. Period closing logic with zero balances in revenue/expense accounts.
2. Direct transfer of revenue (511, 515, 711) and expense (632, 635, 641, 642, 811, 821) into Account 4212.
3. Period lock guard preventing any back-dated posting, modification, or deletion.
4. Correctness of 8-column Trial Balance calculations (Opening Dr/Cr, Period Dr/Cr, Closing Dr/Cr).
5. Subledger vs GL reconciliation detecting any delta between AR (131), AP (331), Inventory (152, 1561) and GL balances.

---

### Task 1: ViewModels and Interfaces for Period Closing & General Ledger

**Files:**
- Create: `Models/ViewModels/GeneralLedgerViewModels.cs`
- Create: `Models/Services/IPeriodClosingService.cs`
- Create: `Models/Services/IGeneralLedgerService.cs`
- Create: `Models/Services/ISubledgerReconciliationService.cs`

**Interfaces:**
- Produces: `IPeriodClosingService`, `IGeneralLedgerService`, `ISubledgerReconciliationService`, and ViewModels (`SoNhatKyChungViewModel`, `SoCaiViewModel`, `BangCanDoiTaiKhoanViewModel`, `SubledgerReconciliationReportViewModel`).

- [ ] **Step 1: Create ViewModels**
Define view models for S03a-DN, S03b-DN, Trial Balance (8 columns), Period Closing result, and Subledger Reconciliation report.

- [ ] **Step 2: Create Service Interfaces**
Define contracts for `IPeriodClosingService`, `IGeneralLedgerService`, and `ISubledgerReconciliationService`.

- [ ] **Step 3: Verify Compilation**
Run `dotnet build` to ensure all types compile cleanly with 0 warnings.

---

### Task 2: Implement Period Closing Service (TDD)

**Files:**
- Create: `ninjaTax.Tests/PeriodClosingTt99Tests.cs`
- Create: `Models/Services/PeriodClosingService.cs`
- Modify: `Program.cs` (Register `IPeriodClosingService`)

**Interfaces:**
- Consumes: `AppDbContext`, `IButToanService`, `CauHinhKeToan`, `TaiKhoan`
- Produces: `KetChuyenCuoiKyResult TaoButToanKetChuyenAsync(int nam, int? thang)`

- [ ] **Step 1: Write failing tests in `PeriodClosingTt99Tests.cs`**
Test closing entries for revenue/expenses directly to 4212 without 911, and check that revenue/expense accounts have 0 balance post-closing.

- [ ] **Step 2: Run test to verify it fails**
Run `dotnet test --filter FullyQualifiedName~PeriodClosingTt99Tests`
Expected: FAIL (service not found / not implemented)

- [ ] **Step 3: Implement `PeriodClosingService.cs`**
Query all unclosed balances for classes 5, 6, 7, 8; construct single balanced `ButToan` with lines to 4212; save and post transaction.

- [ ] **Step 4: Run test to verify it passes**
Run `dotnet test --filter FullyQualifiedName~PeriodClosingTt99Tests`
Expected: PASS

- [ ] **Step 5: Register service in `Program.cs` and verify full suite**
Run `dotnet test`

---

### Task 3: Implement General Ledger Service (TDD)

**Files:**
- Create: `ninjaTax.Tests/GeneralLedgerReportTests.cs`
- Create: `Models/Services/GeneralLedgerService.cs`
- Modify: `Program.cs` (Register `IGeneralLedgerService`)

**Interfaces:**
- Consumes: `AppDbContext`, `TaiKhoan`, `ChiTietButToan`
- Produces: `SoNhatKyChungViewModel`, `SoCaiViewModel`, `BangCanDoiTaiKhoanViewModel`

- [ ] **Step 1: Write failing tests in `GeneralLedgerReportTests.cs`**
Test S03a-DN query, S03b-DN calculation of opening balance, period movements, running balance, and 8-column Trial Balance equality (TongNo == TongCo for all 3 pairs of columns).

- [ ] **Step 2: Run test to verify it fails**
Run `dotnet test --filter FullyQualifiedName~GeneralLedgerReportTests`
Expected: FAIL

- [ ] **Step 3: Implement `GeneralLedgerService.cs`**
Implement ledger extraction, running balance algorithm, and 8-column Trial Balance builder.

- [ ] **Step 4: Run test to verify it passes**
Run `dotnet test --filter FullyQualifiedName~GeneralLedgerReportTests`
Expected: PASS

- [ ] **Step 5: Register service in `Program.cs` and verify full suite**
Run `dotnet test`

---

### Task 4: Implement Subledger Reconciliation Service (TDD)

**Files:**
- Create: `ninjaTax.Tests/SubledgerReconciliationTests.cs`
- Create: `Models/Services/SubledgerReconciliationService.cs`
- Modify: `Program.cs` (Register `ISubledgerReconciliationService`)

**Interfaces:**
- Consumes: `AppDbContext`, AR (`DoiTruCongNo`, `HoaDonBanHang`), AP (`HoaDonMuaHang`), Inventory (`VatTuHangHoa`, `PhieuNhapKho`, `PhieuXuatKho`), GL (`ChiTietButToan`)
- Produces: `SubledgerReconciliationReportViewModel`

- [ ] **Step 1: Write failing tests in `SubledgerReconciliationTests.cs`**
Test detection of matching vs mismatching balances between AR subledger and 131, AP subledger and 331, Warehouse stock value and 1561.

- [ ] **Step 2: Run test to verify it fails**
Run `dotnet test --filter FullyQualifiedName~SubledgerReconciliationTests`
Expected: FAIL

- [ ] **Step 3: Implement `SubledgerReconciliationService.cs`**
Query subledger totals, query GL account balances as of cutoff date, calculate delta and variance items.

- [ ] **Step 4: Run test to verify it passes**
Run `dotnet test --filter FullyQualifiedName~SubledgerReconciliationTests`
Expected: PASS

- [ ] **Step 5: Verify full test suite**
Run `dotnet test`

---

### Task 5: Web UI Controller, Razor Views & Navigation Sync

**Files:**
- Create: `Controllers/GeneralLedgerController.cs`
- Create: `Views/GeneralLedger/Index.cshtml` (Dashboard / Sổ Nhật ký chung S03a-DN)
- Create: `Views/GeneralLedger/SoCai.cshtml` (Sổ Cái S03b-DN với bộ lọc tài khoản)
- Create: `Views/GeneralLedger/BangCanDoi.cshtml` (Bảng Cân đối tài khoản 8 cột)
- Create: `Views/GeneralLedger/KetChuyenCuoiKy.cshtml` (Giao diện kích hoạt kết chuyển cuối kỳ)
- Create: `Views/GeneralLedger/DoiSoat.cshtml` (Đối soát Subledger vs GL)
- Modify: `wwwroot/data/menu.json` (Thêm menu Sổ Cái Tổng Hợp)

- [ ] **Step 1: Implement `GeneralLedgerController.cs`**
Thin controller handling actions: `Index`, `SoCai`, `BangCanDoi`, `KetChuyenCuoiKy` (GET/POST), `DoiSoat`.

- [ ] **Step 2: Create Razor Views**
Build clean UI with AG Grid for tables, summary cards, and action toolbars with keyboard shortcuts.

- [ ] **Step 3: Update `menu.json`**
Add "Tổng Hợp & Sổ Cái" with submenus: Sổ Nhật ký chung, Sổ Cái (S03b-DN), Bảng Cân đối (Trial Balance), Kết chuyển cuối kỳ, Đối soát sổ phụ.

- [ ] **Step 4: Run Build and Test Suite**
Ensure `dotnet build` (0 warnings) and `dotnet test` pass completely.

---

### Task 6: Code Review, Quality Verification & Documentation

**Files:**
- Review: all modified and created files
- Execute: `dotnet test` and `dotnet build`
- Sync: Codegraph and Git commit
