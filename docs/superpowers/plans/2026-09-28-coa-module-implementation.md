# Chart of Accounts (COA) Module Implementation Plan (Phase 11)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Xây dựng hoàn chỉnh module Quản trị Hệ thống Tài khoản kế toán (COA Management) chuẩn Thông tư 99/2025/TT-BTC, cho phép thêm, sửa, xem cây tài khoản đa cấp, ngăn chặn xóa tài khoản đã có phát sinh giao dịch, và tự động quản lý cờ tài khoản tổng hợp (`LaTaiKhoanSoCai`).

**Architecture:** Bổ sung Service layer `ITaiKhoanService` / `TaiKhoanService` để cô lập logic nghiệp vụ; xây dựng `TaiKhoanController` mỏng; render giao diện cây tài khoản với Tree-table / AG Grid trên ASP.NET Core MVC.

**Tech Stack:** ASP.NET Core MVC .NET 10, Entity Framework Core 10 (MariaDB/SQLite), xUnit, Bootstrap 5 & AG Grid Community.

**Spec:** `docs/PHASE11_CHART_OF_ACCOUNTS_MODULE_BRD_SPEC.md`

## Global Constraints
- **Circular 99/2025/TT-BTC compliance**: Tài khoản 911 được chuẩn hóa làm tài khoản trung gian kết chuyển cuối kỳ (không có số dư cuối kỳ).
- **Double-Entry & Posting Boundary**: Bút toán thông thường chỉ được phép hạch toán vào tài khoản chi tiết (lá), không được hạch toán vào tài khoản tổng hợp (`LaTaiKhoanSoCai == true`).
- **Data Protection**: Nghiêm cấm xóa hoặc đổi mã tài khoản khi đã có phát sinh dòng bút toán trong `ChiTietButToans`.
- **Zero Warnings**: Toàn bộ code C# phải biên dịch với 0 warning (`TreatWarningsAsErrors = true`).

## Review Focus
1. Ngăn chặn người dùng tạo tài khoản con có mã không bắt đầu bằng mã tài khoản mẹ (Prefix mismatch).
2. Khi thêm tài khoản con cấp 2 hoặc cấp 3, tài khoản mẹ phải tự động được cập nhật `LaTaiKhoanSoCai = true`.
3. Khi xóa tài khoản chưa phát sinh bút toán nhưng là tài khoản con cuối cùng của một tài khoản mẹ, tài khoản mẹ phải được hoàn trả `LaTaiKhoanSoCai = false` nếu cho phép hạch toán.
4. Cố tình gửi request xóa tài khoản đã có bút toán phải trả về lỗi rõ ràng và không được xóa dữ liệu khỏi database.
5. Danh mục tài khoản hiển thị dạng cây phân cấp trực quan theo đúng cấu trúc cha - con.

---

### Task 1: ViewModels & Domain Service Contract for COA

**Files:**
- Create: `Models/ViewModels/TaiKhoanViewModels.cs`
- Create: `Models/Services/ITaiKhoanService.cs`

**Interfaces:**
- Produces: `TaiKhoanViewModel`, `TaiKhoanCreateEditViewModel`, `ITaiKhoanService`

- [ ] **Step 1: Create `Models/ViewModels/TaiKhoanViewModels.cs`**
  Định nghĩa các ViewModel:
  - `TaiKhoanViewModel`: Hiển thị thông tin phân cấp (Id, MaTaiKhoan, TenTaiKhoan, BacTaiKhoan, TenTaiKhoanMe, TinhChat, LoaiTaiKhoan, LaTaiKhoanSoCai, DangHoatDong, SoLuongCon, DaPhatSinhGiaoDich).
  - `TaiKhoanCreateEditViewModel`: Dùng cho Form thêm mới / chỉnh sửa tài khoản.

- [ ] **Step 2: Create `Models/Services/ITaiKhoanService.cs`**
  Định nghĩa interface:
  ```csharp
  public interface ITaiKhoanService
  {
      Task<List<TaiKhoanViewModel>> LayDanhSachAsync(string? timKiem = null, LoaiTaiKhoan? loaiTaiKhoan = null);
      Task<TaiKhoan?> LayTheoIdAsync(long id);
      Task<TaiKhoan?> LayTheoMaAsync(string maTaiKhoan);
      Task<(bool ThanhCong, string? ThongBao, long? TaiKhoanId)> TaoMoiAsync(TaiKhoanCreateEditViewModel model);
      Task<(bool ThanhCong, string? ThongBao)> CapNhatAsync(long id, TaiKhoanCreateEditViewModel model);
      Task<(bool ThanhCong, string? ThongBao)> XoaAsync(long id);
      Task<List<TaiKhoan>> LayDanhSachTaiKhoanMeAsync();
  }
  ```

