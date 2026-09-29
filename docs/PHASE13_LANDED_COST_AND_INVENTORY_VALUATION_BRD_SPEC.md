# TÀI LIỆU YÊU CẦU NGHIỆP VỤ & ĐẶC TẢ KỸ THUẬT (BRD & TECHNICAL SPECIFICATION)
## PHASE 13: PHÂN BỔ CHI PHÍ MUA HÀNG (LANDED COST), TÍNH LẠI GIÁ VỐN BÌNH QUÂN GIA QUYỀN CUỐI KỲ (VAS 02 & TT99) & TỰ ĐỘNG KHẤU TRỪ THUẾ GTGT (TK 133 vs 3331)

- **Dự án**: `ninjaTax` - Nền tảng Kế toán & Thuế Doanh nghiệp Tinh gọn
- **Phiên bản tài liệu**: 13.0-ENTERPRISE-TRADING
- **Tác giả**: Principal Software Engineer & Chief Accountant (20+ năm kinh nghiệm ERP & Kế toán Doanh nghiệp Việt Nam)
- **Cơ sở pháp lý & Chuẩn mực**: 
  - **VAS 02** (Chuẩn mực Kế toán số 02 - Hàng tồn kho ban hành theo QĐ 149/2001/QĐ-BTC).
  - **Thông tư số 99/2025/TT-BTC** (Bộ Tài chính ban hành 27/10/2025, hiệu lực 01/01/2026).
  - **Thông tư số 200/2014/TT-BTC** (Điều 23 - Hướng dẫn kế toán Hàng tồn kho & Điều 42 - Thuế GTGT).
  - **Nghị định số 123/2020/NĐ-CP** & **Thông tư số 78/2021/TT-BTC** về Hóa đơn chứng từ điện tử.
  - **Luật Thuế Giá trị gia tăng số 13/2008/QH12** và các văn bản hợp nhất, sửa đổi.
- **Trạng thái**: Approved for Implementation

---

## 1. BỐI CẢNH DOANH NGHIỆP & CĂN CỨ PHÁP LÝ (BUSINESS CONTEXT & REGULATORY BASIS)

### 1.1. Nghiệp vụ 1: Phân bổ Chi phí Mua hàng (Landed Cost Allocation - VAS 02 Đoạn 06-07)
1. **Thực trạng**: Khi mua hàng hóa, nguyên vật liệu (TK 152, 1561), giá mua trên hóa đơn của nhà cung cấp chỉ là một phần nguyên giá. Doanh nghiệp thường xuyên phát sinh chi phí vận chuyển, bốc dỡ, lưu kho bãi, bảo hiểm hàng hóa, thuế nhập khẩu không hoàn lại từ các nhà cung cấp dịch vụ độc lập.
2. **Yêu cầu Kế toán**:
   - Theo VAS 02 Đoạn 06-07: Giá gốc hàng tồn kho bao gồm chi phí mua, chi phí chế biến và các chi phí liên quan trực tiếp khác phát sinh để đưa hàng tồn kho về địa điểm và trạng thái hiện tại.
   - Khi có hóa đơn dịch vụ vận chuyển (Ví dụ: 10.000.000 đ cho 3 mặt hàng cùng về một chuyến xe), kế toán bắt buộc phải phân bổ chi phí này vào giá trị nhập kho của từng mặt hàng theo **Tiêu thức Giá trị** hoặc **Tiêu thức Số lượng**.
   - Định khoản:
     - Hóa đơn dịch vụ: $\text{Nợ TK 1561/152 (chi tiết từng mã)} \quad / \quad \text{Nợ TK 1331} \quad / \quad \text{Có TK 331/111/112}$.
