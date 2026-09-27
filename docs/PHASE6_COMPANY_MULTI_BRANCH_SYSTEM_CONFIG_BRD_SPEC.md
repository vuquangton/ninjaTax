# PHASE 6: THÔNG TIN DOANH NGHIỆP, ĐA CHI NHÁNH & THIẾT LẬP HỆ THỐNG KẾ TOÁN (COMPANY PROFILE, MULTI-BRANCH & SYSTEM SETTINGS)
## CHUẨN THÔNG TƯ 99/2025/TT-BTC, LUẬT DOANH NGHIỆP 2020, NGHỊ ĐỊNH 123/2020, THÔNG TƯ 78/2021 & LUẬT QUẢN LÝ THUẾ 38/2019

---

## 🛑 ĐÁNH GIÁ VẬN HÀNH TRÊN MÔI TRƯỜNG THỰC TẾ (PROD ENV AUDIT)

### CÂU HỎI: HỆ THỐNG HIỆN TẠI ĐÃ ĐỦ ĐIỀU KIỆN CHẠY TRÊN MÔI TRƯỜNG PROD HAY CHƯA?
### 🔴 TRẢ LỜI: **TUYỆT ĐỐI CHƯA (CANNOT OPERATE IN PROD ENV)**.

### CÁC LỖ HỔNG CHẾT NGƯỜI (CRITICAL FATAL BLOCKERS) NẾU ĐƯA VÀO PROD NGAY:
1. **Dữ liệu Pháp nhân bị Hardcoded (Cứng)**:
   - Toàn bộ thông tin `CÔNG TY TNHH NINJATAX VIỆT NAM`, MST `0109998888`, địa chỉ đang fix cứng trong mã nguồn C# và Razor views (`ThuyetMinhBctcViewModel.cs`).
   - Khách hàng doanh nghiệp thật không thể cấu hình Tên công ty, Mã số thuế, Người đại diện pháp luật, Kế toán trưởng, Giám đốc, dẫn đến xuất BCTC, Sổ cái, Tờ khai thuế bị vô hiệu về mặt pháp lý (Vi phạm Điều 16 Luật Kế toán 2015).
2. **Thiếu Khái niệm Đơn vị cơ sở / Đa chi nhánh (Branch / Multi-Unit)**:
   - Các doanh nghiệp Việt Nam có chi nhánh (hạch toán phụ thuộc hoặc hạch toán độc lập) tại các tỉnh/thành khác nhau bắt buộc phải kê khai thuế GTGT, TNCN phân bổ hoặc nộp thuế riêng theo Nghị định 126/2020/NĐ-CP và Thông tư 80/2021/TT-BTC.
   - Hệ thống hiện tại chỉ có 1 sổ cái duy nhất không gắn với `ChiNhanhId` / `CompanyId`, không thể phân tách doanh thu, chi phí, công nợ và dòng tiền giữa Trụ sở chính và Chi nhánh.
3. **Cố định Niên độ Kế toán (Fiscal Year Rigidness)**:
   - Hệ thống đang ngầm định năm tài chính bắt đầu từ ngày 01/01 đến 31/12. Các doanh nghiệp FDI có năm tài chính đặc thù (bắt đầu 01/04, 01/07, 01/10) theo quy định của Bộ Tài chính hoàn toàn không thể sử dụng.
4. **Không có Cơ chế Khóa sổ Kế toán Động (`NgayKhoaSo`)**:
   - Nhân viên kế toán hoặc người dùng có thể tùy tiện sửa/xóa chứng từ của các kỳ đã lập BCTC và nộp thuế, vi phạm nguyên tắc bảo toàn số liệu đã chốt nộp cơ quan thuế.
5. **Thiếu Cấu hình Kết nối Cổng Thuế & Hóa đơn Điện tử (E-Invoice Integration Seam)**:
   - Chưa có cấu hình Nhà cung cấp HĐĐT (VNPT, Viettel, MISA, Bkav), Chứng thư số (USB Token / HSM), Mẫu số, Ký hiệu hóa đơn (`1C26TAA`), Tài khoản kết nối API truyền nhận dữ liệu hóa đơn có mã/không mã theo Nghị định 123/2020/NĐ-CP.
6. **Thiếu Phân định Phương pháp Kế toán**:
   - Chưa cho phép chọn Phương pháp tính thuế GTGT (Khấu trừ theo Mẫu 01/GTGT hay Trực tiếp theo Mẫu 04/GTGT).
   - Chưa cho phép chọn Phương pháp tính giá xuất kho (Bình quân gia quyền cuối kỳ, Bình quân tức thời, FIFO).

---

## 1. TỔNG QUAN HỆ THỐNG & CĂN CỨ PHÁP LÝ (LEGAL FRAMEWORK)

