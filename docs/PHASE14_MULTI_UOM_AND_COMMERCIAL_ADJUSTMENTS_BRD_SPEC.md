# TÀI LIỆU YÊU CẦU NGHIỆP VỤ & ĐẶC TẢ KỸ THUẬT (BRD & TECHNICAL SPECIFICATION)
## PHASE 14: ĐƠN VỊ TÍNH QUY ĐỔI (MULTI-UOM), ĐIỀU CHUYỂN KHO NỘI BỘ & CÁC KHOẢN GIẢM TRỪ DOANH THU/GIẢM TRÙ MUA HÀNG (HÀNG BÁN/MUA TRẢ LẠI 5212, CHIẾT KHẤU THƯƠNG MẠI 5211)

- **Dự án**: `ninjaTax` - Nền tảng Kế toán & Thuế Doanh nghiệp Tinh gọn
- **Phiên bản tài liệu**: 14.0-COMMERCIAL-DISTRIBUTION
- **Tác giả**: Principal Software Engineer & Chief Accountant (20+ năm kinh nghiệm ERP & Kế toán Doanh nghiệp Việt Nam)
- **Cơ sở pháp lý & Chuẩn mực**:
  - **VAS 02** (Hàng tồn kho), **VAS 14** (Doanh thu và thu nhập khác).
  - **Thông tư số 99/2025/TT-BTC** & **Thông tư số 200/2014/TT-BTC** (Điều 81 - TK 521 Các khoản giảm trừ doanh thu, Điều 161 - Phiếu xuất kho kiêm vận chuyển nội bộ).
  - **Nghị định số 123/2020/NĐ-CP** (Điều 4, Điều 14 - Quy định về hóa đơn điều chỉnh, hóa đơn thay thế, hàng trả lại).
  - **Thông tư số 78/2021/TT-BTC** hướng dẫn thực hiện một số điều của Luật Quản lý thuế.
- **Trạng thái**: Approved for Implementation

---

## 1. BỐI CẢNH DOANH NGHIỆP & CĂN CỨ PHÁP LÝ (BUSINESS CONTEXT & REGULATORY BASIS)

### 1.1. Nghiệp vụ 1: Đơn vị tính Quy đổi Đa cấp (Multi-UoM Conversion)
1. **Thực trạng**: Doanh nghiệp thương mại, phân phối, bán buôn/bán lẻ và sản xuất thường xuyên nhập mua theo đơn vị lớn nhưng xuất bán hoặc quản lý nội bộ theo đơn vị nhỏ:
   - Ngành F&B / Tiêu dùng nhanh (FMCG): Nhập **Thùng** (24 Lon) $\rightarrow$ Bán buôn **Thùng**, bán lẻ **Lốc** (6 Lon) hoặc **Lon**.
   - Ngành Thép / Xây dựng: Nhập **Tấn** $\rightarrow$ Bán buôn **Tấn**, xuất công trình **Kg** hoặc **Cây**.
   - Ngành Dược phẩm: Nhập **Thùng** $\rightarrow$ Bán **Hộp** $\rightarrow$ Bán **Vỉ** $\rightarrow$ Bán **Viên**.
2. **Yêu cầu Kế toán & Kho**:
   - Mỗi mặt hàng (`VatTuHangHoa`) có một **Đơn vị tính cơ bản (Base UoM)** dùng để theo dõi tồn kho trên Sổ Thẻ kho (Mẫu S12-DN) và Báo cáo Nhập - Xuất - Tồn (Mẫu S10-DN).
   - Hệ thống cho phép khai báo danh mục **Đơn vị tính quy đổi (Secondary UoM)** với tỷ lệ quy đổi cố định hoặc linh hoạt:
     $$1 \text{ Đơn vị phụ} = k \times \text{Đơn vị cơ bản} \quad (k > 0)$$
   - Trên các chứng từ Mua hàng (`HoaDonMuaHang`), Bán hàng (`HoaDonBanHang`), Phiếu Nhập kho (`PhieuNhapKho`), Phiếu Xuất kho (`PhieuXuatKho`), người dùng được phép chọn Đơn vị tính giao dịch. Hệ thống tự động tính quy đổi về Đơn vị cơ bản để kiểm tra tồn kho và định khoản giá vốn.

