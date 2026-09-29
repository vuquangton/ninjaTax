# Trading Enterprise ERP Suite (Phases 13, 14, 15) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `subagent-driven-development` (recommended) or `executing-plans` to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Transform `ninjaTax` into a production-grade commercial & trading accounting platform conforming to Vietnamese Accounting Standards (VAS 02, VAS 03, VAS 10, VAS 14, Circular TT99/2025/TT-BTC, Circular 48/2019/TT-BTC, Decree 123/2020) and matching Tier-1 Vietnamese ERP capabilities (MISA AMIS, FAST Business, BRAVO 8R3).

**Architecture:** 
- Phased incremental delivery ordered strictly from Core to Edge (Phase 13: Core Landed Cost & Valuation $\rightarrow$ Phase 14: Distribution & Adjustments $\rightarrow$ Phase 15: Treasury, Aging & Forex).
- Thin controllers delegating all ledger postings, validation, and domain math to specialized service interfaces.
- Real-time accounting invariants: double-entry equality (`TongNo == TongCo`), lock-date protection (`NgayKhoaSo`), anti-negative stock guards, and zero-balance clearing for intermediate accounts (911, 413).

**Tech Stack:** ASP.NET Core MVC (.NET 10), EF Core 10, MariaDB 12+ / SQLite, Tailwind CSS, xUnit.

**Specs:** 
- `docs/PHASE13_LANDED_COST_AND_INVENTORY_VALUATION_BRD_SPEC.md`
- `docs/PHASE14_MULTI_UOM_AND_COMMERCIAL_ADJUSTMENTS_BRD_SPEC.md`
- `docs/PHASE15_DEBT_AGING_PROVISION_AND_MULTI_CURRENCY_BRD_SPEC.md`

---

## Global Constraints

- Zero compiler warnings policy (`TreatWarningsAsErrors = true`).
- Currency and conversion rates precision strictly stored as `decimal(19, 4)`.
- Respect accounting book lock date (`NgayKhoaSo`): no mutations on or before locked date.
- Anti-negative stock invariant (VAS 02): warehouse inventory balance cannot drop below 0.
- All period-end clearing vouchers must balance to zero (`TongNo == TongCo`).

---

## Review Focus

1. **Penny-Rounding Discrepancy in Landed Cost**: When allocating landed cost across line items, rounding cannot cause $\sum \text{allocated} \neq \text{total expense}$; remainder must be absorbed by the largest line item.
2. **Post-Recalculation Inventory Value Integrity**: Period-end weighted average recalculation must never push ending stock value below 0 when cumulative qty > 0.
3. **Internal Transfer Tax Exemption**: Internal transfers (1561 $\rightarrow$ 1561) must not trigger VAT (TK 133/3331) or revenue/expense recognition.
4. **Sales Return COGS Reversal**: Returning goods must reverse both revenue/tax (TK 5212, 33311) and inventory cost (Nợ 1561 / Có 632) at original issue cost.
5. **Account 413 Zero Ending Balance**: After period-end balance sheet preparation, TK 413 must completely clear to TK 515 (lãi) or TK 635 (lỗ).

---

# PHASE 13: LANDED COST ALLOCATION, PERIOD-END VALUATION & AUTOMATED VAT CLEARING

## Task 13.1: Landed Cost Allocation Engine (Chi Phí Mua Hàng & Phân Bổ Nhập Kho)

**Files:**
- Create: `Models/Entities/ChungTuChiPhiMuaHang.cs`
- Create: `Models/Entities/ChiPhiMuaHangPhanBo.cs`
- Modify: `Models/Entities/ChiTietNhapKho.cs` (add `ChiPhiMuaHangPhanBo`, `DonGiaSauPhanBo`)
- Modify: `Data/AppDbContext.cs` (register DbSets and relations)
- Create: `Models/Services/ILandedCostService.cs`
- Create: `Models/Services/LandedCostService.cs`
- Create: `ninjaTax.Tests/LandedCostServiceTests.cs`

**Steps:**
- [ ] 1. Write failing xUnit test in `LandedCostServiceTests.cs`:
  - Test allocation by Value (`PhanBoTheoGiaTri`): $1,000,000$ VND freight allocated to 2 items ($10,000,000$ and $30,000,000$) yields $250,000$ and $750,000$.
  - Test allocation by Quantity (`PhanBoTheoSoLuong`): $100,000$ VND allocated to $20$ items and $80$ items yields $20,000$ and $80,000$.
  - Test penny-rounding absorption: ensure sum of line allocations matches header amount exactly.
  - Test journal voucher generation: Nợ 1561 / Có 331 (or 111/112) with `TongNo == TongCo`.