```
+---------------------------------------------------------------------------------------+
|                              CĂN CỨ PHÁP LÝ BẮT BUỘC (2025 - 2026)                    |
+---------------------------------------------------------------------------------------+
| 1. Luật Doanh nghiệp số 59/2020/QH14: Cơ cấu tổ chức, chi nhánh, VPĐD, người đại diện.|
| 2. Luật Kế toán số 88/2015/QH13: Đơn vị kế toán, kỳ kế toán, chứng từ, chữ ký, khóa sổ.|
| 3. Thông tư 99/2025/TT-BTC: Chế độ kế toán doanh nghiệp (thay thế TT 200 & TT 133).   |
| 4. Nghị định 123/2020/NĐ-CP & TT 78/2021/TT-BTC: Hóa đơn điện tử, ký số HSM/Token.   |
| 5. Nghị định 126/2020/NĐ-CP & TT 80/2021/TT-BTC: Phân cấp quản lý thuế, khai thuế riêng |
|    và phân bổ nghĩa vụ thuế cho chi nhánh khác tỉnh/thành phố.                         |
| 6. Nghị định 132/2020/NĐ-CP: Quản lý thuế giao dịch liên kết và bên liên kết.          |
+---------------------------------------------------------------------------------------+
```

---

## 2. KIẾN TRÚC MÔ HÌNH DỮ LIỆU ĐA CHI NHÁNH & HỒ SƠ DOANH NGHIỆP (DOMAIN MODEL)

### 2.1 ASCII ART: ENTITY RELATIONSHIP DIAGRAM (ERD)

```
+------------------------------------+          1:N          +------------------------------------+
|            ThongTinDoanhNghiep     |---------------------->|             ChiNhanh               |
+------------------------------------+                       +------------------------------------+
| PK  Id (long)                      |                       | PK  Id (long)                      |
|     MaDoanhNghiep (varchar 50)     |                       | FK  DoanhNghiepId (long)           |
|     TenDoanhNghiep (varchar 255)   |                       |     MaChiNhanh (varchar 50)        |
|     TenGiaoDich (varchar 255)      |                       |     TenChiNhanh (varchar 255)      |
|     MaSoThue (varchar 20) [UNIQUE] |                       |     MaSoThueChiNhanh (varchar 20)  |
|     DiaChiTruSo (varchar 500)      |                       |     LoaiChiNhanh (Enum)            |
|     TinhThanhPho (varchar 100)     |                       |       - TruSoChinh (Head Office)   |
|     MaCoQuanThueQuanLy (varchar 20)|                       |       - PhuThuocCungTinh           |
|     TenCoQuanThue (varchar 255)    |                       |       - PhuThuocKhacTinh           |
|     NguoiDaiDienPhapLuat (vchar 255|                       |       - DocLap (Independent)       |
|     ChucDanhNguoiDaiDien (vchar 100|                       |     KeKhaiThueGtgtRieng (bool)     |
|     GiamDoc (varchar 255)          |                       |     KeKhaiThueTncnRieng (bool)     |
|     KeToanTruong (varchar 255)     |                       |     DiaChi (varchar 500)           |
|     NguoiLapBieu (varchar 255)     |                       |     MaCqtQuanLyRieng (varchar 20)  |
|     SoDienThoai (varchar 50)       |                       |     DangHoatDong (bool)            |
|     Email (varchar 100)            |                       +------------------------------------+
|     Website (varchar 100)          |                                         |
|     VonDieuLe (decimal 19,4)       |                                         | 1:N
|     LogoUrl (varchar 500)          |                                         v
+------------------------------------+                       +------------------------------------+
                  |                                          |   TẤT CẢ CHỨNG TỪ & SỔ CÁI GL      |
                  | 1:1                                      +------------------------------------+
                  v                                          | - ButToan (ChiNhanhId)             |
+------------------------------------+                       | - HoaDonMuaHang (ChiNhanhId)       |
|            CauHinhKeToan           |                       | - HoaDonBanHang (ChiNhanhId)       |
+------------------------------------+                       | - ChungTuThuChi (ChiNhanhId)       |
| PK  Id (long)                      |                       | - TaiSanCoDinh (ChiNhanhId)        |
| FK  DoanhNghiepId (long)           |                       | - BangLuongThang (ChiNhanhId)      |
|     CheDoKeToan (Enum TT99_2025)   |                       | - BaoCaoTaiChinhNam (ChiNhanhId)   |
|     DonViTienTe (varchar 10) [VND] |                       +------------------------------------+
|     ThangBatDauNienDo (int 1..12)  |
|     NgayBatDauNienDo (int 1..31)   |                       +------------------------------------+
|     PhuongPhapTinhThueGtgt (Enum)  |                       |        CauHinhHoaDonDienTu         |
|       - KhauTru (Deduction 01/GTGT)|                       +------------------------------------+
|       - TrucTiep (Direct 04/GTGT)  |                       | PK  Id (long)                      |
|     PhuongPhapXuatKho (Enum)       |                       | FK  ChiNhanhId (long)              |
|       - BinhQuanCuoiKy             |                       |     NhaCungCapHddt (Enum)          |
|       - BinhQuanLienHoan           |                       |       - VNPT, Viettel, MISA, Bkav  |
|       - FIFO                       |                       |     DuongDanApi (varchar 255)      |
|     PhuongPhapKhauHaoTscd (Enum)   |                       |     TaiKhoanApi (varchar 100)      |
|       - DuongThang (Straight-line) |                       |     MatKhauApi (varchar 255)       |
|       - SoDuGiamDanCoDieuChinh     |                       |     MauSoHoaDon (varchar 20)       |
|     NgayKhoaSo (DateTime?)         |                       |     KyHieuHoaDon (varchar 20)      |
|     CanhBaoChiVuotQuy (bool) [true]|                       |     LoaiChungThuSo (Enum USB/HSM)  |
|     CanhBaoXuatAmKho (bool) [true] |                       |     SeriChungThuSo (varchar 100)   |
+------------------------------------+                       +------------------------------------+
```