### 1.2. Nghiệp vụ 2: Điều chuyển Kho Nội bộ (Internal Warehouse Transfer)
1. **Thực trạng**: Doanh nghiệp sở hữu nhiều kho lưu trữ (Kho Tổng Hà Nội, Kho Chi nhánh Đà Nẵng, Kho Showroom, Kho Hàng Lỗi/Bảo Hành). Khi vận chuyển hàng giữa các kho nội bộ:
   - Không phát sinh doanh thu hay giá vốn bán hàng.
   - Hàng hóa chỉ thay đổi địa điểm bảo quản: Tổng tài sản hàng tồn kho không đổi.
2. **Yêu cầu Kế toán & Pháp lý**:
   - Theo Thông tư 200/2014/TT-BTC và Thông tư 99/2025/TT-BTC, chứng từ lưu hành trên đường bắt buộc là **Phiếu xuất kho kiêm vận chuyển nội bộ** (Mẫu ban hành kèm Nghị định 123/2020/NĐ-CP).
   - Hạch toán Kế toán:
     $$\text{Nợ TK 1561/152 (Kho Nhận)} \quad / \quad \text{Có TK 1561/152 (Kho Xuất)}$$
   - Bảo đảm bất biến: Chặn xuất chuyển kho nếu Kho Xuất không đủ tồn kho khả dụng; tự động kế thừa đơn giá xuất kho sang đơn giá nhập kho tại Kho Nhận để bảo toàn nguyên giá.

### 1.3. Nghiệp vụ 3: Hàng bán bị trả lại (Sales Returns - TK 5212) & Giảm trừ Doanh thu
1. **Thực trạng**: Khách hàng trả lại hàng hóa do vi phạm hợp đồng, hàng kém chất lượng, sai quy cách kỹ thuật.
2. **Yêu cầu Kế toán theo VAS 14 & TT 200 / TT 99**:
   - Khi nhận hàng trả lại, bên bán ghi nhận giảm trừ doanh thu và nhập lại kho hàng hóa bị trả:
     - **Bút toán 1 (Giảm trừ doanh thu & thuế GTGT đầu ra)**:
       $$\text{Nợ TK 5212 (Hàng bán bị trả lại)} \quad (\text{Doanh số chưa thuế})$$
       $$\text{Nợ TK 33311 (Thuế GTGT đầu ra giảm)} \quad (\text{Tiền thuế})$$
       $$\text{Có TK 131 / 111 / 112} \quad (\text{Tổng số tiền thanh toán/giảm trừ công nợ})$$
     - **Bút toán 2 (Nhập lại kho & giảm giá vốn hàng bán)**:
       $$\text{Nợ TK 1561 / 152} \quad (\text{Nhập lại kho theo giá vốn đích danh lô xuất})$$
       $$\text{Có TK 632 (Giá vốn hàng bán giảm)}$$
   - **Xử lý Cuối kỳ (Period Closing)**:
     - Tài khoản 5212 không có số dư cuối kỳ. Cuối kỳ kết chuyển giảm trừ doanh thu sang TK 511:
       $$\text{Nợ TK 511} \quad / \quad \text{Có TK 5212}$$
     - Sau đó Doanh thu thuần trên TK 511 mới kết chuyển sang TK 911 theo chuẩn Thông tư 99/2025/TT-BTC.

### 1.4. Nghiệp vụ 4: Hàng mua trả lại & Chiết khấu thương mại mua/bán (Purchase Returns & Trade Discounts - TK 5211)
1. **Hàng mua trả lại nhà cung cấp**:
   - Trả lại hàng cho nhà cung cấp:
     $$\text{Nợ TK 331 / 111 / 112} \quad / \quad \text{Có TK 1561/152} \quad / \quad \text{Có TK 1331}$$
   - Tự động sinh Phiếu Xuất Kho trả hàng NCC và giảm công nợ phải trả.
2. **Chiết khấu thương mại (Trade Discount - TK 5211)**:
   - Khách hàng mua đạt sản lượng lớn theo thỏa thuận sau nhiều lần mua hàng, doanh nghiệp lập hóa đơn điều chỉnh giảm doanh thu:
     $$\text{Nợ TK 5211 (Chiết khấu thương mại)} \quad / \quad \text{Nợ TK 33311} \quad / \quad \text{Có TK 131}$$
   - Cuối kỳ kết chuyển:
     $$\text{Nợ TK 511} \quad / \quad \text{Có TK 5211}$$