3. **Rủi ro Thuế**: Nếu hạch toán thẳng chi phí vận chuyển hàng mua vào chi phí quản lý (TK 642) hoặc chi phí bán hàng (TK 641) trong kỳ mà hàng chưa xuất bán hết, doanh nghiệp sẽ bị Cơ quan Thuế bóc tách chi phí, truy thu thuế TNDN và phạt vi phạm hành chính về trốn thuế theo Nghị định 125/2020/NĐ-CP.

### 1.2. Nghiệp vụ 2: Tính lại Giá vốn Xuất kho Bình quân Gia quyền Cuối kỳ (Period-End Weighted Average Cost Recalculation - VAS 02 Đoạn 13)
1. **Thực trạng**: Hiện tại hệ thống `ninjaTax` chỉ tính giá xuất kho theo phương pháp bình quân tức thời tại thời điểm xuất (`CalculateWeightedAverageCostAsync`). Trong thực tế thương mại:
   - Hàng về trước, hóa đơn chứng từ về sau.
   - Doanh nghiệp xuất bán hàng hóa trong tháng, sau đó mới nhập thêm lô hàng giá rẻ/đắt hơn hoặc mới phân bổ chi phí vận chuyển vào phiếu nhập cũ.
2. **Yêu cầu Kế toán**:
   - Cuối kỳ kế toán (tháng/quý), kế toán chạy chức năng "Tính lại giá xuất kho".
   - Hệ thống tính lại một đơn giá bình quân cố định duy nhất cho cả kỳ theo công thức:
     $$\text{Đơn giá BQ cuối kỳ} = \frac{\text{Giá trị tồn đầu kỳ} + \sum \text{Giá trị nhập trong kỳ}}{\text{Số lượng tồn đầu kỳ} + \sum \text{Số lượng nhập trong kỳ}}$$
   - Tự động quét và cập nhật lại `DonGiaVon`, `TienGiaVon` trên toàn bộ Phiếu xuất kho (`PhieuXuatKho`, `ChiTietXuatKho`) trong kỳ và tự động điều chỉnh số tiền trên Bút toán Sổ Cái (`PKT-XK-` / Nợ 632 / Có 152, 1561).
   - Đảm bảo bất biến: Số lượng và giá trị tồn kho cuối kỳ không âm; tổng giá trị xuất trong kỳ khớp đúng với chênh lệch phát sinh trên Sổ Cái và Báo cáo Nhập - Xuất - Tồn (Mẫu S10-DN).

### 1.3. Nghiệp vụ 3: Tự động Khấu trừ Thuế GTGT Đầu vào vs Đầu ra (Automatic VAT Clearing - TT 200 Điều 42 & TT 99)
1. **Thực trạng**: Doanh nghiệp kê khai thuế GTGT theo phương pháp khấu trừ. Hàng tháng/quý:
   - Thuế GTGT đầu vào được khấu trừ tích lũy trên bên Nợ TK 1331.
   - Thuế GTGT đầu ra phải nộp tích lũy trên bên Có TK 33311.
2. **Yêu cầu Kế toán**:
   - Vào ngày cuối cùng của kỳ tính thuế (tháng hoặc quý), trước khi thực hiện kết chuyển doanh thu chi phí qua TK 911, kế toán phải thực hiện bù trừ khấu trừ giữa TK 1331 và TK 33311.
   - Công thức xác định số thuế khấu trừ:
     $$\text{Số thuế được khấu trừ trong kỳ} = \min(\text{Dư Nợ TK 1331 trước bù trừ}, \text{Dư Có TK 33311 trước bù trừ})$$
   - Hạch toán:
     $$\text{Nợ TK 33311 (Thuế GTGT đầu ra)} \quad / \quad \text{Có TK 1331 (Thuế GTGT đầu vào)}$$
   - Sau bù trừ:
     - Nếu $\text{Dư Nợ TK 1331} > \text{Dư Có TK 33311} \implies$ TK 33311 hết số dư, TK 1331 còn dư Nợ (chuyển kỳ sau khấu trừ tiếp hoặc xin hoàn thuế).
     - Nếu $\text{Dư Có TK 33311} > \text{Dư Nợ TK 1331} \implies$ TK 1331 hết số dư, TK 33311 còn dư Có (số thuế GTGT doanh nghiệp phải nộp vào Ngân sách Nhà nước).