---

## 3. PHÂN LOẠI CHI NHÁNH & LUỒNG HẠCH TOÁN THEO LUẬT THUẾ (MULTI-BRANCH TAX MATRIX)

| Tiêu chí | Trụ sở chính (Head Office) | Chi nhánh phụ thuộc CÙNG TỈNH | Chi nhánh phụ thuộc KHÁC TỈNH | Chi nhánh ĐỘC LẬP |
|---|---|---|---|---|
| **Mã số thuế** | 10 số (`0109998888`) | 13 số (`0109998888-001`) | 13 số (`0109998888-002`) | 13 số (`0109998888-003`) |
| **Tổ chức bộ máy kế toán** | Đầy đủ, chịu trách nhiệm BCTC toàn DN | Không có bộ máy riêng, chuyển chứng từ về Trụ sở | Có thể lập chứng từ riêng hoặc phụ thuộc | Đơn vị kế toán đầy đủ, có BCTC riêng |
| **Kê khai thuế GTGT** | Toàn bộ DN hoặc phần trụ sở | Kê khai tập trung tại Trụ sở chính (Mẫu 01/GTGT) | Tùy cơ sở sản xuất: Khai phân bổ Mẫu 01-6/GTGT hoặc khai riêng tại địa bàn tỉnh | Khai riêng tại Cơ quan thuế quản lý chi nhánh |
| **Kê khai thuế TNDN** | Toàn bộ DN (kể cả chi nhánh) | Tập trung tại Trụ sở chính | Phân bổ tỷ lệ chi phí theo NĐ 126/2020 | Tự quyết toán riêng nếu được phân cấp |
| **Quyết toán thuế TNCN** | Khai cho NLĐ tại Trụ sở | Tập trung tại Trụ sở | Khai và nộp cho cơ quan thuế nơi chi nhánh đóng trụ sở nếu trả lương tại chi nhánh | Tự quyết toán riêng theo Mẫu 05/QTT-TNCN |
| **Báo cáo Tài chính TT99** | BCTC Tổng hợp toàn DN | Không lập BCTC riêng | Không lập BCTC riêng | Lập BCTC riêng, sau đó Trụ sở hợp nhất |

---

## 4. QUY TRÌNH NGHIỆP VỤ & SƠ ĐỒ DÒNG DỮ LIỆU (ASCII WORKFLOWS)

### 4.1 Quy trình Khởi tạo Hồ sơ Doanh nghiệp Lần đầu (Initial Setup Wizard Flow)

```
[Người dùng đăng nhập]
        |
        v
[Hệ thống kiểm tra: Có ThongTinDoanhNghiep chưa?]
        |
        +-----(Đã có)-----> [Vào Dashboard Kế toán bình thường]
        |
        +-----(Chưa có)
                  |
                  v
       +--------------------------------------------------------------+
       | BƯỚC 1: KHAI BÁO PHÁP NHÂN                                   |
       | - Nhập MST -> Tự động tra cứu API Tổng cục Thuế / ĐKKD      |
       | - Tên DN, Tên quốc tế, Tên viết tắt                          |
       | - Địa chỉ ĐKKD, Tỉnh/TP, Quận/Huyện                          |
       | - Cơ quan thuế quản lý cấp Cục / Chi cục                     |
       | - Đại diện pháp luật, Giám đốc, Kế toán trưởng, Thủ quỹ      |
       +--------------------------------------------------------------+
                  |
                  v
       +--------------------------------------------------------------+
       | BƯỚC 2: CẤU HÌNH KẾ TOÁN TT99/2025/TT-BTC                    |
       | - Chế độ: Thông tư 99/2025/TT-BTC (Mặc định bắt buộc)       |
       | - Năm tài chính: Bắt đầu 01/01 (hoặc 01/04, 01/07, 01/10)    |
       | - Đồng tiền hạch toán: VND (hoặc Ngoại tệ)                   |
       | - Phương pháp tính thuế GTGT: [X] Khấu trừ   [ ] Trực tiếp    |
       | - Phương pháp tính giá xuất kho: Bình quân cuối kỳ / FIFO    |
       +--------------------------------------------------------------+
                  |
                  v
       +--------------------------------------------------------------+
       | BƯỚC 3: THIẾT LẬP CƠ CẤU TỔ CHỨC & CHI NHÁNH                 |
       | - Tự động tạo Chi nhánh mặc định: "Trụ sở chính (HO)"        |
       | - Tùy chọn thêm Chi nhánh phụ thuộc / độc lập                |
       +--------------------------------------------------------------+
                  |
                  v
       +--------------------------------------------------------------+
       | BƯỚC 4: KẾT NỐI HÓA ĐƠN ĐIỆN TỬ & CHỮ KÝ SỐ (Tùy chọn)       |
       | - Chọn nhà cung cấp: VNPT / Viettel / MISA / Bkav            |
       | - Mẫu số, Ký hiệu hóa đơn: 1C26TAA                           |
       | - Cấu hình Token / HSM                                       |
       +--------------------------------------------------------------+
                  |
                  v
       [Hoàn tất thiết lập -> Khởi tạo Hệ thống Tài khoản TT99 và Sổ Kế Toán]
```