---

## 2. KIẾN TRÚC PHÂN CHIA LÁT CẮT TRIỂN KHAI (SLICES ROADMAP: CORE TO EDGE)

```
+=======================================================================================================+
|                                    PHASE 14: CORE TO EDGE SLICE ARCHITECTURE                          |
+=======================================================================================================+
|                                                                                                       |
|  [SLICE 1: MULTI-UOM CONVERSION ENGINE]                                                               |
|  - Entity: DonViTinhQuyDoi (Master UoM Table)                                                         |
|  - Tích hợp Dropdown UoM trên ChiTietHoaDon, ChiTietNhapKho, ChiTietXuatKho                           |
|  - Conversion Engine: Tự động quy đổi SoLuongGiaoDich * TyLeQuyDoi -> SoLuongCoBan                     |
|                                                                                                       |
|  [SLICE 2: INTERNAL WAREHOUSE TRANSFER MODULE]                                                        |
|  - Entity: PhieuDieuChuyenKho, ChiTietDieuChuyenKho                                                   |
|  - Engine: DieuchuyenKhoService (Chặn âm Kho Xuất, tự động sinh cặp xuất-nhập kho liên hoàn)          |
|  - Hạch toán: Nợ 1561(Kho Đến) / Có 1561(Kho Đi)                                                       |
|                                                                                                       |
|  [SLICE 3: COMMERCIAL ADJUSTMENTS & RETURNS (TK 5211, 5212, HÀNG MUA TRẢ LẠI)]                        |
|  - Entity: HangBanTraLai, ChiTietHangBanTraLai, HangMuaTraLai, ChiTietHangMuaTraLai                   |
|  - Double-entry Engine:                                                                               |
|    + Hàng bán trả lại: Nợ 5212, Nợ 33311 / Có 131 VÀ Nợ 1561 / Có 632                                 |
|    + Hàng mua trả lại: Nợ 331 / Có 1561, Có 1331                                                      |
|    + Chiết khấu thương mại: Nợ 5211, Nợ 33311 / Có 131                                                |
|                                                                                                       |
|  [SLICE 4: PERIOD CLOSING INTEGRATION & STATUTORY REPORTING B02-DN]                                   |
|  - Nâng cấp PeriodClosingService: Kết chuyển Nợ 511 / Có 5211, 5212 trước khi KC sang TK 911          |
|  - Cập nhật Báo cáo KQKD (Mẫu B02-DN): Chỉ tiêu 02 (Các khoản giảm trừ doanh thu)                     |
|                                                                                                       |
+=======================================================================================================+
```

---

## 3. ĐẶC TẢ THỰC THỂ DỮ LIỆU & QUAN HỆ EF CORE (DATA CONTRACTS & SCHEMA)

### 3.1. Entity `DonViTinhQuyDoi` (UoM Conversion Table)
- `Id` (`bigint`, PK, Identity).
- `VatTuHangHoaId` (`bigint`, NotNull, FK -> `VatTuHangHoa`).
- `TenDonViTinh` (`nvarchar(50)`, NotNull) - VD: `Thùng`, `Két`, `Lốc`, `Hộp`.
- `TyLeQuyDoi` (`decimal(19, 6)`, NotNull) - Tỷ lệ so với ĐVT cơ bản (VD: 1 Thùng = 24 Lon $\rightarrow$ TyLe = 24).
- `PhepTinh` (`enum`: `Nhan = 1`, `Chia = 2`) - Mặc định `Nhan`.
- `DonGiaBanQuyDoi` (`decimal(19, 4)`, default `0m`) - Giá bán niêm yết theo ĐVT phụ.
- `LaDonViBanMacDinh` (`bool`, default `false`).
- `LaDonViMuaMacDinh` (`bool`, default `false`).
- `DangHoatDong` (`bool`, default `true`).