---

## 2. KIẾN TRÚC PHÂN CHIA LÁT CẮT TRIỂN KHAI (SLICES ROADMAP: CORE TO EDGE)

```
+=======================================================================================================+
|                                    PHASE 13: CORE TO EDGE SLICE ARCHITECTURE                          |
+=======================================================================================================+
|                                                                                                       |
|  [SLICE 1: CORE DATA & LANDED COST ALLOCATION ENGINE]                                                 |
|  - Entity: ChiPhiMuaHang, ChiTietPhanBoChiPhi                                                        |
|  - Engine: LandedCostAllocationService (Phân bổ theo Giá trị / Số lượng)                              |
|  - Hạch toán tự động tăng nguyên giá lô nhập kho (Nợ 152/1561 / Có 331, 111, 112)                     |
|                                                                                                       |
|  [SLICE 2: PERIOD-END WEIGHTED AVERAGE INVENTORY COST RECALCULATION ENGINE]                            |
|  - Engine: PeriodEndInventoryCostService                                                              |
|  - Thuật toán quét toàn bộ NXT trong kỳ, tính ĐG BQGQ tháng                                          |
|  - Batch update ChiTietXuatKho, PhieuXuatKho, ChiTietButToan (PKT-XK- / Nợ 632 / Có 152, 1561)        |
|  - Báo cáo đối soát lệch giá vốn trước và sau khi tính lại                                            |
|                                                                                                       |
|  [SLICE 3: AUTOMATED VAT CLEARING ENGINE (TK 1331 vs TK 33311)]                                       |
|  - Engine: VatClearingService & Tích hợp vào Step 0 của PeriodClosingService                          |
|  - Tính min(Dư Nợ 1331, Dư Có 33311) -> Sinh chứng từ PKT-KT-THUE-YYYYMM                             |
|  - Kiểm tra điều kiện khóa sổ và tính chất không bù trừ gộp                                           |
|                                                                                                       |
|  [SLICE 4: UI/UX & MVC REPORTING SUITE]                                                               |
|  - View: Phân bổ chi phí mua hàng (gắn trực tiếp trên Phiếu Nhập Kho hoặc màn hình độc lập)           |
|  - View: Màn hình chạy "Tính lại giá vốn cuối kỳ" kèm ProgressBar & Bảng đối chiếu chênh lệch        |
|  - View: Màn hình "Khấu trừ thuế GTGT cuối kỳ" & Tích hợp cảnh báo tờ khai Mẫu 01/GTGT                |
|                                                                                                       |
+=======================================================================================================+
```

---

## 3. ĐẶC TẢ THỰC THỂ DỮ LIỆU & QUAN HỆ EF CORE (DATA CONTRACTS & SCHEMA)

