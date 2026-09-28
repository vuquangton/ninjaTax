# KẾ HOẠCH TRIỂN KHAI CHI TIẾT (IMPLEMENTATION PLAN)
## CHUẨN HÓA TÀI KHOẢN 911 & BÓC TÁCH LƯỠNG TÍNH THEO THÔNG TƯ 99/2025/TT-BTC

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hiệu chỉnh toàn diện codebase để tuân thủ 100% Thông tư 99/2025/TT-BTC của Bộ Tài chính: Đưa Tài khoản 911 vào quy trình kết chuyển cuối kỳ (đảm bảo số dư cuối kỳ = 0), và bóc tách hai chiều tài khoản lưỡng tính (131, 331) trên Bảng Cân đối tài khoản 8 cột.

**Architecture:** Mở rộng `LoaiTaiKhoan` enum, seed TK 911 trong `DbInitializer`, nâng cấp `PeriodClosingService` thành quy trình 3 chặng qua TK 911, hoàn thiện `GeneralLedgerService` bóc tách lưỡng tính đa đối tượng.

**Tech Stack:** ASP.NET Core MVC .NET 10, C# 14, EF Core 10.0.9, xUnit.

**Spec:** [docs/PHASE10_TT99_ACCOUNT_911_STANDARDIZATION_BRD_SPEC.md](file:///d:/petProjs/ninjaTax/docs/PHASE10_TT99_ACCOUNT_911_STANDARDIZATION_BRD_SPEC.md)

## Global Constraints
- Bắt buộc tuân thủ Thông tư 99/2025/TT-BTC.
- TK 911 phải có số dư cuối kỳ bằng 0 (`TongDuNo == 0 && TongDuCo == 0`).
- Cân đối kép: `TongNo == TongCo` trên mọi chứng từ.
- 0 compiler warnings (`TreatWarningsAsErrors = true`).
- 100% test suite xUnit passed.

## Review Focus
1. TK 911 có số dư cuối kỳ = 0 sau khi chạy kết chuyển.
2. Tổng phát sinh Nợ của TK 911 bằng Tổng phát sinh Có của TK 911.
3. TK 131 và 331 bóc tách đồng thời cả 2 bên Nợ và Có trên Bảng Cân đối 8 Cột.
4. Cập nhật các quy tắc hướng dẫn agent trong `AGENTS.md` và `GEMINI.md`.

---

### Task 1: Cập nhật Nguyên tắc Repo & Tài liệu Định hướng (AGENTS.md & GEMINI.md)

**Files:**
- Modify: `AGENTS.md`
- Modify: `GEMINI.md`

- [ ] **Step 1: Cập nhật AGENTS.md**
Thay thế quy tắc cấm 911 bằng quy tắc chuẩn hóa: TK 911 là tài khoản trung gian kết chuyển cuối kỳ của TT99/2025/TT-BTC, bắt buộc đóng sạch số dư cuối kỳ về 0.

- [ ] **Step 2: Cập nhật GEMINI.md**
Đồng bộ nguyên tắc kế toán trong `GEMINI.md`.

- [ ] **Step 3: Commit**
`git commit -m "docs: update AGENTS.md and GEMINI.md to standardize Account 911 under TT99"`

---

### Task 2: Cập nhật Entity `TaiKhoan` và Seeding Data `DbInitializer`

**Files:**
- Modify: `Models/Entities/TaiKhoan.cs`
- Modify: `Data/DbInitializer.cs`

- [ ] **Step 1: Cập nhật enum `LoaiTaiKhoan` trong `TaiKhoan.cs`**
Thêm giá trị `XacDinhKetQuaKinhDoanh = 9`.

- [ ] **Step 2: Bổ sung seed TK 911 trong `DbInitializer.cs`**
Thêm TK 911 với đầy đủ thuộc tính vào danh mục tài khoản mặc định.

- [ ] **Step 3: Verify Compilation**
Chạy `dotnet build` kiểm tra 0 warnings.

- [ ] **Step 4: Commit**
`git commit -m "feat(entity): add Account 911 to LoaiTaiKhoan and seed data"`

---

### Task 3: Nâng cấp `PeriodClosingService` chạy qua TK 911 (TDD)

**Files:**
- Modify: `ninjaTax.Tests/PeriodClosingTt99Tests.cs`
- Modify: `Models/Services/PeriodClosingService.cs`

- [ ] **Step 1: Viết failing test trong `PeriodClosingTt99Tests.cs`**
Kiểm thử kết chuyển qua TK 911:
- Doanh thu kết chuyển vào Có 911.
- Chi phí kết chuyển vào Nợ 911.
- Chênh lệch lãi chuyển Nợ 911 / Có 4212.
- Assert: TK 911 có phát sinh và số dư cuối kỳ = 0.

- [ ] **Step 2: Chạy test để xác nhận FAIL**
`dotnet test --filter FullyQualifiedName~PeriodClosingTt99Tests`

- [ ] **Step 3: Cập nhật `PeriodClosingService.cs`**
Viết thuật toán 3 chặng kết chuyển chuẩn qua TK 911.

- [ ] **Step 4: Chạy test để xác nhận PASS**
`dotnet test --filter FullyQualifiedName~PeriodClosingTt99Tests`

- [ ] **Step 5: Commit**
`git commit -m "feat(closing): implement 3-stage period closing engine through Account 911"`

---

### Task 4: Hoàn thiện Bóc Tách Lưỡng Tính trên Trial Balance 8 Cột (TDD)

**Files:**
- Modify: `ninjaTax.Tests/GeneralLedgerReportTests.cs`
- Modify: `Models/Services/GeneralLedgerService.cs`

- [ ] **Step 1: Viết failing test trong `GeneralLedgerReportTests.cs`**
Tạo 2 khách hàng: KH A có dư Nợ 10M, KH B có dư Có 5M trên TK 131. Assert Bảng Cân đối 8 Cột thể hiện đồng thời Dư Nợ 10M và Dư Có 5M (không bị bù trừ về 5M).

- [ ] **Step 2: Chạy test để xác nhận FAIL**
`dotnet test --filter FullyQualifiedName~GeneralLedgerReportTests`

- [ ] **Step 3: Cập nhật `GeneralLedgerService.cs`**
Bóc tách số dư Nợ và Có chi tiết theo đối tượng cho các tài khoản lưỡng tính.

- [ ] **Step 4: Chạy test để xác nhận PASS**
`dotnet test --filter FullyQualifiedName~GeneralLedgerReportTests`

- [ ] **Step 5: Commit**
`git commit -m "feat(gl): implement non-offsetting two-way balance calculation for two-way accounts on trial balance"`

---

### Task 5: Cập nhật các Test Cũ & Chạy Kiểm Thử Toàn Diện

**Files:**
- Modify: các bài test từng assert cấm 911 (`MultiDatabaseTests.cs`, `DepartmentPayrollPostingTests.cs`, v.v. nếu có).
- Execute: `dotnet test` (Toàn bộ test suite phải Green 100%).
- Execute: `dotnet build` (0 warnings).

- [ ] **Step 1: Cập nhật các test case liên quan**
Điều chỉnh các assertion cũ để đồng bộ với cơ chế mới của TK 911.

- [ ] **Step 2: Chạy toàn bộ test suite**
`dotnet test`

- [ ] **Step 3: Commit & Hoàn tất**
`git commit -m "test: align all existing test cases with standardized Account 911 and verify green suite"`