- [ ] 2. Run `dotnet test` to confirm test failures.
- [ ] 3. Create entities `ChungTuChiPhiMuaHang`, `ChiPhiMuaHangPhanBo`, and update `ChiTietNhapKho`.
- [ ] 4. Register in `AppDbContext.cs` and configure precision `decimal(19, 4)`.
- [ ] 5. Implement `ILandedCostService` and `LandedCostService.cs` with validation, rounding distribution, and journal voucher creation via `IButToanService`.
- [ ] 6. Register `ILandedCostService` in `Program.cs`.
- [ ] 7. Run `dotnet test` to confirm all tests pass.
- [ ] 8. Commit: `feat(phase13): implement LandedCostService with value/quantity allocation and journal generation`.

---

## Task 13.2: Period-End Weighted Average Inventory Cost Recalculation Engine (Tính Giá Xuất Kho Kỳ)

**Files:**
- Modify: `Models/Services/IInventoryService.cs` (add `RecalculatePeriodWeightedAverageCostAsync`)
- Modify: `Models/Services/InventoryService.cs`
- Modify: `Models/Entities/ChiTietXuatKho.cs` (add `DonGiaVonCuoiKy`, `ChenhLechGiaVon`)
- Create: `ninjaTax.Tests/PeriodEndValuationTests.cs`

**Steps:**
- [ ] 1. Write failing xUnit test in `PeriodEndValuationTests.cs`:
  - Beginning stock: 10 items @ 100,000 VND.
  - Receipts in period: 20 items @ 130,000 VND.
  - Total available: 30 items @ 120,000 VND weighted average.
  - Issues during period: 15 items initially issued @ 100,000 VND (COGS = 1,500,000).
  - Recalculation should update issue unit cost to 120,000 VND, adjust COGS by +300,000 VND, and generate/update GL voucher for TK 632 / TK 1561.
  - Test locked date guard: throws `InvalidOperationException` if period falls within `NgayKhoaSo`.
- [ ] 2. Run `dotnet test` to confirm failure.
- [ ] 3. Implement `RecalculatePeriodWeightedAverageCostAsync` in `InventoryService.cs`:
  - Calculate period rate: $ĐG_{BQ} = \frac{\text{Giá trị Tồn ĐK} + \text{Giá trị Nhập TK}}{\text{Số lượng Tồn ĐK} + \text{Số lượng Nhập TK}}$.
  - Update all `ChiTietXuatKho` within date range.
  - Adjust related `ChiTietButToan` (Nợ 632 / Có 1561) to preserve double-entry equality.
- [ ] 4. Run `dotnet test` to confirm green.
- [ ] 5. Commit: `feat(phase13): implement period-end weighted average inventory cost recalculation engine`.

---

## Task 13.3: Automated Periodic VAT Clearing (Khấu Trừ Thuế GTGT Tự Động)

**Files:**
- Modify: `Models/Services/IPeriodClosingService.cs` (add `KhauTruThueGtgtAsync`)
- Modify: `Models/Services/PeriodClosingService.cs`
- Create: `ninjaTax.Tests/VatClearingServiceTests.cs`

**Steps:**
- [ ] 1. Write failing xUnit test in `VatClearingServiceTests.cs`:
  - Scenario 1: Input VAT (1331) = 50,000,000 VND, Output VAT (33311) = 70,000,000 VND $\rightarrow$ Voucher Nợ 33311 / Có 1331 = 50,000,000 VND (33311 còn phải nộp 20,000,000).
  - Scenario 2: Input VAT (1331) = 80,000,000 VND, Output VAT (33311) = 30,000,000 VND $\rightarrow$ Voucher Nợ 33311 / Có 1331 = 30,000,000 VND (1331 còn được khấu trừ 50,000,000).
  - Verify voucher format: `PKT-KT-THUE-YYYYMM`.
  - Idempotency test: re-running VAT clearing replaces previous clearing voucher.
- [ ] 2. Run `dotnet test` to confirm failure.
- [ ] 3. Implement `KhauTruThueGtgtAsync` in `PeriodClosingService.cs` and wire it into the start of `TaoButToanKetChuyenAsync`.
- [ ] 4. Run `dotnet test` to confirm all tests pass.
- [ ] 5. Commit: `feat(phase13): implement automated periodic VAT clearing voucher generation`.