### 4.2 Cơ chế Kiểm soát Khóa sổ Kế toán (`NgayKhoaSo`)

```
             [Người dùng thực hiện Thêm / Sửa / Xóa Chứng từ GL]
                                      |
                                      v
          [Lấy NgayHachToan của chứng từ so sánh với NgayKhoaSo]
                                      |
                 +--------------------+--------------------+
                 |                                         |
     (NgayHachToan <= NgayKhoaSo)              (NgayHachToan > NgayKhoaSo)
                 |                                         |
                 v                                         v
   +------------------------------+          +------------------------------+
   | CHẶN TUYỆT ĐỐI (EXCEPTION)   |          | CHO PHÉP THỰC HIỆN           |
   | Báo lỗi: "Chứng từ thuộc kỳ  |          | - Ghi sổ kế toán bình thường |
   | đã khóa sổ ngày DD/MM/YYYY.  |          | - Cập nhật số dư Sổ cái GL   |
   | Muốn sửa phải mở khóa sổ."   |          +------------------------------+
   +------------------------------+
```

---

## 5. USE CASES CHI TIẾT (USE CASE SPECIFICATIONS)

### 📌 UC1: Khởi tạo và Cập nhật Hồ sơ Doanh nghiệp (Company Profile)
- **Actor**: Quản trị viên hệ thống (Admin), Kế toán trưởng.
- **Preconditions**: Đã đăng nhập vào hệ thống.
- **Main Flow (Happy Path)**:
  1. Người dùng vào menu `Hệ thống` &rarr; `Thông tin Doanh nghiệp`.
  2. Hệ thống hiển thị form thông tin: Tên DN, Mã số thuế, Địa chỉ, Người đại diện, Cơ quan thuế.
  3. Người dùng nhập thông tin hoặc bấm `Tra cứu mã số thuế tự động`.
  4. Người dùng nhấn `Lưu thay đổi`.
  5. Hệ thống kiểm tra tính hợp lệ của MST (10 chữ số chuẩn mod 11), lưu vào CSDL, cập nhật tiêu đề tất cả Báo cáo tài chính, Tờ khai thuế, Phiếu thu, Phiếu chi, Hóa đơn.
  6. Hệ thống hiển thị thông báo thành công.
- **Alternative Path**:
  - Người dùng tải lên Logo công ty &rarr; Hệ thống kiểm tra định dạng `.png, .jpg, .svg`, kích thước $< 2$MB, lưu vào thư mục `wwwroot/uploads/company/` và cập nhật đường dẫn `LogoUrl`.
- **Exception Path**:
  - Nhập MST sai định dạng &rarr; Báo lỗi: *"Mã số thuế doanh nghiệp phải gồm đúng 10 chữ số (hoặc 13 số đối với chi nhánh), không hợp lệ theo thuật toán Tổng cục Thuế."*

### 📌 UC2: Quản lý Danh mục Chi nhánh (Branch Management)
- **Actor**: Kế toán trưởng.
- **Preconditions**: Doanh nghiệp đã được thiết lập.
- **Main Flow**:
  1. Người dùng chọn `Hệ thống` &rarr; `Cơ cấu tổ chức & Chi nhánh`.
  2. Bấm `Thêm mới chi nhánh`.
  3. Khai báo: Mã chi nhánh (VD: `CN-HCM`), Tên chi nhánh, MST 13 số (`0109998888-001`), Địa chỉ, Loại chi nhánh (Phụ thuộc cùng tỉnh, Phụ thuộc khác tỉnh, Độc lập).
  4. Tích chọn chế độ kê khai thuế GTGT/TNCN riêng (nếu chi nhánh khác tỉnh).
  5. Bấm `Lưu`.
  6. Hệ thống tạo Chi nhánh và cập nhật bộ chọn Chi nhánh (Branch Switcher) trên thanh Top Navigation.
- **Exception Path**:
  - Trùng mã chi nhánh hoặc trùng MST chi nhánh &rarr; Hệ thống từ chối lưu và báo đỏ.