### 3.2. Entity `PhieuDieuChuyenKho` & `ChiTietDieuChuyenKho`
- **Header (`PhieuDieuChuyenKho`)**:
  - `Id` (`bigint`, PK, Identity).
  - `ChiNhanhId` (`bigint`, NotNull, FK -> `ChiNhanh`).
  - `SoPhieu` (`nvarchar(50)`, NotNull, Unique per Year) - VD: `DCK-2026-0001`.
  - `NgayDieuChuyen` (`DateTime`, NotNull).
  - `NgayHachToan` (`DateTime`, NotNull).
  - `KhoXuatId` (`bigint`, NotNull, FK -> `Kho`).
  - `KhoNhapId` (`bigint`, NotNull, FK -> `Kho`).
  - `NguoiVanChuyen` (`nvarchar(255)`, Nullable).
  - `PhuongTienVanChuyen` (`nvarchar(255)`, Nullable).
  - `LenhDieuDongSo` (`nvarchar(100)`, Nullable) - Số lệnh điều động theo NĐ 123.
  - `TongSoLuong` (`decimal(19, 4)`, NotNull).
  - `TongGiaTri` (`decimal(19, 4)`, NotNull).
  - `TrangThai` (`enum`: `TamTinh = 0`, `DangVanChuyen = 1`, `DaHoanThanh = 2`, `DaHuy = 3`).
  - `ButToanId` (`bigint`, Nullable, FK -> `ButToan`).
- **Line Item (`ChiTietDieuChuyenKho`)**:
  - `Id` (`bigint`, PK, Identity).
  - `PhieuDieuChuyenKhoId` (`bigint`, NotNull, FK -> `PhieuDieuChuyenKho`).
  - `VatTuHangHoaId` (`bigint`, NotNull, FK -> `VatTuHangHoa`).
  - `DonViTinh` (`nvarchar(50)`, NotNull).
  - `SoLuong` (`decimal(19, 4)`, NotNull).
  - `DonGiaVon` (`decimal(19, 4)`, NotNull).
  - `ThanhTien` (`decimal(19, 4)`, NotNull).
  - `TaiKhoanXuatId` (`bigint`, NotNull, FK -> `TaiKhoan` - VD: 1561).
  - `TaiKhoanNhapId` (`bigint`, NotNull, FK -> `TaiKhoan` - VD: 1561).

### 3.3. Entity `HangBanTraLai` & `ChiTietHangBanTraLai` (Sales Return)
- **Header (`HangBanTraLai`)**:
  - `Id` (`bigint`, PK, Identity).
  - `ChiNhanhId` (`bigint`, NotNull, FK -> `ChiNhanh`).
  - `SoChungTu` (`nvarchar(50)`, NotNull, Unique per Year) - VD: `HBTL-2026-0001`.
  - `NgayChungTu` (`DateTime`, NotNull).
  - `NgayHachToan` (`DateTime`, NotNull).
  - `KhachHangId` (`bigint`, NotNull, FK -> `DoiTuong`).
  - `HoaDonBanHangGocId` (`bigint`, Nullable, FK -> `HoaDonBanHang`).
  - `KhoNhapLaiId` (`bigint`, NotNull, FK -> `Kho`).
  - `TongTienGiamTruDoanhThu` (`decimal(19, 4)`, NotNull).
  - `TongTienThueGtgt` (`decimal(19, 4)`, NotNull).
  - `TongTienThanhToan` (`decimal(19, 4)`, NotNull).
  - `TongGiaTriNhapLaiKho` (`decimal(19, 4)`, NotNull) - Tổng giá vốn nhập lại (Nợ 1561 / Có 632).
  - `HinhThucXuLy` (`enum`: `GiamTruCongNo = 1`, `TraLaiTienMat = 2`, `TraLaiTienGui = 3`).
  - `ButToanGiamTruDoanhThuId` (`bigint`, Nullable, FK -> `ButToan`).
  - `ButToanNhapKhoGiaVonId` (`bigint`, Nullable, FK -> `ButToan`).
  - `TrangThai` (`enum`: `TamTinh = 0`, `DaGhiSo = 1`, `DaHuy = 2`).

### 3.4. Entity `HangMuaTraLai` (Purchase Return)
- Cấu trúc tương tự:
  - Header: `SoChungTu`, `NhaCungCapId`, `KhoXuatTraId`, `TongTienHangTra`, `TongThueGtgt`, `TongThanhToan`.
  - Bút toán sinh: Nợ TK 331 / Có TK 1561, Có TK 1331.

---

