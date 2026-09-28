# SPECIFICATION & ARCHITECTURAL BLUEPRINT (PHASE 12)
## PHÂN HỆ DANH MỤC LÕI & CẬN LÕI KẾ TOÁN (MASTER DATA SUITE - TT99/2025/TT-BTC)

- **Dự án**: `ninjaTax`
- **Phiên bản**: Phase 12 (Core to Edge Master Data Implementation)
- **Tác giả**: Principal Software Engineer & Lead BA (20+ năm kinh nghiệm ERP/Fintech & Kế toán Việt Nam)
- **Ngày ban hành**: 28/09/2026
- **Trạng thái**: Approved for Implementation

---

## 1. MỤC TIÊU VÀ BỐI CẢNH (GOALS & CONTEXT)

Trong hệ thống kế toán doanh nghiệp theo Thông tư 99/2025/TT-BTC và chuẩn mực VAS, Master Data là nền tảng cốt lõi định hình toàn bộ tính toàn vẹn của sổ sách tài chính:
1. **Đối tượng (DoiTuong)**: Quyết định việc hạch toán công nợ chi tiết (TK 131, 331, 138, 338, 141) và nguyên tắc lập Bảng Cân Đối Tài Khoản 8 cột hai chiều không bù trừ.
2. **Vật tư Hàng hóa (VatTuHangHoa)**: Định hình chính xác tài khoản kho (152, 155, 1561), thuế suất GTGT và giá vốn (TK 632) theo VAS 02.
3. **Kho Hàng (Kho)**: Căn cứ quản lý thẻ kho, phân quyền thủ kho, và đối soát S10-DN theo địa điểm lưu trữ.
4. **Tài khoản Ngân hàng (TaiKhoanNganHang)**: Căn cứ chi tiết sổ tiền gửi ngân hàng (Mẫu S02b-DN / TK 1121).

Phân hệ Phase 12 thực hiện đóng gói hoàn chỉnh từ Core ra Edge theo 4 Slice độc lập, tăng dần từ đơn giản đến phức tạp, tuân thủ nghiêm ngặt phương pháp luận TDD (Test-Driven Development).

---

## 2. KIẾN TRÚC PHÂN CHIA LÁT CẮT (SLICES ROADMAP: CORE TO EDGE)

### Slice 1 (The Foundational Core): Quản lý Đối tượng Công nợ (`DoiTuong`)
- **Domain Service**: `IDoiTuongService`, `DoiTuongService`
- **Controller**: `DoiTuongController` (Thin Controller)
- **ViewModels**: `DoiTuongIndexViewModel`, `DoiTuongCreateEditViewModel`, `DoiTuongDetailsViewModel`
- **Views**: `Index.cshtml` (AG Grid / DataTable tìm kiếm, lọc Khách hàng/NCC/Khác), `Create.cshtml`, `Edit.cshtml`, `Details.cshtml` (kèm thống kê công nợ tức thời)
- **Chốt chặn Bảo vệ Dữ liệu (Invariants)**:
  - Mã đối tượng là duy nhất, không khoảng trắng thừa.
  - Chuẩn hóa định dạng Mã số thuế (MST 10 số hoặc 13 số có gạch ngang `XXXXXXXXXX-XXX`).
  - Cấm xóa đối tượng khi đã phát sinh trong `ChiTietButToans`, `HoaDonBanHangs`, hoặc `HoaDonMuaHangs`.
  - Hỗ trợ đổi trạng thái hoạt động (`DangHoatDong = false`) để khóa giao dịch an toàn mà không phá vỡ liên kết dữ liệu lịch sử.

