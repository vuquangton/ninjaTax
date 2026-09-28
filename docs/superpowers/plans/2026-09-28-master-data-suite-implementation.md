# Master Data Suite Implementation Plan (Phase 12: Core to Edge)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Triển khai hoàn chỉnh toàn bộ các phân hệ Master Data còn thiếu hoặc chưa đủ chức năng theo lộ trình từ Lõi ra Rìa (Core to Edge): DoiTuong (Đối tượng), VatTuHangHoa (Vật tư hàng hóa), Kho (Danh mục Kho), TaiKhoanNganHang (Tài khoản ngân hàng), tuân thủ TT99/2025/TT-BTC, kiến trúc Thin Controller, TDD và Zero Warnings.

**Architecture:** Áp dụng Service Layer cô lập nghiệp vụ (`IDoiTuongService`, `IVatTuHangHoaService`, mở rộng `IInventoryService`, `ITaiKhoanNganHangService`); Controllers mỏng; ViewModels phân tách rõ ràng; Giao diện Razor Views Bootstrap 5 + AG Grid; Kiểm tra chốt chặn toàn vẹn kế toán trước khi xóa/sửa; Tích hợp đầy đủ Navigation Menu.

**Tech Stack:** ASP.NET Core MVC .NET 10, Entity Framework Core 10 (MariaDB/SQLite), xUnit, Bootstrap 5, AG Grid Community.

**Spec:** `docs/PHASE12_MASTER_DATA_SUITE_BRD_SPEC.md`

## Global Constraints
- **Strict Invariants**: Double-entry `TongNo == TongCo`, cấm sử dụng TK 911 cho các chứng từ nghiệp vụ, precision `decimal(19, 4)`.
- **Data Protection**: Nghiêm cấm xóa hoặc đổi mã định danh của Đối tượng, Mặt hàng, Kho, Tài khoản ngân hàng khi đã phát sinh chứng từ phát sinh kế toán.
- **Zero Warnings Policy**: Toàn bộ dự án phải biên dịch 0 warning (`TreatWarningsAsErrors = true`).
- **Quality Gates**: Mọi thay đổi phải vượt qua `dotnet build` và `dotnet test` (100% pass rate).

## Review Focus
1. `DoiTuong`: Cấm xóa khách hàng/nhà cung cấp đã có giao dịch phát sinh trong `ChiTietButToan` hoặc hóa đơn; cho phép chuyển sang `DangHoatDong = false`.
2. `VatTuHangHoa`: Cập nhật được giá bán, thuế suất, tài khoản ngầm định; cấm sửa `MaVatTu` khi đã phát sinh nhập/xuất kho.
3. `Kho`: Thêm/sửa kho hoạt động chuẩn; cấm xóa kho khi đã phát sinh phiếu kho.
4. `TaiKhoanNganHang`: Quản lý tài khoản ngân hàng liên kết TK 1121x; cấm xóa khi đã phát sinh chứng từ thu/chi tiền ngân hàng.
5. `menu.json`: Tất cả các phân hệ Master Data được hiển thị trực quan trong thanh điều hướng hệ thống.

---

### Task 1: ViewModels & Domain Service for `DoiTuong` (Core Master)

**Files:**
- Create: `Models/ViewModels/DoiTuongViewModels.cs`
- Create: `Models/Services/IDoiTuongService.cs`
- Create: `Models/Services/DoiTuongService.cs`
- Test: `ninjaTax.Tests/DoiTuongServiceTests.cs`

**Interfaces:**
- Produces: `DoiTuongItemViewModel`, `DoiTuongCreateEditViewModel`, `DoiTuongDetailsViewModel`, `IDoiTuongService`

- [ ] **Step 1: Write failing tests for `DoiTuongService`**
  Tạo `ninjaTax.Tests/DoiTuongServiceTests.cs` kiểm thử:
  - Tạo mới đối tượng hợp lệ (sinh ID, mã viết hoa không khoảng trắng).
  - Trùng mã đối tượng báo lỗi thất bại.
  - Xóa đối tượng chưa phát sinh giao dịch thành công.
  - Xóa đối tượng ĐÃ phát sinh giao dịch trong bút toán/hóa đơn bị từ chối và bảo toàn dữ liệu.
  - Cập nhật thông tin đối tượng thành công.

- [ ] **Step 2: Run test to verify it fails**
  Run: `dotnet test --filter "FullyQualifiedName~DoiTuongServiceTests"`
  Expected: FAIL (types not found / not implemented).