### 3.1. Entity `ChiPhiMuaHang` (Landed Cost Header)
Lưu thông tin hóa đơn/chứng từ chi phí mua hàng cần phân bổ.
- `Id` (`bigint`, PK, Identity).
- `ChiNhanhId` (`bigint`, FK -> `ChiNhanh`).
- `SoChungTu` (`nvarchar(50)`, NotNull, Unique per Year) - VD: `CPMH-2026-0001`.
- `NgayChungTu` (`DateTime`, NotNull).
- `NgayHachToan` (`DateTime`, NotNull).
- `NhaCungCapDichVuId` (`bigint`, NotNull, FK -> `DoiTuong`).
- `SoHoaDonDichVu` (`nvarchar(50)`, Nullable) - Số hóa đơn GTGT cước vận tải/bốc dỡ.
- `NgayHoaDonDichVu` (`DateTime`, Nullable).
- `TongTienChiPhi` (`decimal(19, 4)`, NotNull) - Tổng chi phí dịch vụ chưa VAT.
- `ThueSuatVat` (`decimal(19, 4)`, default 10m).
- `TienThueVat` (`decimal(19, 4)`, NotNull).
- `TongThanhToan` (`decimal(19, 4)`, NotNull).
- `TaiKhoanChiPhiId` (`bigint`, NotNull, FK -> `TaiKhoan`) - Mặc định TK 1561 hoặc 152.
- `TaiKhoanCongNoId` (`bigint`, NotNull, FK -> `TaiKhoan`) - TK 331, 1111, 1121.
- `TaiKhoanThueVatId` (`bigint`, Nullable, FK -> `TaiKhoan`) - TK 1331.
- `TieuThucPhanBo` (`enum`: `TheoGiaTri = 1`, `TheoSoLuong = 2`).
- `TrangThai` (`enum`: `TamTinh = 0`, `DaPhanBo = 1`, `DaHuy = 2`).
- `DienGiai` (`nvarchar(500)`).
- `ButToanId` (`bigint`, Nullable, FK -> `ButToan`).
- `NgayTao` (`DateTime`, UtcNow), `NgayCapNhat` (`DateTime`, Nullable).

### 3.2. Entity `ChiTietPhanBoChiPhi` (Landed Cost Detail)
Lưu chi tiết chi phí phân bổ cho từng dòng của Phiếu Nhập Kho.
- `Id` (`bigint`, PK, Identity).
- `ChiPhiMuaHangId` (`bigint`, NotNull, FK -> `ChiPhiMuaHang`).
- `PhieuNhapKhoId` (`bigint`, NotNull, FK -> `PhieuNhapKho`).
- `ChiTietNhapKhoId` (`bigint`, NotNull, FK -> `ChiTietNhapKho`).
- `VatTuHangHoaId` (`bigint`, NotNull, FK -> `VatTuHangHoa`).
- `SoLuongNhap` (`decimal(19, 4)`, NotNull).
- `TienHangNhap` (`decimal(19, 4)`, NotNull).
- `HeSoPhanBo` (`decimal(19, 6)`, NotNull) - Tỷ lệ % phân bổ.
- `SoTienChiPhiPhanBo` (`decimal(19, 4)`, NotNull) - Số tiền chi phí phân bổ cho dòng này.
- `DonGiaSauPhanBo` (`decimal(19, 4)`, NotNull) - $(\text{TienHangNhap} + \text{SoTienChiPhiPhanBo}) / \text{SoLuongNhap}$.

### 3.3. Mở rộng Entity `ChiTietNhapKho`
- Bổ sung trường:
  - `ChiPhiMuaHangPhanBo` (`decimal(19, 4)`, default `0m`).
  - `TongGiaTriNhapKho` (`decimal(19, 4)`, computed hoặc persisted: `ThanhTien + ChiPhiMuaHangPhanBo`).

### 3.4. Entity `LichSuTinhGiaXuatKho` (Period-End Cost Recalculation Audit Log)
Lưu lịch sử các lần chạy tính lại giá vốn cuối kỳ.
- `Id` (`bigint`, PK, Identity).
- `ChiNhanhId` (`bigint`, FK -> `ChiNhanh`).
- `Nam` (`int`, NotNull).
- `Thang` (`int`, NotNull).
- `KhoId` (`bigint`, Nullable, FK -> `Kho`) - Null nếu chạy toàn bộ kho.
- `NgayThucHien` (`DateTime`, UtcNow).
- `NguoiThucHienId` (`string`, Nullable).
- `SoLuongPhieuCapNhat` (`int`, NotNull).
- `TongChenhLechGiaVon` (`decimal(19, 4)`, NotNull) - Chênh lệch tổng giá vốn xuất trước và sau khi tính lại.
- `GhiChu` (`nvarchar(1000)`).