### Slice 2 (Core Inventory Master): Hoàn thiện Vật tư Hàng hóa (`VatTuHangHoa`)
- **Domain Service**: `IVatTuHangHoaService`, `VatTuHangHoaService`
- **Refactor Controller**: `VatTuHangHoaController` (chuyển sang gọi Service thay vì inject trực tiếp DbContext)
- **ViewModels**: Bổ sung `VatTuHangHoaEditViewModel`, `VatTuHangHoaDetailsViewModel`
- **Views**: Bổ sung `Edit.cshtml`, `Details.cshtml`; cập nhật `Index.cshtml` có nút Sửa/Khóa; thêm Navigation menu vào `wwwroot/data/menu.json`.
- **Chốt chặn Bảo vệ Dữ liệu (Invariants)**:
  - Cấm sửa `MaVatTu` và cấm xóa mặt hàng khi đã có phát sinh trong `ChiTietNhapKho`, `ChiTietXuatKho`, `ChiTietHoaDonBan`, hoặc `ChiTietHoaDonMua`.
  - Kiểm tra hợp lệ tài khoản ngầm định: TK Kho (152, 153, 155, 156), TK Doanh thu (511), TK Giá vốn (632).

### Slice 3 (Core Warehouse Master): Quản lý Danh mục Kho (`Kho`)
- **Domain Service**: Mở rộng `IInventoryService` các hàm quản lý Kho: `GetAllWarehousesAsync`, `GetWarehouseByIdAsync`, `CreateWarehouseAsync`, `UpdateWarehouseAsync`, `DeleteWarehouseAsync`.
- **Controller**: `KhoController`
- **ViewModels**: `KhoIndexViewModel`, `KhoCreateEditViewModel`
- **Views**: `Views/Kho/Index.cshtml`, `Views/Kho/Create.cshtml`, `Views/Kho/Edit.cshtml`
- **Chốt chặn Bảo vệ Dữ liệu (Invariants)**:
  - `MaKho` là duy nhất trong toàn hệ thống.
  - Cấm xóa Kho khi đã có phiếu nhập/xuất kho (`PhieuNhapKhos`, `PhieuXuatKhos`) hoặc tồn kho hiện hữu > 0.
  - Gắn liên kết thủ kho (`ThuKhoId` &rarr; `NhanVien`) và tài khoản kho ngầm định (`TaiKhoanKhoMacDinhId` &rarr; `TaiKhoan`).

### Slice 4 (Inner Edge - Banking Master): Quản lý Tài khoản Ngân hàng (`TaiKhoanNganHang`)
- **Domain Service**: `ITaiKhoanNganHangService`, `TaiKhoanNganHangService`
- **Controller**: `TaiKhoanNganHangController`
- **ViewModels**: `TaiKhoanNganHangIndexViewModel`, `TaiKhoanNganHangCreateEditViewModel`
- **Views**: `Views/TaiKhoanNganHang/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`
- **Chốt chặn Bảo vệ Dữ liệu (Invariants)**:
  - Số tài khoản ngân hàng duy nhất theo từng Ngân hàng.
  - Liên kết với tiểu khoản tiền gửi ngân hàng (TK 1121x).
  - Cấm xóa tài khoản ngân hàng khi đã phát sinh giao dịch trong Thu tiền (Báo có) hoặc Chi tiền (Ủy nhiệm chi).

---

## 3. CHỈ TIÊU CHẤP THUẬN KỸ THUẬT (ACCEPTANCE CRITERIA)
1. **Kiến trúc Clean & Thin Controller**: Controllers không truy vấn database trực tiếp; toàn bộ business logic và validation nằm trong Service Layer.
2. **Tuân thủ TT99/2025/TT-BTC & VAS**: Không xâm phạm bất biến kế toán; bảo toàn liên kết đối tượng hai chiều.
3. **TDD Coverage**: Mỗi slice có bộ unit test độc lập trong `ninjaTax.Tests/` bao phủ mọi kịch bản thành công và ngoại lệ vi phạm chốt chặn.
4. **Zero Warnings Policy**: `dotnet build /p:TreatWarningsAsErrors=true` đạt 0 warning, 0 error.
5. **Git Pre-commit Hook & CI**: 100% test pass (148/148 + new tests).