### 📌 UC3: Thiết lập Kỳ Kế Toán & Khóa Sổ (`NgayKhoaSo`)
- **Actor**: Kế toán trưởng.
- **Main Flow**:
  1. Người dùng vào `Thiết lập Kế toán` &rarr; Tab `Kỳ kế toán & Khóa sổ`.
  2. Xem ngày khóa sổ hiện tại (Ví dụ: `31/12/2025`).
  3. Chọn ngày khóa sổ mới (Ví dụ: sau khi đã nộp BCTC Quý 1/2026, chọn khóa sổ đến ngày `31/03/2026`).
  4. Bấm `Thực hiện Khóa sổ`.
  5. Hệ thống kiểm tra: Nếu còn chứng từ ở trạng thái `ChuaGhiSo` trước ngày `31/03/2026`, cảnh báo danh sách chứng từ chưa ghi sổ. Người dùng xác nhận hoặc xử lý trước khi khóa.
  6. Sau khi khóa thành công: Mọi thao tác thêm/sửa/xóa chứng từ có `NgayHachToan <= 31/03/2026` đều bị chặn.

### 📌 UC4: Thiết lập Phương pháp Kế toán & Thuế
- **Actor**: Kế toán trưởng.
- **Main Flow**:
  1. Thiết lập Phương pháp tính thuế GTGT: Khấu trừ (Mẫu 01/GTGT) / Trực tiếp (Mẫu 04/GTGT).
  2. Thiết lập Phương pháp tính giá xuất kho: Bình quân gia quyền cuối kỳ / Bình quân tức thời / FIFO.
  3. Thiết lập Khấu hao TSCĐ: Đường thẳng (Straight-line).
  4. Bấm `Lưu cấu hình`.
  5. Hệ thống áp dụng cấu hình này làm quy tắc hạch toán mặc định cho toàn bộ các phân hệ Mua hàng, Bán hàng, Kho, Tài sản, BCTC.

### 📌 UC5: Chuyển đổi Đơn vị làm việc (Branch Context Switcher)
- **Actor**: Mọi người dùng kế toán được phân quyền.
- **Main Flow**:
  1. Trên góc phải thanh Header/Navbar, người dùng nhấp vào Dropdown `Đơn vị làm việc: [Tất cả chi nhánh / Trụ sở chính / CN Hồ Chí Minh / CN Đà Nẵng]`.
  2. Chọn `CN Hồ Chí Minh`.
  3. Phiên làm việc (Session/Cookie) cập nhật `CurrentBranchId = CN_HCM.Id`.
  4. Toàn bộ danh sách chứng từ, hóa đơn, tồn kho, công nợ tự động lọc theo `ChiNhanhId == CurrentBranchId`.
  5. Khi lập chứng từ mới, `ChiNhanhId` tự động gắn với đơn vị đang chọn.

---

## 6. THIẾT KẾ GIAO DIỆN & BẢN VẼ WIREFRAME (ASCII ART UI/UX)

### 6.1 Màn hình Thiết lập Thông tin Doanh nghiệp & Kế toán (Company Settings View)