### 3.5. Entity `ChungTuKhauTruThue` (VAT Clearance Record)
- `Id` (`bigint`, PK, Identity).
- `KyThue` (`nvarchar(20)`, NotNull) - VD: `2026-03` hoặc `2026-Q1`.
- `NgayChungTu` (`DateTime`, NotNull).
- `DuNo1331TruocKhauTru` (`decimal(19, 4)`, NotNull).
- `DuCo33311TruocKhauTru` (`decimal(19, 4)`, NotNull).
- `SoTienKhauTru` (`decimal(19, 4)`, NotNull).
- `DuNo1331ConLai` (`decimal(19, 4)`, NotNull).
- `DuCo33311ConLai` (`decimal(19, 4)`, NotNull).
- `ButToanId` (`bigint`, NotNull, FK -> `ButToan`).
- `NgayTao` (`DateTime`, UtcNow).

---

## 4. QUY TRÌNH NGHIỆP VỤ & THUẬT TOÁN CỐT LÕI (CORE INVARIANTS & ALGORITHMS)

### 4.1. Thuật toán Phân bổ Chi phí Mua hàng (Landed Cost Engine)
Khi phân bổ tổng chi phí $C$ cho $n$ dòng nhập kho:
1. **Phân bổ theo Giá trị hàng mua**:
   $$\text{Tỷ lệ } r_i = \frac{\text{TienHang}_i}{\sum_{k=1}^n \text{TienHang}_k}$$
   $$\text{ChiPhi}_i = \text{Round}\left(C \times r_i, 4\right)$$
2. **Phân bổ theo Số lượng hàng mua**:
   $$\text{Tỷ lệ } r_i = \frac{\text{SoLuong}_i}{\sum_{k=1}^n \text{SoLuong}_k}$$
   $$\text{ChiPhi}_i = \text{Round}\left(C \times r_i, 4\right)$$
3. **Bất biến Chống lệch số học (Rounding Discrepancy Invariant)**:
   Do làm tròn số thập phân, tổng $\sum_{i=1}^n \text{ChiPhi}_i$ có thể lệch $\pm 1$ đồng so với $C$. Thuật toán bắt buộc tính phần chênh lệch:
   $$\Delta = C - \sum_{i=1}^n \text{ChiPhi}_i$$
   và cộng dồn $\Delta$ vào dòng có giá trị chi phí phân bổ lớn nhất.
   $$\sum_{i=1}^n \text{ChiPhi}_i \equiv C \quad (\text{Tuyệt đối chính xác 100\%})$$
4. **Cập nhật Phiếu Nhập Kho**:
   - Ghi đè trường `ChiPhiMuaHangPhanBo` trên từng dòng `ChiTietNhapKho`.
   - Sinh bút toán chi phí:
     - Nợ TK 1561/152 (chi tiết từng vật tư): $\text{ChiPhi}_i$
     - Nợ TK 1331: Tiền thuế VAT dịch vụ
     - Có TK 331 / 1111 / 1121: Tổng tiền thanh toán cho nhà xe/vận tải.