## 4. QUY TRÌNH NGHIỆP VỤ & THUẬT TOÁN CỐT LÕI (CORE INVARIANTS & ALGORITHMS)

### 4.1. Quy đổi Đơn vị tính tự động (UoM Auto-Calculation)
Khi người dùng nhập chứng từ và chọn `DonViTinhQuyDoiId`:
1. Lấy hệ số quy đổi: $k = \text{TyLeQuyDoi}$.
2. Số lượng cơ bản trên Thẻ kho:
   $$\text{SoLuongCoBan} = \begin{cases} \text{SoLuongGiaoDich} \times k & (\text{nếu PhepTinh} = \text{Nhan}) \\ \text{SoLuongGiaoDich} / k & (\text{nếu PhepTinh} = \text{Chia}) \end{cases}$$
3. Đơn giá cơ bản:
   $$\text{DonGiaCoBan} = \text{ThanhTien} / \text{SoLuongCoBan}$$
4. Mọi kiểm tra tồn kho (`ValidateStockAvailabilityAsync`) bắt buộc chạy trên `SoLuongCoBan` so với số dư cơ bản trong kho.

### 4.2. Quy trình Điều chuyển Kho Chống Xuất Âm (Warehouse Transfer Workflow)
```
[User nhập Phiếu Điều Chuyển Kho A -> Kho B]
                     |
                     v
[1. Kiểm tra Ngày Khóa Sổ: NgayHachToan > NgayKhoaSo]
                     |
                     v
[2. ValidateStockAvailabilityAsync tại Kho A]
   -> Nếu TonKhaDung(Kho A) < SoLuong: BẬT LỖI InvalidOperationException
                     |
                     v
[3. Xác định Đơn giá vốn tại Kho A theo BQGQ tức thời]
   -> DonGiaDieuChuyen = CalculateWeightedAverageCost(Kho A, VatTuId)
   -> ThanhTien = SoLuong * DonGiaDieuChuyen
                     |
                     v
[4. Ghi Sổ Kho kép]:
   -> Kho A: Giảm tồn kho SoLuong, Giảm giá trị ThanhTien
   -> Kho B: Tăng tồn kho SoLuong, Tăng giá trị ThanhTien
                     |
                     v
[5. Sinh Bút toán Sổ Cái PKT-DCK-YYYYMM]:
   -> Nợ TK 1561 (Chi tiết Kho B)
   -> Có TK 1561 (Chi tiết Kho A)
   -> Bất biến: TongNo == TongCo == ThanhTien
```

### 4.3. Bút toán Nghiệp vụ Giảm trừ Doanh thu & Hàng bán trả lại
1. **Hàng bán bị trả lại**:
   - Dòng bút toán giảm doanh thu:
     - Nợ TK 5212: Số tiền hàng trả lại chưa VAT.
     - Nợ TK 33311: Số tiền thuế GTGT tương ứng.
     - Có TK 131 (nếu giảm trừ nợ) hoặc Có TK 1111/1121 (nếu hoàn tiền mặt/tiền gửi).
   - Dòng bút toán giảm giá vốn:
     - Nợ TK 1561 (Kho nhập lại): $\text{Số lượng} \times \text{Đơn giá vốn ban đầu}$.
     - Có TK 632: $\text{Số lượng} \times \text{Đơn giá vốn ban đầu}$.
2. **Chiết khấu thương mại đạt doanh số (TK 5211)**:
   - Nợ TK 5211: Số tiền chiết khấu chưa VAT.
   - Nợ TK 33311: Số tiền thuế GTGT giảm.
   - Có TK 131: Giảm trừ trực tiếp vào công nợ của khách hàng.
3. **Quy trình Kết chuyển Cuối kỳ (Cập nhật `PeriodClosingService`)**:
   - **Step 0.5 (Trước khi kết chuyển sang TK 911)**:
     - Bút toán kết chuyển các khoản giảm trừ:
       $$\text{Nợ TK 511 (Doanh thu)} \quad / \quad \text{Có TK 5211, 5212, 5213}$$
     - Sau bước này, TK 5211 và 5212 có số dư bằng 0.
   - **Step 1 (Kết chuyển Doanh thu thuần sang 911)**:
     - Số tiền kết chuyển: $\text{Doanh thu thuần} = \text{Tổng Có 511} - \text{Tổng Nợ 511}$.
     - Hạch toán: Nợ TK 511 / Có TK 911.