- [ ] **Step 3: Implement `DoiTuongViewModels`, `IDoiTuongService`, and `DoiTuongService`**
  - Tạo `Models/ViewModels/DoiTuongViewModels.cs`.
  - Tạo `Models/Services/IDoiTuongService.cs`.
  - Triển khai `Models/Services/DoiTuongService.cs` với đầy đủ logic kiểm tra phát sinh dữ liệu trong `ChiTietButToans`, `HoaDonBanHangs`, `HoaDonMuaHangs`.
  - Đăng ký DI trong `Program.cs`.

- [ ] **Step 4: Run test to verify it passes**
  Run: `dotnet test --filter "FullyQualifiedName~DoiTuongServiceTests"`
  Expected: PASS.

- [ ] **Step 5: Verify build with zero warnings**
  Run: `dotnet build /p:TreatWarningsAsErrors=true`
  Expected: Build succeeded with 0 warnings.

---

### Task 2: Controller, Razor Views & Menu for `DoiTuong`

**Files:**
- Create: `Controllers/DoiTuongController.cs`
- Create: `Views/DoiTuong/Index.cshtml`
- Create: `Views/DoiTuong/Create.cshtml`
- Create: `Views/DoiTuong/Edit.cshtml`
- Create: `Views/DoiTuong/Details.cshtml`
- Modify: `wwwroot/data/menu.json`

**Interfaces:**
- Consumes: `IDoiTuongService`
- Produces: Web UI endpoints `/DoiTuong`, `/DoiTuong/Create`, `/DoiTuong/Edit/{id}`, `/DoiTuong/Details/{id}`, `/DoiTuong/Delete/{id}`

- [ ] **Step 1: Implement `DoiTuongController`**
  Kế thừa `Controller`, tiêm `IDoiTuongService`, triển khai các action Index (lọc theo từ khóa, loại đối tượng), Create (GET/POST), Edit (GET/POST), Details (GET), Delete (POST).

- [ ] **Step 2: Create Razor Views for `DoiTuong`**
  - `Index.cshtml`: Bảng danh sách đối tượng, bộ lọc phân loại (Tất cả, Khách hàng, Nhà cung cấp, Khác), nút Thêm mới, Sửa, Chi tiết.
  - `Create.cshtml` & `Edit.cshtml`: Form nhập liệu chuẩn thuế (Mã, Tên, Loại, MST, Địa chỉ, SĐT, Email, Người liên hệ, Số tài khoản NH, Tên NH).
  - `Details.cshtml`: Thẻ tóm tắt thông tin đối tượng và số dư công nợ.

- [ ] **Step 3: Update `wwwroot/data/menu.json`**
  Bổ sung mục "Khách hàng & Nhà cung cấp" vào menu "Hệ Thống" hoặc menu độc lập.

- [ ] **Step 4: Verify build and all tests**
  Run: `dotnet build /p:TreatWarningsAsErrors=true` và `dotnet test`
  Expected: 0 warnings, 100% tests pass.

---

### Task 3: Complete `VatTuHangHoa` (Service Layer, Edit, Details, Navigation)

**Files:**
- Create: `Models/Services/IVatTuHangHoaService.cs`
- Create: `Models/Services/VatTuHangHoaService.cs`
- Modify: `Models/ViewModels/VatTuHangHoaViewModels.cs`
- Modify: `Controllers/VatTuHangHoaController.cs`
- Create: `Views/VatTuHangHoa/Edit.cshtml`
- Create: `Views/VatTuHangHoa/Details.cshtml`
- Modify: `Views/VatTuHangHoa/Index.cshtml`
- Modify: `wwwroot/data/menu.json`
- Test: `ninjaTax.Tests/VatTuHangHoaServiceTests.cs`

**Interfaces:**
- Produces: `IVatTuHangHoaService`, `VatTuHangHoaService`, endpoints `/VatTuHangHoa/Edit/{id}`, `/VatTuHangHoa/Details/{id}`

- [ ] **Step 1: Write failing tests for `VatTuHangHoaService`**
  Tạo `ninjaTax.Tests/VatTuHangHoaServiceTests.cs` kiểm thử:
  - Tạo mới mặt hàng thành công.
  - Trùng mã mặt hàng báo lỗi.
  - Sửa thông tin giá và tài khoản thành công.
  - Sửa mã mặt hàng khi đã có giao dịch phát sinh bị chặn.
  - Xóa mặt hàng đã có phát sinh nhập/xuất kho bị chặn.

- [ ] **Step 2: Implement Service Layer for `VatTuHangHoa`**
  - Triển khai `IVatTuHangHoaService` & `VatTuHangHoaService`.
  - Đăng ký DI trong `Program.cs`.