### 4.2. Thuật toán Tính lại Giá vốn Bình quân Gia quyền Cuối kỳ
Quy trình thực hiện tuần tự theo tháng kế toán $M$ của năm $Y$:
```
                      +------------------------------------------+
                      | BẮT ĐẦU TÍNH LẠI GIÁ VỐN THÁNG Y-M        |
                      +------------------------------------------+
                                           |
                                           v
                      +------------------------------------------+
                      | 1. Kiểm tra Khóa Sổ: NgayKhoaSo < Y-M-01 |
                      +------------------------------------------+
                                           |
                                           v
                      +------------------------------------------+
                      | 2. Quét Tồn Đầu Kỳ (Số lượng, Giá trị)   |
                      |    theo từng Cặp (KhoId, VatTuId)        |
                      +------------------------------------------+
                                           |
                                           v
                      +------------------------------------------+
                      | 3. Quét Tổng Nhập Kho Đã Ghi Sổ trong    |
                      |    kỳ (bao gồm chi phí mua hàng phân bổ) |
                      +------------------------------------------+
                                           |
                                           v
                      +------------------------------------------+
                      | 4. Tính Đơn Giá Bình Quân Cố Định:       |
                      |    ĐG = (GT_Dau + GT_Nhap) /             |
                      |         (SL_Dau + SL_Nhap)               |
                      +------------------------------------------+
                                           |
                                           v
                      +------------------------------------------+
                      | 5. Cập nhật Đơn giá vốn & Tiền giá vốn   |
                      |    cho TOÀN BỘ Phiếu Xuất Kho trong tháng|
                      +------------------------------------------+
                                           |
                                           v
                      +------------------------------------------+
                      | 6. Đồng bộ UPDATE ChiTietButToan của     |
                      |    tất cả Bút toán Sổ Cái: PKT-XK-       |
                      |    Nợ TK 632 / Có TK 152, 1561           |
                      +------------------------------------------+
                                           |
                                           v
                      +------------------------------------------+
                      | 7. Kiểm tra An toàn: Tồn Cuối Kỳ >= 0    |
                      |    Tổng Nợ 632 == Tổng Có 1561           |
                      +------------------------------------------+
```
- **Xử lý đặc biệt nếu $SL_{dau} + SL_{nhap} == 0$**: Nếu không có phát sinh nhập và không có tồn đầu mà lại có chứng từ xuất (trong trường hợp dữ liệu cũ lỗi), giữ nguyên đơn giá tạm tính và ghi log cảnh báo.

### 4.3. Thuật toán Tự động Khấu trừ Thuế GTGT (VAT Clearance)
1. **Kiểm tra kỳ khóa sổ**: Đảm bảo ngày cuối tháng chưa bị khóa sổ.
2. **Tính toán số dư**:
   - `DuNo1331` = Phát sinh Nợ lũy kế TK 1331 đến cuối kỳ trừ đi phát sinh Có lũy kế TK 1331 (không tính các bút toán khấu trừ đã chạy của kỳ này).
   - `DuCo33311` = Phát sinh Có lũy kế TK 33311 đến cuối kỳ trừ đi phát sinh Nợ lũy kế TK 33311.
3. **Số tiền khấu trừ**:
   $$S = \max(0, \min(DuNo1331, DuCo33311))$$
4. **Nếu $S > 0$**:
   - Sinh bút toán Sổ Cái: Mã chứng từ `PKT-KT-THUE-YYYYMM`.
   - Dòng 1: Nợ TK 33311, số tiền $S$.
   - Dòng 2: Có TK 1331, số tiền $S$.
   - Bất biến: `TongNo == TongCo == S`.
5. **Gắn vào Quy trình Đóng sổ Kỳ kế toán (`PeriodClosingService`)**:
   - `PeriodClosingService` gọi `VatClearingService` chạy tại **Step 0**.
   - Sau khi hoàn thành khấu trừ thuế GTGT mới tiến hành Step 1 (Kết chuyển Doanh thu) và Step 2 (Kết chuyển Chi phí) vào TK 911.

---

## 5. ĐẶC TẢ GIAO DIỆN & TRẢI NGHIỆM NGƯỜI DÙNG (UI/UX SPECIFICATIONS)