```
+----------------------------------------------------------------------------------------------------+
|  ninjaTax  |  [Chi nhánh: Trụ sở chính v]               [Năm: 2026 v]        [Admin: Kế toán trưởng] |
+----------------------------------------------------------------------------------------------------+
| [Trang Chủ] [Mua Hàng] [Bán Hàng] [Kho] [Tiền Mặt/NH] [Tiền Lương] [BCTC] [Thuế] [HỆ THỐNG v]       |
+----------------------------------------------------------------------------------------------------+
|                                                                                                    |
|  THIẾT LẬP THÔNG TIN DOANH NGHIỆP & CẤU HÌNH KẾ TOÁN TT99/2025/TT-BTC                              |
|  <Quản lý định danh pháp nhân, đơn vị cơ sở, phương pháp tính thuế và khóa sổ kế toán>             |
|                                                                                                    |
|  [Tab 1: Hồ Sơ Pháp Nhân]  [Tab 2: Cấu Hình Kế Toán & Thuế]  [Tab 3: Đa Chi Nhánh]  [Tab 4: HĐĐT] |
|  ------------------------------------------------------------------------------------------------- |
|                                                                                                    |
|  1. THÔNG TIN ĐĂNG KÝ DOANH NGHIỆP                                                                 |
|  +-------------------------------------+  +-----------------------------------------------------+  |
|  | Mã số thuế (MST) (*):               |  | Tên doanh nghiệp (Tiếng Việt) (*):                  |  |
|  | [ 0109998888                      ] |  | [ CÔNG TY CỔ PHẦN CÔNG NGHỆ NINJATAX VIỆT NAM      ] |  |
|  | [ Tra cứu tự động từ Tổng cục Thuế ] |  +-----------------------------------------------------+  |
|  +-------------------------------------+  +-----------------------------------------------------+  |
|  | Tên giao dịch viết tắt:             |  | Tên tiếng Anh (quốc tế):                            |  |
|  | [ NINJATAX JSC                    ] |  | [ NINJATAX VIETNAM TECHNOLOGY JOINT STOCK COMPANY ] |  |
|  +-------------------------------------+  +-----------------------------------------------------+  |
|  | Địa chỉ trụ sở chính (*):                                                                       |
|  | [ Tầng 10, Tòa nhà Keangnam Landmark 72, Đường Phạm Hùng, Q. Nam Từ Liêm, TP. Hà Nội          ] |
|  +-------------------------------------------------------------------------------------------------+
|  | Tỉnh / Thành phố: [ TP. Hà Nội                 v]  Quận / Huyện: [ Quận Nam Từ Liêm           v] |
|  | Cơ quan thuế cấp Cục:  [ Cục Thuế TP. Hà Nội                                                  v] |
|  | Cơ quan thuế quản lý trực tiếp: [ Chi cục Thuế Quận Nam Từ Liêm                               v] |
|  +-------------------------------------------------------------------------------------------------+
|                                                                                                    |
|  2. THÔNG TIN NGƯỜI ĐẠI DIỆN & KÝ DUYỆT SỔ SÁCH BCTC                                              |
|  +-------------------------------------------------+  +------------------------------------------+ |
|  | Người đại diện theo pháp luật (*):              |  | Chức danh:                               | |
|  | [ Nguyễn Văn Doanh                              ] |  | [ Tổng Giám Đốc                       ] | |
|  +-------------------------------------------------+  +------------------------------------------+ |
|  | Giám đốc / Tổng giám đốc:                       |  | Kế toán trưởng (*):                      | |
|  | [ Nguyễn Văn Doanh                              ] |  | [ Trần Thị Kế Toán                    ] | |
|  +-------------------------------------------------+  +------------------------------------------+ |
|  | Người lập biểu BCTC:                            |  | Thủ quỹ:                                 | |
|  | [ Lê Văn Lập Biểu                               ] |  | [ Phạm Thị Thủ Quỹ                    ] | |
|  +-------------------------------------------------+  +------------------------------------------+ |
|                                                                                                    |
|  3. VỐN ĐIỀU LỆ & THÔNG TIN LIÊN HỆ                                                                |
|  +------------------------+  +------------------------+  +---------------------------------------+ |
|  | Vốn điều lệ (VNĐ):     |  | Số điện thoại:         |  | Email nhận thông báo thuế:            | |
|  | [ 20,000,000,000      ] |  | [ 024.3999.8888        ] |  | [ ketoan@ninjatax.vn                  ] | |
|  +------------------------+  +------------------------+  +---------------------------------------+ |
|                                                                                                    |
|  [ HỦY BỎ ]                                                          [ LƯU THÔNG TIN DOANH NGHIỆP ] |
+----------------------------------------------------------------------------------------------------+
```

### 6.2 Màn hình Tab Cấu hình Kế toán & Khóa sổ (Accounting & Lock Configuration)

```
+----------------------------------------------------------------------------------------------------+
|  [Tab 1: Hồ Sơ Pháp Nhân]  [Tab 2: Cấu Hình Kế Toán & Thuế]  [Tab 3: Đa Chi Nhánh]  [Tab 4: HĐĐT] |
|  ------------------------------------------------------------------------------------------------- |
|                                                                                                    |
|  1. CHẾ ĐỘ KẾ TOÁN & ĐỒNG TIỀN HẠCH TOÁN                                                           |
|  - Chế độ kế toán áp dụng: (*)                                                                     |
|    (o) Thông tư 99/2025/TT-BTC (Chế độ kế toán doanh nghiệp mới nhất - Bắt buộc từ 01/01/2026)     |
|        [Lưu ý: Nghiêm cấm tuyệt đối TK 911; kết chuyển trực tiếp doanh thu/chi phí vào TK 421]     |
|  - Đồng tiền hạch toán: (*) [ VND - Đồng Việt Nam          v]                                      |
|                                                                                                    |
|  2. NIÊN ĐỘ TÀI CHÍNH & PHƯƠNG PHÁP TÍNH THUẾ                                                      |
|  - Ngày bắt đầu năm tài chính: [ Ngày 01 ] Tháng: [ Tháng 01 v]                                    |
|    -> Kỳ kế toán năm hiện tại: 01/01/2026 đến 31/12/2026                                           |
|  - Phương pháp tính thuế GTGT: (*)                                                                 |
|    (o) Phương pháp Khấu trừ (Kê khai Tờ khai 01/GTGT, Hạch toán TK 133, TK 3331)                   |
|    ( ) Phương pháp Trực tiếp trên doanh thu (Kê khai Tờ khai 04/GTGT)                              |
|  - Phương pháp tính giá trị hàng xuất kho: (*)                                                     |
|    (o) Bình quân gia quyền cuối kỳ                                                                 |
|    ( ) Bình quân gia quyền tức thời (liên hoàn sau mỗi lần nhập)                                   |
|    ( ) Nhập trước - Xuất trước (FIFO)                                                              |
|  - Phương pháp trích khấu hao TSCĐ: (*)                                                            |
|    (o) Phương pháp đường thẳng (Theo Thông tư 45/2013/TT-BTC)                                      |
|    ( ) Phương pháp số dư giảm dần có điều chỉnh                                                    |
|                                                                                                    |
|  3. KHÓA SỔ KẾ TOÁN & KIỂM SOÁT RỦI RO                                                             |
|  - Ngày khóa sổ kế toán hiện tại: [ 31/12/2025 ]      [ MỞ KHÓA SỔ ] [ KHÓA SỔ ĐẾN NGÀY MỚI... ]  |
|    (!) Chứng từ có Ngày hạch toán <= Ngày khóa sổ sẽ KHÔNG ĐƯỢC THÊM, SỬA HOẶC XÓA.               |
|  - [X] Bật cảnh báo khi chi tiền vượt quá tồn quỹ tiền mặt thời điểm (Chống âm quỹ TK 1111)        |
|  - [X] Bật cảnh báo khi xuất kho vượt quá số lượng tồn kho (Chống âm kho TK 156)                  |
|  - [X] Bật cảnh báo đỏ khi thanh toán hóa đơn mua hàng >= 20 triệu bằng tiền mặt (Bẫy B4 TNDN)     |
|                                                                                                    |
|  [ HỦY BỎ ]                                                                   [ LƯU CẤU HÌNH ]     |
+----------------------------------------------------------------------------------------------------+
```

