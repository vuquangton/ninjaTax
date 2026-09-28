# BÁO CÁO NGHIỆM THU VẬN HÀNH & ĐẶC TẢ NGHIỆP VỤ HỆ THỐNG TÀI KHOẢN (COA)
## CHUẨN HÓA THEO THÔNG TƯ 99/2025/TT-BTC BỘ TÀI CHÍNH (PHASE 11)

- **Vai trò thẩm định**: Giám đốc Nghiệp vụ Phân tích (BA Lead - 20+ năm kinh nghiệm ERP/Fintech) & Kế toán trưởng Doanh nghiệp (20+ năm thực chiến VAS/TT99/Thanh tra Thuế).
- **Cơ sở pháp lý**:
  - Thông tư số **99/2025/TT-BTC** (Bộ Tài chính ban hành, có hiệu lực từ 01/01/2026).
  - Luật Kế toán số **88/2015/QH13**.
  - Nghị định **41/2018/NĐ-CP** (Xử phạt vi phạm hành chính lĩnh vực kế toán, hóa đơn).
  - Chuẩn mực Kế toán Việt Nam (VAS 01, VAS 02, VAS 14, VAS 16) và đối chiếu IFRS / IAS 1.
  - Khảo sát thực tế hệ sinh thái ERP/Kế toán hàng đầu Việt Nam: MISA AMIS/SME 2026, FAST Financial/Business Online 2026, BRAVO 8R3 ERP.

---

## 🛑 PHẦN 1: ĐÁNH GIÁ SẴN SÀNG VẬN HÀNH MÔI TRƯỜNG PRODUCTION (PROD ENV READINESS)

### Câu hỏi: "Is it can operate in PROD ENV?"
### Trả lời trực diện: **CHƯA THỂ VẬN HÀNH AN TOÀN TRÊN PROD NẾU THIẾU MODULE COA RIÊNG BIỆT (CURRENT STATUS: CONDITIONAL NO).**

### Các lỗ hổng sống còn (Critical Production Blockers):
1. **Thiếu Giao diện & Nghiệp vụ Quản trị Hệ thống Tài khoản (COA Management UI/API)**:
   - Hiện tại, danh mục tài khoản `TaiKhoan` chỉ được nạp cố định (hardcoded seed data) qua `DbInitializer.cs`.
   - Khi doanh nghiệp phát sinh nghiệp vụ thực tế (mở thêm tài khoản chi tiết ngân hàng 11211, 11212...; mở thêm chi tiết chi phí bán hàng 64211, 64212; mở thêm kho hàng chi tiết 15611, 15612), kế toán trưởng **hoàn toàn không có giao diện Web để thêm, sửa, phân cấp cha-con, hoặc kích hoạt/ngừng dùng tài khoản**.
2. **Thiếu Bất Biến Kiểm Soát Xóa/Sửa khi Tài Khoản Đã Phát Sinh Ghi Sổ (Data Integrity & Audit Shield)**:
   - Nếu người dùng sửa đổi hoặc xóa một mã tài khoản đã có bút toán phát sinh trong bảng `ChiTietButToan`, toàn bộ Sổ Cái, Bảng Cân Đối Tài Khoản và Báo Cáo Tài Chính sẽ bị gãy toàn vẹn dữ liệu (Orphaned Foreign Keys hoặc Balance Mismatch).
3. **Thiếu Chức năng Nhập Khẩu Số Dư Ban Đầu (Opening Balance Entry - S01-DN)**:
   - Khi bắt đầu triển khai phần mềm cho một doanh nghiệp mới trên PROD, bước đầu tiên bắt buộc của Kế toán trưởng là **nhập số dư đầu kỳ của tất cả tài khoản** (chi tiết theo Đối tượng, Ngân hàng, Kho). Hiện tại chưa có module nhập và kiểm soát cân đối số dư đầu kỳ (`TongDuNoDauKy == TongDuCoDauKy`).