### 5.1. Màn hình Phân bổ Chi phí Mua hàng (`Views/ChiPhiMuaHang/Create.cshtml`)
```
+======================================================================================================+
| ninjaTax | CHỨNG TỪ PHÂN BỔ CHI PHÍ MUA HÀNG (LANDED COST)                                          |
+======================================================================================================+
| Số chứng từ: [ CPMH-2026-0005     ]   Ngày hạch toán: [ 15/03/2026 ]   Nhà xe/NCC: [ Cty Vận Tải ABC] |
| Hóa đơn số:  [ 0004521            ]   Ngày hóa đơn:   [ 15/03/2026 ]   Mã số thuế: [ 0108999888     ] |
| Tổng chi phí:[ 10.000.000 đ       ]   Thuế GTGT (10%):[ 1.000.000 đ]   Tổng TT:    [ 11.000.000 đ   ] |
| Tiêu thức PB: (X) Theo Giá trị hàng nhập     ( ) Theo Số lượng hàng nhập                             |
+------------------------------------------------------------------------------------------------------+
| DANH SÁCH PHIẾU NHẬP KHO ĐƯỢC PHÂN BỔ: [ + Chọn Phiếu Nhập Kho ]                                     |
+----+------------+-----------+----------------+----------+---------------+--------------+-------------+
| STT| SỐ PHIẾU NK| MÃ VẬT TƯ | TÊN HÀNG HÓA   | SỐ LƯỢNG | TIỀN HÀNG     | TIỀN CP PHÂN | ĐƠN GIÁ MỚI |
+----+------------+-----------+----------------+----------+---------------+--------------+-------------+
|  1 | PNK-2026-01| HH-DELL-01| Laptop Dell 15 |       10 |   150.000.000 |    6.000.000 |  15.600.000 |
|  2 | PNK-2026-01| HH-HP-02  | Laptop HP 14   |       10 |   100.000.000 |    4.000.000 |  10.400.000 |
+----+------------+-----------+----------------+----------+---------------+--------------+-------------+
| CỘNG:                                        |       20 |   250.000.000 |   10.000.000 | [ CÂN ĐỐI ] |
+------------------------------------------------------------------------------------------------------+
| [ Nợ 1561 / Có 331: 10.000.000 đ ]   [ Nợ 1331 / Có 331: 1.000.000 đ ]                                |
| [ Nút: TÍNH PHÂN BỔ ]       [ Nút: LƯU & GHI SỔ KHO & SỔ CÁI ]       [ Nút: ĐÓNG / HỦY ]             |
+======================================================================================================+
```

### 5.2. Màn hình Tính lại Giá vốn Cuối kỳ (`Views/Inventory/RecalculateCost.cshtml`)
```
+======================================================================================================+
| ninjaTax | TÍNH LẠI GIÁ XUẤT KHO BÌNH QUÂN GIA QUYỀN CUỐI KỲ (VAS 02)                                |
+======================================================================================================+
| Kỳ kế toán: Năm [ 2026 ]  Tháng [ Tháng 03 ▼ ]       Kho hàng: [ Tất cả các kho               ▼ ]    |
| Tùy chọn:   [X] Tự động cập nhật lại Bút toán Sổ Cái (PKT-XK-)                                       |
|             [X] Kiểm tra và chặn nếu phát hiện tồn kho âm                                            |
|                                                                                                      |
| [ Nút: BẮT ĐẦU CHẠY TÍNH GIÁ VỐN ]                                                                   |
| TIẾN ĐỘ: [==================================================] 100% (Hoàn thành)                      |
+------------------------------------------------------------------------------------------------------+
| KẾT QUẢ ĐỐI SOÁT GIÁ VỐN TRONG THÁNG 03/2026:                                                        |
+----+-----------+----------------+----------+---------------+---------------+---------------+---------+
| STT| MÃ VẬT TƯ | TÊN VẬT TƯ     | SỐ LƯỢNG | GIÁ VỐN CŨ    | GIÁ VỐN MỚI   | CHÊNH LỆCH    | TRẠNG T |
+----+-----------+----------------+----------+---------------+---------------+---------------+---------+
|  1 | HH-DELL-01| Laptop Dell 15 |        8 |   120.000.000 |   124.800.000 |    +4.800.000 | ĐÃ CẬP  |
|  2 | HH-HP-02  | Laptop HP 14   |        5 |    50.000.000 |    52.000.000 |    +2.000.000 | ĐÃ CẬP  |
+----+-----------+----------------+----------+---------------+---------------+---------------+---------+
| TỔNG GIÁ VỐN ĐIỀU CHỈNH TRÊN TK 632:                                       |    +6.800.000 | KHỚP SỔ |
+======================================================================================================+
```