---

## 5. ĐẶC TẢ GIAO DIỆN & TRẢI NGHIỆM NGƯỜI DÙNG (UI/UX SPECIFICATIONS)

### 5.1. Màn hình Phiếu Điều Chuyển Kho (`Views/PhieuDieuChuyenKho/Create.cshtml`)
```
+======================================================================================================+
| ninjaTax | PHIẾU XUẤT KHO KIÊM ĐIỀU CHUYỂN NỘI BỘ (MẪU NĐ 123/2020)                                  |
+======================================================================================================+
| Số phiếu:    [ DCK-2026-0012      ]   Ngày xuất điều chuyển: [ 20/03/2026 ]                          |
| Kho Xuất:    [ Kho Tổng Hà Nội ▼ ]   Kho Nhận:             [ Kho Chi Nhánh Hải Phòng ▼ ]            |
| Người VC:    [ Nguyễn Văn Vận     ]   Phương tiện vận chuyển:[ Xe tải 29C-123.45                   ] |
| Lệnh điều động số: [ LĐĐ-HN-045   ]   Lý do điều chuyển:    [ Bổ sung tồn kho kinh doanh Q2        ] |
+------------------------------------------------------------------------------------------------------+
| CHI TIẾT HÀNG HÓA ĐIỀU CHUYỂN: [ + Thêm Mặt Hàng ]                                                   |
+----+-----------+----------------+-------+----------+---------------+---------------+-----------------+
| STT| MÃ VẬT TƯ | TÊN HÀNG HÓA   | ĐVT   | SỐ LƯỢNG | TỒN KHO XUẤT  | ĐƠN GIÁ VỐN   | THÀNH TIỀN      |
+----+-----------+----------------+-------+----------+---------------+---------------+-----------------+
|  1 | HH-BIA-HN | Bia Hà Nội Lon | Thùng |       50 | 120 Thùng [V] |       240.000 |      12.000.000 |
|  2 | HH-PEPSI  | Nước ngọt Pep  | Két   |       30 |  45 Két   [V] |       180.000 |       5.400.000 |
+----+-----------+----------------+-------+----------+---------------+---------------+-----------------+
| CỘNG SỐ LƯỢNG: 80                   | TỔNG GIÁ TRỊ VỐN ĐIỀU CHUYỂN:         |      17.400.000 |
+------------------------------------------------------------------------------------------------------+
| ĐỊNH KHOẢN: Nợ TK 1561 (Kho Hải Phòng) / Có TK 1561 (Kho Hà Nội): 17.400.000 đ                      |
| [ Nút: IN PHIẾU VẬN CHUYỂN NỘI BỘ ]      [ Nút: LƯU & GHI SỔ ]      [ Nút: HỦY ]                    |
+======================================================================================================+
```

### 5.2. Màn hình Chứng từ Hàng Bán Trả Lại (`Views/HangBanTraLai/Create.cshtml`)
```
+======================================================================================================+
| ninjaTax | CHỨNG TỪ HÀNG BÁN BỊ TRẢ LẠI (GIẢM TRỪ DOANH THU & NHẬP LẠI KHO)                           |
+======================================================================================================+
| Số chứng từ: [ HBTL-2026-0003     ]   Ngày hạch toán: [ 22/03/2026 ]   Khách hàng: [ Cty TNHH Sao Mai]|
| Hóa đơn bán gốc: [ HĐ-2026-0045   ]   Kho nhập lại:   [ Kho Tổng Hà Nội ▼ ]                          |
| Hình thức xử lý: (X) Giảm trừ công nợ 131   ( ) Trả tiền mặt 111   ( ) Hoàn chuyển khoản 112         |
+------------------------------------------------------------------------------------------------------+
| CHI TIẾT HÀNG TRẢ LẠI:                                                                               |
+----+-----------+----------------+-------+----------+-------------+---------------+---------+---------+
| STT| MÃ HÀNG   | TÊN HÀNG HÓA   | ĐVT   | SỐ LƯỢNG | ĐƠN GIÁ BÁN | TIỀN HÀNG TRẢ | VAT (%) | TIỀN GV |
+----+-----------+----------------+-------+----------+-------------+---------------+---------+---------+
|  1 | HH-LAP-01 | Laptop Asus 15 | Chiếc |        2 |  15.000.000 |    30.000.000 |     10% |  24.000M|
+----+-----------+----------------+-------+----------+-------------+---------------+---------+---------+
| GIẢM DOANH THU: 30.000.000 đ | THUẾ GTGT GIẢM: 3.000.000 đ | TỔNG GIẢM CÔNG NỢ 131: 33.000.000 đ   |
| GIÁ VỐN NHẬP LẠI KHO (Nợ 1561 / Có 632): 24.000.000 đ                                                |
| [ Nút: LƯU & GHI SỔ KHO + SỔ CÁI ]                      [ Nút: ĐÓNG ]                                |
+======================================================================================================+
```