4. **Quy định Cây Tài khoản Đa cấp (Parent-Child Hierarchy Rule)**:
   - Bất biến kế toán: **Chỉ được hạch toán vào tài khoản lá (chi tiết nhất, `TaiKhoanCons.Count == 0`)**. Khi một tài khoản cấp 1 (VD: 111) đã sinh ra tài khoản cấp 2 (1111, 1112), hệ thống phải tự động khóa hạch toán trực tiếp vào tài khoản 111, biến nó thành `LaTaiKhoanSoCai = true`.

---

## 🏛️ PHẦN 2: THIẾT KẾ KIẾN TRÚC NGHIỆP VỤ & SƠ ĐỒ DÒNG DỮ LIỆU (ASCII ART)

### 1. Kiến trúc Quản trị Cây Tài khoản (Hierarchical COA Architecture)

```
+==================================================================================================+
|                        KIẾN TRÚC CÂY DANH MỤC TÀI KHOẢN (HIERARCHICAL COA)                        |
+==================================================================================================+
|                                                                                                  |
|   LOẠI TÀI KHOẢN (LoaiTaiKhoan: 1 -> 9)                                                          |
|   |-- 1: Tài sản ngắn hạn & dài hạn       |-- 4: Vốn chủ sở hữu        |-- 8: Chi phí khác       |
|   |-- 2: Nợ phải trả                     |-- 5: Doanh thu             |-- 9: Xác định KQKD      |
|   |-- 3: Chi phí SXKD (hoặc Loại 6)       |-- 7: Thu nhập khác                                   |
|                                                                                                  |
|   CẤU TRÚC PHÂN CẤP & QUY TẮC GHI SỔ:                                                            |
|                                                                                                  |
|       [TK Cấp 1] (3 chữ số: VD 111) ----> LaTaiKhoanSoCai = TRUE (Không cho phép hạch toán)     |
|           |                                                                                      |
|           +--- [TK Cấp 2] (4 chữ số: VD 1111) ----> LaTaiKhoanSoCai = FALSE (Cho phép hạch toán) |
|           |                                                                                      |
|           +--- [TK Cấp 2] (4 chữ số: VD 1112) ----> LaTaiKhoanSoCai = FALSE (Cho phép hạch toán) |
|                   |                                                                              |
|                   +--- [TK Cấp 3] (5 chữ số: VD 11121) -> LaTaiKhoanSoCai = FALSE (Hạch toán)   |
|                                         (Khi này TK 1112 chuyển thành LaTaiKhoanSoCai = TRUE)    |
|                                                                                                  |
|   RÀNG BUỘC KIỂM SOÁT THAO TÁC (CONTROL INVARIANTS):                                            |
|   1. Mã tài khoản con BẮT BUỘC phải bắt đầu bằng mã tài khoản mẹ (Prefix Validation).           |
|   2. Nếu tài khoản đã có phát sinh bút toán trong kỳ -> NGHIÊM CẤM XÓA & KHÔNG ĐỔI MÃ.          |
|   3. Chỉ cho phép Ngừng hoạt động (DangHoatDong = false) khi số dư hiện tại bằng 0.              |
+==================================================================================================+
```

### 2. Sơ đồ Trạng thái Tài khoản (State Machine)

```
        +-------------------------------------------------------+
        |                                                       |
        v                                                       |
  [TẠO MỚI] ---> [HOẠT ĐỘNG (Active)] <---> [NGỪNG HOẠT ĐỘNG]  |
        |              |                           |            |
        |              | (Phát sinh bút toán)      | (Kích hoạt)|
        |              v                           +------------+
        |       [ĐÃ PHÁT SINH GHI SỔ]
        |              |
        |              | (Cấm xóa tuyệt đối)
        |              v
        +------> [XÓA VĨNH VIỄN] (Chỉ khi chưa từng có bút toán nào)
```

---

## 📋 PHẦN 3: ĐẶC TẢ CA SỬ DỤNG (USE CASES & DETAILED PATHS)