- [ ] **Step 3: Verify build**
  Run: `dotnet build`
  Expected: Build succeeded with 0 warnings.

---

### Task 2: Implement TDD Tests for TaiKhoanService (RED)

**Files:**
- Create: `ninjaTax.Tests/TaiKhoanServiceTests.cs`

**Interfaces:**
- Consumes: `ITaiKhoanService`, `TaiKhoanCreateEditViewModel`

- [ ] **Step 1: Write failing unit tests in `ninjaTax.Tests/TaiKhoanServiceTests.cs`**
  Test cases bao gồm:
  1. `TaoMoiTaiKhoanCon_ValidPrefix_UpdatesParentToLaTaiKhoanSoCai`: Tạo 1113 thuộc 111 -> 111 trở thành `LaTaiKhoanSoCai = true`.
  2. `TaoMoiTaiKhoanCon_InvalidPrefix_ThrowsOrReturnsError`: Tạo 1123 với mẹ là 111 -> Báo lỗi tiền tố không khớp.
  3. `XoaTaiKhoan_KhiDaPhatSinhButToan_ReturnsFailure`: Tạo tài khoản, sinh bút toán ghi sổ, thử xóa -> Trả về `ThanhCong = false`.
  4. `CapNhatTaiKhoan_DoiMaKhiDaPhatSinh_ReturnsFailure`: Đã có bút toán nhưng cố tình đổi mã -> Trả về `ThanhCong = false`.

- [ ] **Step 2: Run test to verify failure**
  Run: `dotnet test --filter FullyQualifiedName~TaiKhoanServiceTests`
  Expected: FAIL (Service implementation chưa tồn tại).

---

### Task 3: Implement `TaiKhoanService` (GREEN)

**Files:**
- Create: `Models/Services/TaiKhoanService.cs`
- Modify: `Program.cs` (Register DI: `builder.Services.AddScoped<ITaiKhoanService, TaiKhoanService>();`)

**Interfaces:**
- Consumes: `AppDbContext`, `ITaiKhoanService`
- Produces: `TaiKhoanService` implementation

- [ ] **Step 1: Implement `Models/Services/TaiKhoanService.cs`**
  Hiện thực đầy đủ các phương thức trong `ITaiKhoanService`:
  - Validate tiền tố mã tài khoản: `model.MaTaiKhoan.StartsWith(parent.MaTaiKhoan)`.
  - Validate trùng mã.
  - Tự động gán `BacTaiKhoan = parent.BacTaiKhoan + 1` và `LoaiTaiKhoan = parent.LoaiTaiKhoan`.
  - Cập nhật tài khoản mẹ `LaTaiKhoanSoCai = true`.
  - Kiểm tra `_context.ChiTietButToans.AnyAsync(c => c.TaiKhoanNoId == id || c.TaiKhoanCoId == id)` trước khi xóa hoặc sửa mã.

- [ ] **Step 2: Register service in `Program.cs`**

- [ ] **Step 3: Run tests and verify PASS**
  Run: `dotnet test --filter FullyQualifiedName~TaiKhoanServiceTests`
  Expected: PASS 100%.

---

### Task 4: Controller & Web UI for Chart of Accounts

**Files:**
- Create: `Controllers/TaiKhoanController.cs`
- Create: `Views/TaiKhoan/Index.cshtml`
- Create: `Views/TaiKhoan/Create.cshtml`
- Create: `Views/TaiKhoan/Edit.cshtml`
- Modify: `wwwroot/data/menu.json` (Thêm menu Hệ thống tài khoản vào nhóm "Hệ Thống")

**Interfaces:**
- Consumes: `ITaiKhoanService`

- [ ] **Step 1: Implement `TaiKhoanController.cs`**
  Actions: `Index`, `Create` (GET/POST), `Edit` (GET/POST), `Delete` (POST), `GetTreeJson` (GET).

- [ ] **Step 2: Create Views**
  - `Index.cshtml`: Bảng danh mục tài khoản dạng cây (hierarchical tree), có bộ lọc nhóm tài khoản (Loại 1 -> Loại 9), tìm kiếm realtime, huy hiệu trạng thái `LaTaiKhoanSoCai` và phím tắt F2 thêm mới.
  - `Create.cshtml` & `Edit.cshtml`: Form nhập liệu có validation Client-side & Server-side, dropdown chọn tài khoản mẹ.

- [ ] **Step 3: Update `menu.json`**
  Thêm item `Hệ thống tài khoản (COA)` dưới nhóm menu `Hệ Thống`.

- [ ] **Step 4: Run build & full regression tests**
  Run: `dotnet test`
  Expected: 148+ tests passed, 0 warnings.