---

## Task 13.4: Phase 13 UI & Integration Slice (Controllers & Views)

**Files:**
- Create: `Controllers/LandedCostController.cs`
- Create: `Models/ViewModels/LandedCostViewModels.cs`
- Create: `Views/LandedCost/Index.cshtml`, `Create.cshtml`, `Details.cshtml`
- Modify: `Controllers/GeneralLedgerController.cs` (add action to trigger inventory cost recalculation)
- Modify: `Views/GeneralLedger/KetChuyen.cshtml` (add VAT clearing & inventory recalculation buttons)
- Modify: `wwwroot/data/menu.json` (add Landed Cost menu item under Mua Hàng)

**Steps:**
- [ ] 1. Build `LandedCostViewModels` and `LandedCostController` with clean validation.
- [ ] 2. Create Razor views using semantic theme classes (`bg-[var(--card)]`, `text-[var(--foreground)]`).
- [ ] 3. Update `GeneralLedgerController` to expose trigger endpoints for period-end costing and VAT clearing.
- [ ] 4. Verify compiler warnings: `dotnet build /p:TreatWarningsAsErrors=true` (must be 0 warnings).
- [ ] 5. Run full test suite: `dotnet test`.
- [ ] 6. Commit: `feat(phase13): add Landed Cost UI, period recalculation action, and menu links`.

---

# PHASE 14: MULTI-UOM CONVERSION, INTERNAL TRANSFERS & COMMERCIAL ADJUSTMENTS

## Task 14.1: Multi-UoM Unit Conversion Module (Đơn Vị Tính Phụ & Quy Đổi)

**Files:**
- Create: `Models/Entities/DonViTinhQuyDoi.cs`
- Modify: `Models/Entities/VatTuHangHoa.cs` (add collection `DonViTinhQuyDois`)
- Modify: `Data/AppDbContext.cs`
- Create: `Models/Services/IUnitConversionService.cs`
- Create: `Models/Services/UnitConversionService.cs`
- Create: `ninjaTax.Tests/UnitConversionServiceTests.cs`

**Steps:**
- [ ] 1. Write failing xUnit test in `UnitConversionServiceTests.cs`:
  - 1 Thùng = 24 Lon (TyLeQuyDoi = 24).
  - Convert 5 Thùng $\rightarrow$ 120 Lon (base unit).
  - Convert 48 Lon $\rightarrow$ 2 Thùng.
  - Validate non-negative conversion rate and duplicate unit prohibition per item.
- [ ] 2. Run `dotnet test` to confirm failure.
- [ ] 3. Create `DonViTinhQuyDoi` entity and register in `AppDbContext.cs`.
- [ ] 4. Implement `UnitConversionService.cs` and wire into `Program.cs`.
- [ ] 5. Run `dotnet test` to confirm all tests pass.
- [ ] 6. Commit: `feat(phase14): implement Multi-UoM conversion service and entity relations`.

---

## Task 14.2: Internal Warehouse Stock Transfer (Phiếu Điều Chuyển Kho Nội Bộ Mẫu 03-VT)

**Files:**
- Create: `Models/Entities/PhieuDieuChuyenKho.cs`
- Create: `Models/Entities/ChiTietDieuChuyenKho.cs`
- Modify: `Data/AppDbContext.cs`
- Modify: `Models/Services/IInventoryService.cs` (add stock transfer methods)
- Modify: `Models/Services/InventoryService.cs`
- Create: `ninjaTax.Tests/StockTransferTests.cs`

**Steps:**
- [ ] 1. Write failing xUnit test in `StockTransferTests.cs`:
  - Transfer 10 items from Warehouse A to Warehouse B.
  - Verify stock availability check at source warehouse (anti-negative stock).
  - Verify journal posting: Nợ 1561 (Kho B) / Có 1561 (Kho A). Total assets unchanged.
  - Verify cancellation flow restores stock levels safely.
- [ ] 2. Run `dotnet test` to confirm failure.
- [ ] 3. Implement entities `PhieuDieuChuyenKho`, `ChiTietDieuChuyenKho` and update `InventoryService.cs`.
- [ ] 4. Run `dotnet test` to confirm green.
- [ ] 5. Commit: `feat(phase14): implement internal warehouse stock transfer voucher with GL posting`.

---