### Use Case 1: Thêm mới tài khoản con (Create Sub-Account)
- **Actor**: Kế toán tổng hợp / Kế toán trưởng.
- **Pre-conditions**: Người dùng đã đăng nhập, có quyền cấu hình hệ thống kế toán.
- **Main Flow (Happy Path)**:
  1. Người dùng vào menu `Hệ Thống` -> `Hệ Thống Tài Khoản (COA)`.
  2. Bấm nút `[F2] + Thêm tài khoản mới`.
  3. Chọn tài khoản mẹ (VD: `1121 - Tiền gửi ngân hàng`).
  4. Hệ thống tự động gợi ý bậc tài khoản (`BacTaiKhoan = 3`) và loại tài khoản (`LoaiTaiKhoan = TaiSan`).
  5. Nhập mã tài khoản: `11211`, tên: `Tiền gửi Ngân hàng Vietcombank - VNĐ`.
  6. Chọn tính chất: `Dư Nợ`.
  7. Bấm `Lưu`. Hệ thống kiểm tra:
     - Mã `11211` bắt đầu bằng `1121` (Hợp lệ).
     - Mã `11211` chưa tồn tại trong hệ sinh thái (Hợp lệ).
     - Cập nhật tài khoản `1121` thành `LaTaiKhoanSoCai = true` (khóa hạch toán trực tiếp vào 1121).
  8. Hệ thống lưu vào CSDL, hiển thị thông báo thành công và làm mới cây thư mục.
- **Alternative Paths**:
  - Thêm tài khoản Cấp 1 mới (chưa có tài khoản mẹ): Phải tuân thủ danh mục Thông tư 99.
- **Exception Paths**:
  - Mã tài khoản con không chứa tiền tố của tài khoản mẹ -> Báo lỗi: *"Mã tài khoản con phải bắt đầu bằng mã tài khoản mẹ [1121]"*.
  - Tài khoản mẹ đang ở trạng thái ngừng hoạt động -> Báo lỗi: *"Không thể tạo tài khoản con cho tài khoản mẹ đang bị khóa"*.

---

### Use Case 2: Sửa đổi và Khóa tài khoản (Update & Deactivate Account)
- **Actor**: Kế toán trưởng.
- **Main Flow (Happy Path)**:
  1. Chọn một tài khoản trên bảng danh mục (VD: `1112`).
  2. Bấm `Chỉnh sửa`. Sửa tên hiển thị hoặc ghi chú.
  3. Chuyển trạng thái `Đang hoạt động` sang `Ngừng sử dụng`.
  4. Hệ thống kiểm tra: Số dư cuối kỳ hiện tại = 0 và không có bút toán chưa ghi sổ.
  5. Cho phép lưu thành công.
- **Exception Paths**:
  - Người dùng cố tình sửa `MaTaiKhoan` khi đã có bút toán -> Hệ thống disable ô nhập mã hoặc báo lỗi: *"Tài khoản đã phát sinh giao dịch, nghiêm cấm thay đổi số hiệu tài khoản"*.
  - Người dùng xóa tài khoản đã có bút toán -> Báo lỗi: *"Tài khoản này đã có [N] bút toán phát sinh trong sổ sách. Không được phép xóa!"*.

---

### Use Case 3: Thiết lập Số Dư Đầu Kỳ (Initial Opening Balances)
- **Actor**: Kế toán trưởng.
- **Main Flow (Happy Path)**:
  1. Truy cập `Hệ Thống` -> `Nhập Số Dư Đầu Kỳ`.
  2. Chọn niên độ áp dụng (VD: 01/01/2026).
  3. Lưới dữ liệu (AG Grid) hiển thị toàn bộ các tài khoản chi tiết (lá).
  4. Người dùng nhập số Dư Nợ đầu kỳ hoặc Dư Có đầu kỳ theo từng tài khoản.
  5. Đối với tài khoản công nợ (131, 331), hệ thống mở pop-up chi tiết theo từng Đối tượng (Khách hàng/NCC).
  6. Thanh trạng thái tính tổng:
     $$\text{Tổng Dư Nợ Đầu Kỳ} = \text{Tổng Dư Có Đầu Kỳ}$$
  7. Khi cân đối 100%, người dùng bấm `Lưu & Khóa Số Dư Đầu Kỳ`.