---

## 6. SERVICE LAYER CONTRACTS & TESTING MATRIX

### 6.1. Service Contracts
```csharp
public interface ILandedCostAllocationService
{
    Task<ChiPhiMuaHangResult> TaoChungTuChiPhiAsync(ChiPhiMuaHangCreateViewModel model);
    Task<(bool ThanhCong, string? ThongBao)> PhanBoChiPhiAsync(long chiPhiMuaHangId, TieuThucPhanBo tieuThuc);
    Task<(bool ThanhCong, string? ThongBao)> GhiSoChiPhiMuaHangAsync(long chiPhiMuaHangId);
    Task<(bool ThanhCong, string? ThongBao)> HuyGhiSoChiPhiMuaHangAsync(long chiPhiMuaHangId);
}

public interface IPeriodEndInventoryCostService
{
    Task<RecalculateCostResult> TinhLaiGiaXuatKhoBqgqAsync(int nam, int thang, long? khoId = null);
    Task<List<ChenhLechGiaVonItem>> XemTruocBienDongGiaVonAsync(int nam, int thang, long? khoId = null);
}

public interface IVatClearingService
{
    Task<VatClearingResult> ThucHienKhauTruThueGtgtAsync(int nam, int thang);
    Task<VatClearingPreviewModel> XemTruocSoLieuKhauTruAsync(int nam, int thang);
    Task<(bool ThanhCong, string? ThongBao)> HuyKhauTruThueGtgtAsync(int nam, int thang);
}
```

### 6.2. Testing Matrix (Unit & Integration Tests)
1. **UT-LC-01 (Landed Cost Precision)**: Phân bổ 1.000.000 đ cho 3 mặt hàng trị giá 300.000, 300.000, 400.000. Đảm bảo tổng số tiền phân bổ cộng lại bằng chính xác 1.000.000 đ (triệt tiêu sai số làm tròn).
2. **UT-LC-02 (Book Lock on Landed Cost)**: Cấm phân bổ hoặc sửa chi phí mua hàng nếu ngày hạch toán nhỏ hơn hoặc bằng `NgayKhoaSo`.
3. **UT-PE-01 (Period-End Weighted Average Formula)**: Tồn đầu 10 cái giá 100k, nhập ngày 05/03 10 cái giá 120k, xuất ngày 10/03 5 cái (tạm tính), nhập ngày 20/03 10 cái giá 140k. Chạy tính lại giá vốn cuối kỳ: Đơn giá bình quân = $(1000k + 1200k + 1400k) / 30 = 120k$. Phiếu xuất ngày 10/03 được cập nhật giá vốn = $5 \times 120k = 600k$.
4. **UT-PE-02 (GL Synchronization)**: Sau khi chạy tính lại giá vốn, toàn bộ `ChiTietButToan` của `PKT-XK-` phải có số tiền khớp từng đồng với `PhieuXuatKho.TongTienGiaVon`.
5. **UT-VAT-01 (VAT Clearing Normal)**: Dư Nợ 1331 = 50.000.000 đ, Dư Có 33311 = 70.000.000 đ. Kết quả khấu trừ: Bút toán Nợ 33311 / Có 1331 = 50.000.000 đ. Sau khấu trừ, Dư Nợ 1331 = 0, Dư Có 33311 = 20.000.000 đ.
6. **UT-VAT-02 (VAT Clearing Invariant)**: Bút toán khấu trừ thuế GTGT luôn bảo đảm `TongNo == TongCo`.