## Task 14.3: Commercial Adjustments Engine (Hàng Bán/Mua Trả Lại & Chiết Khấu 5211/5212)

**Files:**
- Create: `Models/Entities/ChungTuDieuChinhThuongMai.cs` (Hàng bán trả lại, hàng mua trả lại, chiết khấu)
- Create: `Models/Entities/ChiTietDieuChinhThuongMai.cs`
- Modify: `Data/AppDbContext.cs`
- Create: `Models/Services/ICommercialAdjustmentService.cs`
- Create: `Models/Services/CommercialAdjustmentService.cs`
- Create: `ninjaTax.Tests/CommercialAdjustmentServiceTests.cs`

**Steps:**
- [ ] 1. Write failing xUnit test in `CommercialAdjustmentServiceTests.cs`:
  - Sales Return: Nợ 5212, Nợ 33311 / Có 131; Cost reversal: Nợ 1561 / Có 632.
  - Trade Discount: Nợ 5211, Nợ 33311 / Có 131.
  - Purchase Return: Nợ 331 / Có 1561, Có 1331.
  - Guard: cannot return more than original invoice quantity.
- [ ] 2. Run `dotnet test` to confirm failure.
- [ ] 3. Implement `CommercialAdjustmentService.cs` and wire into `Program.cs`.
- [ ] 4. Run `dotnet test` to confirm pass.
- [ ] 5. Commit: `feat(phase14): implement commercial adjustment service for returns and trade discounts`.

---

## Task 14.4: Phase 14 UI & View Integration

**Files:**
- Create: `Controllers/DieuChuyenKhoController.cs`
- Create: `Controllers/DieuChinhThuongMaiController.cs`
- Create: `Views/DieuChuyenKho/Index.cshtml`, `Create.cshtml`, `Details.cshtml`
- Create: `Views/DieuChinhThuongMai/Index.cshtml`, `Create.cshtml`
- Modify: `Views/VatTuHangHoa/Edit.cshtml` (add Multi-UoM conversion management table)
- Modify: `wwwroot/data/menu.json` (add Stock Transfer and Commercial Adjustments)

**Steps:**
- [ ] 1. Build controllers and Razor views.
- [ ] 2. Add UoM management sub-table inside `VatTuHangHoa/Edit.cshtml`.
- [ ] 3. Verify zero compiler warnings: `dotnet build /p:TreatWarningsAsErrors=true`.
- [ ] 4. Run all unit and integration tests: `dotnet test`.
- [ ] 5. Commit: `feat(phase14): add UI for stock transfer, commercial adjustments, and multi-uom`.

---

# PHASE 15: AR/AP CROSS-CLEARING, AGING ANALYSIS, TT48 PROVISIONING & MULTI-CURRENCY

## Task 15.1: AR/AP Cross-Clearing Engine (Bù Trừ Công Nợ 131 <-> 331 Cùng Đối Tượng)

**Files:**
- Create: `Models/Entities/ChungTuBuTruCongNo.cs`
- Modify: `Data/AppDbContext.cs`
- Modify: `Models/Services/ICongNoService.cs` (add `BuTruCongNoHaiChieuAsync`)
- Modify: `Models/Services/CongNoService.cs`
- Create: `ninjaTax.Tests/DebtClearingTests.cs`

**Steps:**
- [ ] 1. Write failing xUnit test in `DebtClearingTests.cs`:
  - Partner A owes 100M VND (Nợ 131) and Company owes Partner A 60M VND (Có 331).
  - Cross-clearing voucher created for 60M VND: Nợ 331 / Có 131.
  - Result: 331 balance becomes 0; 131 remaining balance becomes 40M VND.
  - Guard: clearing amount cannot exceed $\min(\text{Nợ 131}, \text{Có 331})$.
- [ ] 2. Run `dotnet test` to confirm failure.
- [ ] 3. Implement entity and `BuTruCongNoHaiChieuAsync` in `CongNoService.cs`.
- [ ] 4. Run `dotnet test` to confirm pass.
- [ ] 5. Commit: `feat(phase15): implement AR/AP cross-clearing engine for dual-role counterparties`.

---

## Task 15.2: Comprehensive Debt Aging Analysis & TT 48/2019 Bad Debt Provisioning