---

## 6. SERVICE LAYER CONTRACTS & TESTING MATRIX

### 6.1. Service Contracts
```csharp
public interface IDonViTinhQuyDoiService
{
    Task<List<DonViTinhQuyDoiViewModel>> GetByVatTuIdAsync(long vatTuId);
    Task<decimal> QuyDoiVeSoLuongCoBanAsync(long vatTuId, string donViTinhGiaoDich, decimal soLuongGiaoDich);
    Task<(bool ThanhCong, string? ThongBao)> TaoHoacCapNhatDvtQuyDoiAsync(DonViTinhQuyDoiEditViewModel model);
}

public interface IDieuChuyenKhoService
{
    Task<DieuChuyenKhoResult> TaoPhieuDieuChuyenAsync(PhieuDieuChuyenKhoCreateViewModel model);
    Task<(bool ThanhCong, string? ThongBao)> GhiSoDieuChuyenKhoAsync(long phieuDieuChuyenId);
    Task<(bool ThanhCong, string? ThongBao)> HuyGhiSoDieuChuyenKhoAsync(long phieuDieuChuyenId);
}

public interface IHangTraLaiService
{
    Task<HangBanTraLaiResult> TaoHangBanTraLaiAsync(HangBanTraLaiCreateViewModel model);
    Task<(bool ThanhCong, string? ThongBao)> GhiSoHangBanTraLaiAsync(long id);
    Task<HangMuaTraLaiResult> TaoHangMuaTraLaiAsync(HangMuaTraLaiCreateViewModel model);
    Task<(bool ThanhCong, string? ThongBao)> GhiSoHangMuaTraLaiAsync(long id);
}
```

### 6.2. Testing Matrix (Unit & Integration Tests)
1. **UT-UOM-01 (Multi-UoM Conversion Accuracy)**: Khai báo 1 Thùng = 24 Lon. Xuất bán 5 Thùng $\rightarrow$ Service tự động tính quy đổi ra 120 Lon cơ bản trên Thẻ kho.
2. **UT-TRANS-01 (Transfer Anti-Negative Stock)**: Kho A chỉ có 20 cái tồn kho. Lập phiếu điều chuyển 25 cái sang Kho B $\rightarrow$ Hệ thống throw `InvalidOperationException` chặn điều chuyển tức thì.
3. **UT-TRANS-02 (Transfer Ledger Neutrality)**: Điều chuyển 10 cái hàng hóa trị giá 100.000.000 đ từ Kho A sang Kho B. Tổng tài sản hàng tồn kho toàn công ty không thay đổi; Bút toán Nợ 1561 (Kho B) / Có 1561 (Kho A) cân đối `TongNo == TongCo == 100.000.000`.
4. **UT-RET-01 (Sales Return Double-Entry)**: Khách trả lại hàng giá bán 10.000.000 đ, VAT 10% (1.000.000 đ), giá vốn gốc 8.000.000 đ. Kiểm tra sinh đủ 2 bút toán: Nợ 5212 = 10tr, Nợ 33311 = 1tr / Có 131 = 11tr VÀ Nợ 1561 = 8tr / Có 632 = 8tr.
5. **UT-RET-02 (Period Closing with TK 521)**: Chạy kết chuyển cuối kỳ khi có số dư TK 5212 = 10tr. Đảm bảo TK 5212 được kết chuyển Nợ 511 / Có 5212 sạch số dư trước khi TK 511 kết chuyển sang TK 911.