### 6.3 Màn hình Quản lý Cơ cấu Tổ chức & Chi nhánh (Multi-Branch Management)

```
+----------------------------------------------------------------------------------------------------+
|  [Tab 1: Hồ Sơ Pháp Nhân]  [Tab 2: Cấu Hình Kế Toán & Thuế]  [Tab 3: Đa Chi Nhánh]  [Tab 4: HĐĐT] |
|  ------------------------------------------------------------------------------------------------- |
|                                                                                                    |
|  DANH SÁCH ĐƠN VỊ CƠ SỞ & CHI NHÁNH                            [ + Thêm Mới Chi Nhánh ]           |
|  ------------------------------------------------------------------------------------------------- |
|  MÃ ĐƠN VỊ | TÊN ĐƠN VỊ          | LOẠI CHI NHÁNH        | MST 13 SỐ       | KHAI THUẾ RIÊNG| TT   |
|  ----------+---------------------+-----------------------+-----------------+----------------+------|
|  HO-01     | Trụ sở chính Hà Nội | Trụ sở chính (HO)     | 0109998888      | [Tập trung]    | Hoạt |
|  CN-HCM    | Chi nhánh TP.HCM    | Phụ thuộc (Khác tỉnh) | 0109998888-001  | [X] GTGT riêng | Hoạt |
|  CN-DN     | Chi nhánh Đà Nẵng   | Phụ thuộc (Khác tỉnh) | 0109998888-002  | [X] GTGT riêng | Hoạt |
|  NM-BN     | Nhà máy Bắc Ninh    | Phụ thuộc (Khác tỉnh) | 0109998888-003  | [ ] Phân bổ 016| Hoạt |
|  ------------------------------------------------------------------------------------------------- |
|  Tổng số: 4 đơn vị cơ sở (1 Trụ sở chính, 3 Chi nhánh/Nhà máy).                                   |
+----------------------------------------------------------------------------------------------------+
```

---

## 7. QUY TẮC NGHIỆP VỤ & BẤT BIẾN KẾ TOÁN (BUSINESS RULES & INVARIANTS)

- **BR-CP-01 (Mã số thuế chuẩn quốc gia)**:
  - MST của Doanh nghiệp (`ThongTinDoanhNghiep.MaSoThue`) phải đúng 10 chữ số, tính theo thuật toán Trọng số Modulo 11 của Tổng cục Thuế Việt Nam:
    $$\text{CheckSum} = 10 - \left(\sum_{i=1}^{9} d_i \times w_i\right) \bmod 11$$
    với bộ trọng số $w = [31, 29, 23, 19, 17, 13, 7, 5, 3]$.
  - MST của Chi nhánh phải đúng định dạng `XXXXXXXXXX-YYY` (10 số của công ty mẹ + dấu gạch nối + 3 chữ số từ `001` đến `999`).
- **BR-CP-02 (Bất biến Trụ sở chính)**:
  - Một doanh nghiệp bắt buộc phải có DUY NHẤT một Chi nhánh mang loại `LoaiChiNhanh.TruSoChinh`.
  - Không được phép xóa Chi nhánh Trụ sở chính.
- **BR-CP-03 (Bảo toàn Khóa sổ Kế toán)**:
  - Mọi thao tác thêm mới, sửa đổi, xóa bỏ chứng từ (`ButToan`, `ChungTuThuChi`, `HoaDonMuaHang`, `HoaDonBanHang`, `BangLuongThang`, `BangTinhKhauHao`) có `NgayHachToan <= CauHinhKeToan.NgayKhoaSo` đều bị `DbUpdateException` hoặc `InvalidOperationException` ngăn chặn ngay tại Application Seam và Domain Invariant.