**Files:**
- Modify: `Models/ViewModels/CongNoViewModels.cs` (add aging bucket view models & provision models)
- Modify: `Models/Services/ICongNoService.cs` (add `TinhDuPhongKhoDoiTT48Async`, `TaoButToanTrichLapDuPhongAsync`)
- Modify: `Models/Services/CongNoService.cs`
- Create: `ninjaTax.Tests/BadDebtProvisionTests.cs`

**Steps:**
- [ ] 1. Write failing xUnit test in `BadDebtProvisionTests.cs`:
  - Test aging buckets: 0-30, 31-60, 61-90, 91-180, 181-360, >360 days.
  - Test TT 48 statutory provision rates:
    * Overdue 6 to <12 months: 30%
    * Overdue 1 to <2 years: 50%
    * Overdue 2 to <3 years: 70%
    * Overdue $\ge$ 3 years: 100%
  - Test provision voucher creation: Nợ 6422 / Có 2293.
  - Test reversal voucher when debt is collected: Nợ 2293 / Có 6422.
- [ ] 2. Run `dotnet test` to confirm failure.
- [ ] 3. Implement logic in `CongNoService.cs`.
- [ ] 4. Run `dotnet test` to confirm all tests pass.
- [ ] 5. Commit: `feat(phase15): implement TT 48 bad debt provision calculation and voucher generation`.

---

## Task 15.3: Multi-Currency & Period-End Foreign Exchange Revaluation (VAS 10 & TK 413)

**Files:**
- Create: `Models/Entities/ChungTuDanhGiaNgoaiTe.cs`
- Modify: `Data/AppDbContext.cs`
- Create: `Models/Services/IMultiCurrencyService.cs`
- Create: `Models/Services/MultiCurrencyService.cs`
- Create: `ninjaTax.Tests/MultiCurrencyServiceTests.cs`

**Steps:**
- [ ] 1. Write failing xUnit test in `MultiCurrencyServiceTests.cs`:
  - Bank USD account (TK 1122): Book balance $10,000$ USD @ $24,500$ = $245,000,000$ VND.
  - Year-end closing exchange rate = $25,200$ VND/USD (Actual value = $252,000,000$ VND).
  - Gain revaluation: Nợ 1122 / Có 413 = 7,000,000 VND.
  - Final clearing step before B01-DN: Nợ 413 / Có 515 = 7,000,000 VND. TK 413 clears to 0 balance.
- [ ] 2. Run `dotnet test` to confirm failure.
- [ ] 3. Implement `MultiCurrencyService.cs` with exchange rate lookup and revaluation posting.
- [ ] 4. Run `dotnet test` to confirm pass.
- [ ] 5. Commit: `feat(phase15): implement VAS 10 multi-currency revaluation engine via Account 413`.

---

## Task 15.4: Phase 15 UI, Reports & Menu Wiring

**Files:**
- Modify: `Controllers/CongNoController.cs` (add actions: `BuTru`, `BaoCaoTuoiNoChiTiet`, `DuPhongKhoDoi`)
- Create: `Controllers/NgoaiTeController.cs`
- Create: `Views/CongNo/BuTru.cshtml`, `BaoCaoTuoiNoChiTiet.cshtml`, `DuPhongKhoDoi.cshtml`
- Create: `Views/NgoaiTe/Index.cshtml`, `DanhGiaLai.cshtml`
- Modify: `wwwroot/data/menu.json` (add Aging & Forex menu items)

**Steps:**
- [ ] 1. Implement UI views with data tables, aging progress bars, and provision proposal action cards.
- [ ] 2. Verify build: `dotnet build /p:TreatWarningsAsErrors=true` (0 warnings).
- [ ] 3. Run complete application test suite: `dotnet test`.
- [ ] 4. Commit: `feat(phase15): add UI for AR/AP cross-clearing, aging reports, and forex revaluation`.

---

## Final Verification & Exit Criteria (Docs/test-strategy.md)

1. **Compilation**: `dotnet build /p:TreatWarningsAsErrors=true` $\rightarrow$ 0 warnings, 0 errors.
2. **Test Suite**: `dotnet test` $\rightarrow$ 100% pass rate (165 baseline + ~50 new tests $\approx$ 215 tests).
3. **Statutory TT99 Audit**:
   - TK 911 balance clears to 0.
   - TK 413 balance clears to 0.
   - Anti-negative stock prevents illegal out-of-stock warehouse postings.
   - B01-DN, B02-DN, and 8-column Trial Balance remain strictly reconciled.
4. **CodeGraph**: Run `codegraph sync` and verify zero broken references.