- [ ] **Step 3: Refactor `VatTuHangHoaController` & add Edit/Details Views**
  - Refactor controller sử dụng `IVatTuHangHoaService`.
  - Tạo `Views/VatTuHangHoa/Edit.cshtml` và `Details.cshtml`.
  - Bổ sung thao tác Sửa / Xem chi tiết vào `Index.cshtml`.
  - Bổ sung link Danh mục Vật tư hàng hóa vào `wwwroot/data/menu.json` trong mục "Kho & Hàng Hóa".

- [ ] **Step 4: Run tests and verify build**
  Run: `dotnet build /p:TreatWarningsAsErrors=true` và `dotnet test`
  Expected: 0 warnings, all tests pass.

---

### Task 4: Warehouse Master (`Kho`) CRUD & Management

**Files:**
- Modify: `Models/Services/IInventoryService.cs`
- Modify: `Models/Services/InventoryService.cs`
- Create: `Models/ViewModels/KhoViewModels.cs`
- Create: `Controllers/KhoController.cs`
- Create: `Views/Kho/Index.cshtml`
- Create: `Views/Kho/Create.cshtml`
- Create: `Views/Kho/Edit.cshtml`
- Modify: `wwwroot/data/menu.json`
- Test: `ninjaTax.Tests/KhoServiceTests.cs`

**Interfaces:**
- Produces: `IKhoService` (hoặc methods mở rộng trong `IInventoryService`), `KhoController`, `/Kho` endpoints.

- [ ] **Step 1: Write failing tests for Warehouse Management**
  Tạo `ninjaTax.Tests/KhoServiceTests.cs` kiểm thử:
  - Thêm mới kho hợp lệ.
  - Trùng mã kho báo lỗi.
  - Cập nhật thông tin kho thành công.
  - Cấm xóa kho khi đã có phiếu nhập/xuất kho.

- [ ] **Step 2: Implement Warehouse service methods & ViewModels**
  - Cập nhật `IInventoryService` và `InventoryService`.
  - Tạo `Models/ViewModels/KhoViewModels.cs`.

- [ ] **Step 3: Implement `KhoController` & Views**
  - Tạo `Controllers/KhoController.cs`.
  - Tạo `Views/Kho/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`.
  - Bổ sung mục "Danh mục Kho hàng" vào `wwwroot/data/menu.json`.

- [ ] **Step 4: Run tests and verify build**
  Run: `dotnet build /p:TreatWarningsAsErrors=true` và `dotnet test`
  Expected: 0 warnings, all tests pass.

---

### Task 5: Bank Account Master (`TaiKhoanNganHang`) Management

**Files:**
- Create: `Models/ViewModels/TaiKhoanNganHangViewModels.cs`
- Create: `Models/Services/ITaiKhoanNganHangService.cs`
- Create: `Models/Services/TaiKhoanNganHangService.cs`
- Create: `Controllers/TaiKhoanNganHangController.cs`
- Create: `Views/TaiKhoanNganHang/Index.cshtml`
- Create: `Views/TaiKhoanNganHang/Create.cshtml`
- Create: `Views/TaiKhoanNganHang/Edit.cshtml`
- Modify: `wwwroot/data/menu.json`
- Test: `ninjaTax.Tests/TaiKhoanNganHangServiceTests.cs`

- [ ] **Step 1: Write failing tests for `TaiKhoanNganHangService`**
  Tạo `ninjaTax.Tests/TaiKhoanNganHangServiceTests.cs` kiểm thử:
  - Thêm mới tài khoản ngân hàng hợp lệ.
  - Trùng số tài khoản tại cùng ngân hàng báo lỗi.
  - Cấm xóa tài khoản ngân hàng khi đã phát sinh chứng từ thu/chi tiền ngân hàng.

- [ ] **Step 2: Implement Service, Controller & Views for `TaiKhoanNganHang`**
  - Triển khai Service, Controller, Views và đăng ký DI.
  - Bổ sung menu "Tài khoản ngân hàng" vào `menu.json` dưới mục "Hệ Thống" hoặc "Thu Tiền/Chi Tiền".

- [ ] **Step 3: Run all tests and verify build**
  Run: `dotnet build /p:TreatWarningsAsErrors=true` và `dotnet test`
  Expected: 0 warnings, 100% tests green.

---

### Task 6: Full Verification, Code Review & Git Sync

**Files:**
- Code Review against repo standards & TT99 accounting invariants.
- Sync CodeGraph: `codegraph sync`.
- Git commit & push.

- [ ] **Step 1: Execute complete test suite**
  Run: `dotnet test`
  Expected: All existing tests (148) + all new tests pass.

- [ ] **Step 2: Run Code Review (Standards & Spec subagents)**
  Verify clean architecture, zero warnings, no fowler smells.

- [ ] **Step 3: Sync CodeGraph and push branch**
  Run: `codegraph sync`, commit through pre-commit hook, push branch to remote.