- **Exception Paths**:
  - Tổng Dư Nợ $\neq$ Tổng Dư Có: Hệ thống tô đỏ chênh lệch và vô hiệu hóa nút `Khóa sổ dư`.

---

## 🎨 PHẦN 4: THIẾT KẾ GIAO DIỆN NGƯỜI DÙNG (UI/UX WIREFRAMES)

### Màn hình Danh mục Tài khoản (Tree Table / Grid View)

```
+--------------------------------------------------------------------------------------------------+
|  ninjaTax  |  Mua Hàng | Bán Hàng | Sổ Cái | ... | Hệ Thống [v]                     User: ketoantruong|
+--------------------------------------------------------------------------------------------------+
|                                                                                                  |
|   DANH MỤC HỆ THỐNG TÀI KHOẢN (CHẾ ĐỘ THÔNG TƯ 99/2025/TT-BTC)                                   |
|   [F2 + Thêm tài khoản]   [Xuất Excel]   [Nạp dữ liệu mẫu TT99]        [ Ô tìm kiếm mã/tên... ]  |
|                                                                                                  |
|   +------------------------------------------------------------------------------------------+   |
|   | Số hiệu TK | Tên tài khoản                  | Cấp | Tính chất    | Sổ cái? | Trạng thái  | T/t|   |
|   |------------+--------------------------------+-----+--------------+---------+-------------+----|   |
|   | > 111      | Tiền mặt                       |  1  | Dư Nợ        | [V]     | Hoạt động   |... |   |
|   |   |-- 1111 | Tiền Việt Nam                  |  2  | Dư Nợ        | [ ]     | Hoạt động   | sửa|   |
|   |   +-- 1112 | Ngoại tệ                       |  2  | Dư Nợ        | [ ]     | Hoạt động   | sửa|   |
|   | > 112      | Tiền gửi ngân hàng             |  1  | Dư Nợ        | [V]     | Hoạt động   |... |   |
|   |   +-- 1121 | Tiền Việt Nam gửi ngân hàng    |  2  | Dư Nợ        | [V]     | Hoạt động   | sửa|   |
|   |       +--11211| VCB - VNĐ Chi nhánh Hà Nội  |  3  | Dư Nợ        | [ ]     | Hoạt động   | sửa|   |
|   | > 131      | Phải thu của khách hàng        |  1  | Lưỡng tính   | [ ]     | Hoạt động   | sửa|   |
|   | > 911      | Xác định kết quả kinh doanh    |  1  | Không số dư  | [ ]     | Hoạt động   |... |   |
|   +------------------------------------------------------------------------------------------+   |
|                                                                                                  |
+--------------------------------------------------------------------------------------------------+
```

---

## 🗺️ PHẦN 5: KẾ HOẠCH TRIỂN KHAI CHI TIẾT (IMPLEMENTATION ROADMAP)

| Sprint | Hạng mục công việc (Scope) | Deliverables | Thời lượng |
|---|---|---|---|
| **Sprint 1** | **Domain Modeling & Backend Engine** | `ITaiKhoanService`, `TaiKhoanService`, DTOs/ViewModels, Validation Rules (Prefix check, Child-account check, In-use check) | 1.0 ngày |
| **Sprint 2** | **Unit & Integration Tests (TDD)** | `TaiKhoanServiceTests.cs` (Đảm bảo 100% logic phân cấp, cấm xóa khi đã có bút toán, tự động set `LaTaiKhoanSoCai`) | 0.5 ngày |
| **Sprint 3** | **MVC Controller & Web API** | `TaiKhoanController.cs` (CRUD, Tree JSON API, Validate Uniqueness) | 0.5 ngày |
| **Sprint 4** | **UI/UX Razor Views & Navigation** | `Views/TaiKhoan/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, cập nhật `menu.json` | 0.5 ngày |
| **Sprint 5** | **Opening Balances Module** | `SoDuDauKy` entity & service (Nhập số dư đầu kỳ cân đối Nợ = Có) | 1.0 ngày |
| **Sprint 6** | **Nghiệm thu toàn diện & PROD Deployment** | Code review 2 trục, CodeGraph sync, Git commit | 0.5 ngày |