- **BR-CP-04 (Hợp nhất Báo cáo Tài chính Đa Chi Nhánh)**:
  - Khi xem BCTC toàn công ty (`ChiNhanhId == null`), hệ thống tự động bù trừ các bút toán giao dịch nội bộ giữa Trụ sở chính và Chi nhánh (TK 136 - Phải thu nội bộ và TK 336 - Phải trả nội bộ).
- **BR-CP-05 (Chế độ Kế toán TT99 Bắt buộc)**:
  - Trong niên độ 2026 trở đi, Chế độ kế toán mặc định là `TT99_2025`. Danh mục tài khoản bị cấm tuyệt đối TK 911 trong toàn bộ mọi chi nhánh.

---

## 8. LỘ TRÌNH TRIỂN KHAI THEO PHƯƠNG PHÁP TDD & SEAM ARCHITECTURE (IMPLEMENTATION ROADMAP)

```
+---------------------------------------------------------------------------------------------+
|                                    ROADMAP PHASE 6: 4 SPRINTS                               |
+---------------------------------------------------------------------------------------------+
| SPRINT 1: Core Domain Entities & Multi-DB Seam (Entities, DbContext, Migrations)            |
|   - Tạo ThongTinDoanhNghiep.cs, ChiNhanh.cs, CauHinhKeToan.cs, CauHinhHoaDonDienTu.cs.     |
|   - Thêm ChiNhanhId (nullable) vào ButToan, HoaDon, ChungTuThuChi, BangLuong, TaiSan.       |
|   - Migration Sqlite & Multi-DB seeding với thông tin công ty mẫu.                          |
+---------------------------------------------------------------------------------------------+
| SPRINT 2: Business Services & Lock Date Invariant (Service Seams)                           |
|   - Interface ICompanyService & Implementation CompanyService.                              |
|   - Cập nhật ButToanService: Kiểm tra khóa sổ NgayKhoaSo trước khi ghi sổ.                  |
|   - Cập nhật FinancialReportService & TaxFinalizationService lấy động thông tin DN.         |
+---------------------------------------------------------------------------------------------+
| SPRINT 3: Web UI & Branch Context Middleware (MVC Controllers & Razor Views)                |
|   - CompanyController: Index, Edit, BranchManager, AccountingConfig.                        |
|   - Topbar Branch Switcher dropdown: lưu cookie/session đơn vị làm việc hiện tại.           |
|   - Cập nhật layout và các trang in ấn phiếu thu/chi, hóa đơn, BCTC lấy động logo và cty.  |
+---------------------------------------------------------------------------------------------+
| SPRINT 4: TDD Test Suite & Security Validation                                              |
|   - CompanyTaxIdValidationTests: kiểm tra thuật toán Mod 11 MST và định dạng 13 số.         |
|   - AccountingBookLockingTests: kiểm thử chặn sửa chứng từ trước ngày khóa sổ.              |
|   - MultiBranchFilterTests: kiểm thử phân tách dữ liệu theo ChiNhanhId.                     |
|   - Build verification 0 warnings, 0 errors, git sync master.                               |
+---------------------------------------------------------------------------------------------+
```

---

## 9. ĐẶC TẢ TEST CASES TDD (TEST SPECIFICATION)

1. `CompanyProfile_MstHopLe_LuuThanhCong`: Nhập MST 10 chữ số hợp lệ &rarr; Lưu thành công.
2. `CompanyProfile_MstSaiModulo11_NemNgoaiLe`: Nhập MST sai chữ số kiểm tra &rarr; Báo lỗi `ArgumentException`.
3. `Branch_Mst13So_DungDinhDang`: Nhập chi nhánh MST `0109998888-001` &rarr; Thành công.
4. `KhoaSổKeToan_SuaChungTuTruocNgayKhoaSo_ChanTuyetDoi`: Thiết lập `NgayKhoaSo = 31/12/2025`. Sửa chứng từ ngày `15/10/2025` &rarr; Ném `InvalidOperationException: Kỳ kế toán đã khóa sổ`.
5. `KhoaSổKeToan_ThemChungTuSauNgayKhoaSo_ThanhCong`: Thêm chứng từ ngày `10/01/2026` &rarr; Thành công.
6. `MultiBranch_GiaoDichChiNhanhHCM_KhongXuatHienKhiLocChiNhanhDaNang`: Chứng từ của `CN_HCM` không bị lẫn sang sổ của `CN_DN`.

---
*Tài liệu được soạn thảo bởi Kế toán trưởng & BA Lead 20+ năm kinh nghiệm kế toán Việt Nam, đối soát nghiêm ngặt theo Thông tư 99/2025/TT-BTC và hệ thống văn bản pháp luật hiện hành.*
