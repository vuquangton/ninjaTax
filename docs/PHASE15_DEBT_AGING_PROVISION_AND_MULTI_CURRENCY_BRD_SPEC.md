# TÀI LIỆU YÊU CẦU NGHIỆP VỤ & ĐẶC TẢ KỸ THUẬT (BRD & TECHNICAL SPECIFICATION)
## PHASE 15: PHÂN TÍCH TUỔI NỢ AR/AP, TRÍCH LẬP DỰ PHÒNG NỢ PHẢI THU KHÓ ĐÒI (THÔNG TƯ 48/2019/TT-BTC) & CƠ CHẾ ĐA NGUYÊN TỆ (VAS 10 & TK 413)

- **Dự án**: `ninjaTax` - Nền tảng Kế toán & Thuế Doanh nghiệp Tinh gọn
- **Phiên bản tài liệu**: 15.0-FINANCIAL-GOVERNANCE
- **Tác giả**: Principal Software Engineer & Chief Accountant (20+ năm kinh nghiệm ERP & Kế toán Doanh nghiệp Việt Nam)
- **Cơ sở pháp lý & Chuẩn mực**:
  - **Thông tư số 48/2019/TT-BTC** (Bộ Tài chính ban hành 08/08/2019 hướng dẫn trích lập và xử lý các khoản dự phòng giảm giá hàng tồn kho, tổn thất các khoản đầu tư, nợ phải thu khó đòi và bảo hành sản phẩm).
  - **Thông tư số 24/2022/TT-BTC** sửa đổi, bổ sung một số điều của Thông tư 48/2019/TT-BTC.
  - **VAS 10** (Chuẩn mực Kế toán số 10 - Ảnh hưởng của việc thay đổi tỷ giá hối đoái).
  - **Thông tư số 99/2025/TT-BTC** & **Thông tư số 200/2014/TT-BTC** (Điều 69 - Kế toán chênh lệch tỷ giá hối đoái TK 413 & TK 2293 Dự phòng nợ phải thu khó đòi).
  - **Luật Quản lý thuế số 38/2019/QH14**.
- **Trạng thái**: Approved for Implementation

---

## 1. BỐI CẢNH DOANH NGHIỆP & CĂN CỨ PHÁP LÝ (BUSINESS CONTEXT & REGULATORY BASIS)

### 1.1. Nghiệp vụ 1: Báo cáo Phân tích Tuổi nợ Đa dải (AR/AP Aging Analysis)
1. **Thực trạng**: Quản trị dòng tiền là mạch máu của doanh nghiệp. Việc chỉ nhìn vào tổng số dư nợ trên TK 131 (Phải thu khách hàng) hay TK 331 (Phải trả người bán) không thể phát hiện các khoản nợ đọng lâu ngày có nguy cơ mất vốn hoặc các khoản nợ sắp đến hạn thanh toán gây áp lực thanh khoản.
2. **Yêu cầu Kế toán & Quản trị**:
   - Phân tích chi tiết tuổi nợ theo từng hóa đơn bán hàng (`HoaDonBanHang`) và hóa đơn mua hàng (`HoaDonMuaHang`) dựa trên `HanThanhToan`.
   - Các dải tuổi nợ chuẩn ERP:
     - Trong hạn (Current / Chưa đến hạn thanh toán).
     - Quá hạn từ 1 đến 30 ngày.
     - Quá hạn từ 31 đến 60 ngày.
     - Quá hạn từ 61 đến 90 ngày.
     - Quá hạn từ 91 đến 180 ngày (từ 3 tháng đến dưới 6 tháng).
     - Quá hạn từ 181 đến 360 ngày (từ 6 tháng đến dưới 1 năm).
     - Quá hạn từ 1 đến 2 năm.
     - Quá hạn từ 2 đến 3 năm.
     - Quá hạn trên 3 năm.
   - Hỗ trợ xem báo cáo tuổi nợ tại bất kỳ mốc thời gian nào trong quá khứ (Historical Point-in-time Aging).

### 1.2. Nghiệp vụ 2: Trích lập Dự phòng Nợ phải thu khó đòi theo Thông tư 48/2019/TT-BTC
1. **Khung Pháp lý & Mức Trích lập Bắt buộc**:
   Theo Khoản 2 Điều 6 Thông tư 48/2019/TT-BTC:
   - **Quá hạn từ 06 tháng đến dưới 01 năm**: Trích lập **30%** giá trị khoản nợ.
   - **Quá hạn từ 01 năm đến dưới 02 năm**: Trích lập **50%** giá trị khoản nợ.
   - **Quá hạn từ 02 năm đến dưới 03 năm**: Trích lập **70%** giá trị khoản nợ.
   - **Quá hạn từ 03 năm trở lên**: Trích lập **100%** giá trị khoản nợ.
   - *Đối với doanh nghiệp viễn thông/bán lẻ*: Có quy định riêng cho nợ cước viễn thông (quá hạn 3 tháng: 30%, 6 tháng: 50%...).
   - *Trường hợp đặc biệt*: Con nợ bị phá sản, đang làm thủ tục giải thể, mất tích hoặc bỏ trốn $\rightarrow$ Được phép trích lập tối đa **100%** ngay cả khi chưa quá hạn 3 năm (kèm tài liệu chứng minh).
2. **Quy định Hạch toán Dự phòng (TK 2293 & TK 6426)**:
   - Trích lập lần đầu hoặc trích lập bổ sung khi rủi ro nợ xấu tăng:
     $$\text{Nợ TK 642 (Chi phí quản lý doanh nghiệp - tiểu khoản 6426)} \quad / \quad \text{Có TK 2293 (Dự phòng phải thu khó đòi)}$$
   - Hoàn nhập dự phòng khi khách hàng thanh toán hoặc rủi ro nợ xấu giảm:
     $$\text{Nợ TK 2293} \quad / \quad \text{Có TK 642 (hoặc 6426 - ghi giảm chi phí QLDN)}$$
   - Xử lý xóa nợ không có khả năng thu hồi (khi có biên bản xử lý nợ):
     $$\text{Nợ TK 2293 (Số đã trích lập)} \quad / \quad \text{Nợ TK 642 (Phần tổn thất chưa lập dự phòng)} \quad / \quad \text{Có TK 131}$$
   - Theo dõi ngoài Bảng cân đối kế toán: Nợ đã xử lý xóa sổ theo dõi trên Sổ theo dõi nợ khó đòi đã xử lý (trước đây là TK 004 ngoài bảng).

### 1.3. Nghiệp vụ 3: Động cơ Đa Nguyên tệ & Đánh giá lại Tỷ giá Hối đoái (VAS 10 & TK 413)
1. **Khung Chuẩn mực VAS 10 & TT 200 / TT 99**:
   - Đơn vị tiền tệ kế toán là **VND**. Mọi giao dịch phát sinh bằng ngoại tệ (USD, EUR, JPY, CNY...) phải được quy đổi ra VND theo **Tỷ giá giao dịch thực tế**.
2. **Nguyên tắc Tỷ giá Thực tế**:
   - Khi ghi nhận nợ phải thu (Bán hàng): Tỷ giá mua của ngân hàng thương mại nơi chỉ định khách thanh toán.
   - Khi ghi nhận nợ phải trả (Mua hàng): Tỷ giá bán của ngân hàng thương mại nơi dự kiến thanh toán.
   - Khi thanh toán công nợ:
     - Xuất quỹ ngoại tệ: Ghi sổ theo tỷ giá bình quân gia quyền di động hoặc tỷ giá đích danh.
     - Chênh lệch giữa tỷ giá ghi sổ và tỷ giá thực tế hạch toán vào:
       - Lãi tỷ giá: **Có TK 515 (Doanh thu hoạt động tài chính)**.
       - Lỗ tỷ giá: **Nợ TK 635 (Chi phí tài chính)**.
3. **Đánh giá lại Số dư Ngoại tệ Cuối kỳ Kế toán (Period-End FX Revaluation - TK 413)**:
   - Vào ngày lập Báo cáo tài chính (cuối năm hoặc cuối quý), doanh nghiệp bắt buộc phải đánh giá lại toàn bộ các khoản mục tiền tệ có gốc ngoại tệ:
     - Tiền mặt, tiền gửi ngoại tệ (TK 1112, 1122): Đánh giá theo tỷ giá mua của ngân hàng thương mại nơi mở tài khoản.
     - Nợ phải thu ngoại tệ (TK 131, 138): Đánh giá theo tỷ giá mua.
     - Nợ phải trả ngoại tệ (TK 331, 338): Đánh giá theo tỷ giá bán.
   - **Bút toán Đánh giá lại qua TK 4131 (Chênh lệch tỷ giá hối đoái đánh giá lại cuối kỳ)**:
     - Lãi tỷ giá: $\text{Nợ TK 1112, 1122, 131...} \quad / \quad \text{Có TK 4131}$.
     - Lỗ tỷ giá: $\text{Nợ TK 4131} \quad / \quad \text{Có TK 331, 1112, 1122...}$.
   - **Xử lý Kết chuyển TK 4131 tại thời điểm lập Báo cáo Tài chính**:
     - Kết chuyển toàn bộ số dư TK 4131 vào Doanh thu tài chính (nếu lãi ròng): $\text{Nợ TK 4131} / \text{Có TK 515}$.
     - Kết chuyển toàn bộ số dư TK 4131 vào Chi phí tài chính (nếu lỗ ròng): $\text{Nợ TK 635} / \text{Có TK 4131}$.
     - **Bất biến**: Tài khoản 4131 phải có số dư bằng 0 trên Báo cáo tài chính năm.

---

## 2. KIẾN TRÚC PHÂN CHIA LÁT CẮT TRIỂN KHAI (SLICES ROADMAP: CORE TO EDGE)

```
+=======================================================================================================+
|                                    PHASE 15: CORE TO EDGE SLICE ARCHITECTURE                          |
+=======================================================================================================+
|                                                                                                       |
|  [SLICE 1: GRANULAR AR/AP AGING ANALYSIS ENGINE]                                                      |
|  - Nâng cấp CongNoService: Mở rộng 9 dải tuổi nợ chuẩn Thông tư 48 (0-30, 31-60, 61-90, 91-180,       |
|    181-360, 1-2 năm, 2-3 năm, >3 năm)                                                                 |
|  - Engine tính tuổi nợ lịch sử (Historical as-of-date calculation) theo từng hóa đơn                  |
|                                                                                                       |
|  [SLICE 2: TT48 BAD DEBT PROVISIONING ENGINE]                                                         |
|  - Entity: BangTrichLapDuPhongNoPhaiThu, ChiTietTrichLapDuPhong                                       |
|  - Engine: BadDebtProvisionService                                                                    |
|  - Tự động áp tỷ lệ 30%, 50%, 70%, 100% dựa trên số ngày quá hạn                                      |
|  - Tự động tính chênh lệch tăng/giảm dự phòng so với số dư lũy kế TK 2293                            |
|  - Hạch toán tự động: Nợ 642 / Có 2293 (Trích lập) hoặc Nợ 2293 / Có 642 (Hoàn nhập)                  |
|                                                                                                       |
|  [SLICE 3: MULTI-CURRENCY & VAS 10 REAL-TIME TRANSACTION ENGINE]                                      |
|  - Nâng cấp ExchangeRateHistory: Tỷ giá mua, Tỷ giá bán, Tỷ giá trung tâm của từng ngân hàng         |
|  - Cho phép chọn Tiền tệ (USD, EUR...) trên Hóa đơn, Thu/Chi, Nhập/Xuất kho                           |
|  - Tự động ghi nhận Lãi tỷ giá (Có 515) hoặc Lỗ tỷ giá (Nợ 635) khi thanh toán                        |
|                                                                                                       |
|  [SLICE 4: PERIOD-END FX REVALUATION ENGINE (TK 413)]                                                 |
|  - Engine: FxRevaluationService                                                                       |
|  - Quét toàn bộ số dư ngoại tệ trên TK 1112, 1122, 131, 331 cuối kỳ                                   |
|  - Sinh bút toán đánh giá lại qua TK 4131 và kết chuyển sạch số dư 4131 sang 515/635                   |
|                                                                                                       |
|  [SLICE 5: REPORTING & UI/UX INTERACTIVE DASHBOARDS]                                                  |
|  - Màn hình Báo cáo Tuổi nợ tương tác (Interactive Aging Matrix kèm drill-down từng Hóa đơn)        |
|  - Màn hình Bảng Trích lập Dự phòng nợ phải thu khó đòi theo Mẫu Thông tư 48/2019/TT-BTC              |
|  - Màn hình Đánh giá lại chênh lệch tỷ giá cuối kỳ (FX Revaluation Wizard)                            |
|                                                                                                       |
+=======================================================================================================+
```

---

## 3. ĐẶC TẢ THỰC THỂ DỮ LIỆU & QUAN HỆ EF CORE (DATA CONTRACTS & SCHEMA)

### 3.1. Entity `BangTrichLapDuPhong` & `ChiTietTrichLapDuPhong` (TT 48 Bad Debt Provision)
- **Header (`BangTrichLapDuPhong`)**:
  - `Id` (`bigint`, PK, Identity).
  - `ChiNhanhId` (`bigint`, NotNull, FK -> `ChiNhanh`).
  - `SoChungTu` (`nvarchar(50)`, NotNull, Unique per Year) - VD: `DPNT-2026-0001`.
  - `NgayLap` (`DateTime`, NotNull).
  - `NgayHachToan` (`DateTime`, NotNull) - Thường là ngày 31/12 hoặc cuối quý.
  - `TongNoQuaHan` (`decimal(19, 4)`, NotNull).
  - `TongSoDuPhongPhaiTrich` (`decimal(19, 4)`, NotNull).
  - `SoDuDuPhongHienTai2293` (`decimal(19, 4)`, NotNull) - Số dư Có hiện có trên TK 2293 trước khi trích.
  - `SoTienTrichThem` (`decimal(19, 4)`, default `0m`) - Nếu PhảiTrích > HiệnCó.
  - `SoTienHoanNhap` (`decimal(19, 4)`, default `0m`) - Nếu HiệnCó > PhảiTrích.
  - `TrangThai` (`enum`: `TamTinh = 0`, `DaGhiSo = 1`, `DaHuy = 2`).
  - `ButToanId` (`bigint`, Nullable, FK -> `ButToan`).
- **Line Item (`ChiTietTrichLapDuPhong`)**:
  - `Id` (`bigint`, PK, Identity).
  - `BangTrichLapDuPhongId` (`bigint`, NotNull, FK -> `BangTrichLapDuPhong`).
  - `KhachHangId` (`bigint`, NotNull, FK -> `DoiTuong`).
  - `HoaDonBanHangId` (`bigint`, Nullable, FK -> `HoaDonBanHang`).
  - `SoHoaDon` (`nvarchar(50)`).
  - `NgayHoaDon` (`DateTime`).
  - `HanThanhToan` (`DateTime`).
  - `SoTienConNo` (`decimal(19, 4)`, NotNull).
  - `SoNgayQuaHan` (`int`, NotNull).
  - `TyLeTrichLap` (`decimal(5, 2)`, NotNull) - `30.00`, `50.00`, `70.00`, `100.00`.
  - `SoTienDuPhong` (`decimal(19, 4)`, NotNull) - $\text{SoTienConNo} \times \text{TyLeTrichLap} / 100$.
  - `LyDoDacBiet` (`nvarchar(255)`, Nullable) - Ví dụ: "Doanh nghiệp phá sản theo Quyết định tòa án số X".

### 3.2. Mở rộng `ExchangeRateHistory` & Hỗ trợ Đa Tệ
```csharp
public class ExchangeRateHistory
{
    public long Id { get; set; }
    public string FromCurrency { get; set; } = "USD"; // ISO 4217 code
    public string ToCurrency { get; set; } = "VND";
    public decimal TyGiaMua { get; set; }              // Tỷ giá mua chuyển khoản của NHTM
    public decimal TyGiaBan { get; set; }              // Tỷ giá bán chuyển khoản của NHTM
    public decimal TyGiaTrungTam { get; set; }          // Tỷ giá trung tâm NHNN
    public long? NganHangId { get; set; }              // Ngân hàng tham chiếu (VD: VCB, TCB)
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
```

### 3.3. Entity `DanhGiaLaiNgoaiTe` & `ChiTietDanhGiaLaiNgoaiTe` (Period-End FX Revaluation Header & Lines)
- **Header (`DanhGiaLaiNgoaiTe`)**:
  - `Id` (`bigint`, PK, Identity).
  - `ChiNhanhId` (`bigint`, NotNull, FK -> `ChiNhanh`).
  - `SoChungTu` (`nvarchar(50)`, NotNull, Unique per Year) - VD: `DGLTG-2026-12`.
  - `NgayChungTu` (`DateTime`, NotNull).
  - `LoaiTien` (`nvarchar(10)`, NotNull) - VD: `USD`.
  - `TyGiaDanhGiaLai` (`decimal(19, 4)`, NotNull).
  - `TongLaiTyGia` (`decimal(19, 4)`, default `0m`).
  - `TongLoTyGia` (`decimal(19, 4)`, default `0m`).
  - `ChenhLechThuan` (`decimal(19, 4)`, NotNull) - `TongLaiTyGia - TongLoTyGia`.
  - `ButToanDanhGiaLaiId` (`bigint`, Nullable, FK -> `ButToan`).
  - `ButToanKetChuyen413Id` (`bigint`, Nullable, FK -> `ButToan`).
- **Line Item (`ChiTietDanhGiaLaiNgoaiTe`)**:
  - `Id` (`bigint`, PK, Identity).
  - `DanhGiaLaiNgoaiTeId` (`bigint`, NotNull, FK -> `DanhGiaLaiNgoaiTe`).
  - `TaiKhoanId` (`bigint`, NotNull, FK -> `TaiKhoan`) - TK 1112, 1122, 131, 331.
  - `DoiTuongId` (`bigint`, Nullable, FK -> `DoiTuong`).
  - `SoDuNgoaiTe` (`decimal(19, 4)`, NotNull).
  - `TyGiaGhiSo` (`decimal(19, 4)`, NotNull).
  - `GiaTriGhiSoVnd` (`decimal(19, 4)`, NotNull).
  - `GiaTriDanhGiaLaiVnd` (`decimal(19, 4)`, NotNull) - $\text{SoDuNgoaiTe} \times \text{TyGiaDanhGiaLai}$.
  - `ChenhLechVnd` (`decimal(19, 4)`, NotNull) - $\text{GiaTriDanhGiaLaiVnd} - \text{GiaTriGhiSoVnd}$.
  - `LoaiChenhLech` (`enum`: `Lai = 1`, `Lo = 2`).

---

## 4. QUY TRÌNH NGHIỆP VỤ & THUẬT TOÁN CỐT LÕI (CORE INVARIANTS & ALGORITHMS)

### 4.1. Thuật toán Phân loại Tuổi nợ & Trích lập Dự phòng Thông tư 48
Đối với từng khoản nợ phải thu của khách hàng $i$ tại mốc thời gian $T$:
1. Xác định số ngày quá hạn:
   $$\Delta D = \text{DateDiff}(T, \text{HanThanhToan})$$
2. Xác định Tỷ lệ Dự phòng $P(\Delta D)$ theo Thông tư 48:
   $$P(\Delta D) = \begin{cases} 
   0\% & \text{nếu } \Delta D < 180 \text{ ngày (dưới 6 tháng)} \\ 
   30\% & \text{nếu } 180 \le \Delta D < 365 \text{ ngày (từ 6 tháng đến dưới 1 năm)} \\ 
   50\% & \text{nếu } 365 \le \Delta D < 730 \text{ ngày (từ 1 năm đến dưới 2 năm)} \\ 
   70\% & \text{nếu } 730 \le \Delta D < 1095 \text{ ngày (từ 2 năm đến dưới 3 năm)} \\ 
   100\% & \text{nếu } \Delta D \ge 1095 \text{ ngày (từ 3 năm trở lên)} 
   \end{cases}$$
3. Tính số dự phòng cần có của từng khoản nợ:
   $$DP_i = \text{Round}\left(\text{ConPhaiThu}_i \times P(\Delta D), 4\right)$$
4. Tổng mức dự phòng cần duy trì toàn công ty:
   $$\text{TongDuPhongCanCo} = \sum_{i} DP_i$$
5. So sánh với Số dư Có hiện tại của TK 2293 ($SD_{2293}$):
   - Nếu $\text{TongDuPhongCanCo} > SD_{2293}$: Trích lập bổ sung:
     $$\Delta DP = \text{TongDuPhongCanCo} - SD_{2293}$$
     Hạch toán: $\text{Nợ TK 6426} \quad / \quad \text{Có TK 2293}: \Delta DP$.
   - Nếu $\text{TongDuPhongCanCo} < SD_{2293}$: Hoàn nhập dự phòng:
     $$\Delta DP = SD_{2293} - \text{TongDuPhongCanCo}$$
     Hạch toán: $\text{Nợ TK 2293} \quad / \quad \text{Có TK 6426}: \Delta DP$.
   - Nếu bằng nhau: Không hạch toán.

### 4.2. Thuật toán Đánh giá lại Ngoại tệ Cuối kỳ (VAS 10 & TK 413)
```
[BƯỚC 1: Quét toàn bộ Số dư Ngoại tệ tại Ngày 31/12]
- Quét Tài khoản Tiền: TK 1112, TK 1122
- Quét Tài khoản Công nợ theo từng Đối tượng: TK 131 (Dư Nợ), TK 331 (Dư Có)
                           |
                           v
[BƯỚC 2: Tính Chênh Lệch Tỷ Giá Chi Tiết]
- Với Tài sản (1112, 1122, 131):
  CL = SoDuNgoaiTe * TyGiaMua - SoDuSoSachVnd
  CL > 0 -> Lãi: Nợ 1112/1122/131 / Có 4131
  CL < 0 -> Lỗ:  Nợ 4131 / Có 1112/1122/131
- Với Nợ phải trả (331):
  CL = SoDuSoSachVnd - SoDuNgoaiTe * TyGiaBan
  CL > 0 -> Lãi: Nợ 331 / Có 4131
  CL < 0 -> Lỗ:  Nợ 4131 / Có 331
                           |
                           v
[BƯỚC 3: Kết Chuyển Sạch Số Dư TK 4131 vào Kết quả Tài chính]
- Tính Số dư Thuần trên TK 4131:
  ChenhLechThuan = TongPhatSinhCo(4131) - TongPhatSinhNo(4131)
- Nếu ChenhLechThuan > 0 (Lãi thuần):
  Hạch toán: Nợ TK 4131 / Có TK 515 (Doanh thu tài chính)
- Nếu ChenhLechThuan < 0 (Lỗ thuần):
  Hạch toán: Nợ TK 635 / Có TK 4131 (Chi phí tài chính)
                           |
                           v
[BẤT BIẾN KẾ TOÁN CUỐI KỲ]:
- Số dư TK 4131 cuối kỳ bắt buộc == 0
- Bút toán kết chuyển tuân thủ TongNo == TongCo
```

---

## 5. ĐẶC TẢ GIAO DIỆN & TRẢI NGHIỆM NGƯỜI DÙNG (UI/UX SPECIFICATIONS)

### 5.1. Bảng Trích lập Dự phòng Nợ phải thu Khó đòi (`Views/BadDebt/Create.cshtml`)
```
+======================================================================================================+
| ninjaTax | BẢNG TRÍCH LẬP DỰ PHÒNG NỢ PHẢI THU KHÓ ĐÒI (THÔNG TƯ 48/2019/TT-BTC)                    |
+======================================================================================================+
| Số chứng từ: [ DPNT-2026-0001     ]   Ngày lập: [ 31/12/2026 ]   Kỳ kế toán: [ Năm 2026            ] |
| Số dư hiện tại TK 2293: [ 25.000.000 đ ]   Tổng nợ quá hạn: [ 180.000.000 đ ]                         |
| MỨC DỰ PHÒNG CẦN TRÍCH: [ 42.000.000 đ ]   SỐ TIỀN TRÍCH THÊM: [ +17.000.000 đ ] (Nợ 6426 / Có 2293) |
+------------------------------------------------------------------------------------------------------+
| DANH SÁCH CÔNG NỢ QUÁ HẠN PHẢI TRÍCH LẬP:                                                            |
+----+-----------+---------------+------------+-------------+----------+--------+------------+---------+
| STT| MÃ KH     | TÊN KHÁCH HÀNG| SỐ HÓA ĐƠN | CÒN NỢ (VND)| HẠN TT   | QUÁ HẠN| TỶ LỆ TRÍCH| MỨC DP  |
+----+-----------+---------------+------------+-------------+----------+--------+------------+---------+
|  1 | KH-MINHDUC| Cty Minh Đức  | HĐ-00123   |  50.000.000 | 15/01/24 | 715 ng |     50%    | 25.000M |
|  2 | KH-ANPHAT | Cty An Phát   | HĐ-00245   |  20.000.000 | 10/05/25 | 235 ng |     30%    |  6.000M |
|  3 | KH-HOANGHA| DN tư nhân HH | HĐ-00089   |  11.000.000 | 20/08/23 | 1229 ng|    100%    | 11.000M |
+----+-----------+---------------+------------+-------------+----------+--------+------------+---------+
| CỘNG DỰ PHÒNG THEO TT 48:                                                     |            | 42.000M |
+------------------------------------------------------------------------------------------------------+
| HẠCH TOÁN DỰ PHÒNG:                                                                                  |
| [X] Trích lập bổ sung vào Chi phí QLDN: Nợ TK 6426 / Có TK 2293: 17.000.000 đ                        |
| [ Nút: TÍNH LẠI TOÀN BỘ ]          [ Nút: LƯU & GHI SỔ KẾ TOÁN ]          [ Nút: XUẤT EXCEL ]        |
+======================================================================================================+
```

### 5.2. Màn hình Đánh giá lại Ngoại tệ Cuối kỳ (`Views/FxRevaluation/Index.cshtml`)
```
+======================================================================================================+
| ninjaTax | ĐÁNH GIÁ LẠI CHÊNH LỆCH TỶ GIÁ NGOẠI TỆ CUỐI KỲ (VAS 10 & TK 413)                         |
+======================================================================================================+
| Ngày đánh giá lại: [ 31/12/2026 ]   Đồng tiền: [ USD ▼ ]                                             |
| Tỷ giá mua NHTM (cho Tài sản):     [ 25.450 VND/USD ] (Vietcombank Sở Giao Dịch)                     |
| Tỷ giá bán NHTM (cho Nợ phải trả): [ 25.820 VND/USD ] (Vietcombank Sở Giao Dịch)                     |
+------------------------------------------------------------------------------------------------------+
| KẾT QUẢ ĐÁNH GIÁ LẠI CÁC TÀI KHOẢN TIỀN TỆ CÓ GỐC NGOẠI TỆ:                                          |
+----+--------+--------------------+------------+------------+---------------+---------------+---------+
| STT| SỐ TK  | ĐỐI TƯỢNG / NGÂN HÀ| NGUYÊN TỆ  | TỶ GIÁ SỔ  | GIÁ TRỊ GHI SỔ| GIÁ TRỊ ĐGL   | LÃI/LỖ  |
+----+--------+--------------------+------------+------------+---------------+---------------+---------+
|  1 | 1122   | VCB USD - 00112345 |  10.000 USD|     25.100 |   251.000.000 |   254.500.000 | +3.500M |
|  2 | 131    | Global Tech Corp   |   5.000 USD|     25.200 |   126.000.000 |   127.250.000 | +1.250M |
|  3 | 331    | Intel Asia Ptd Ltd |   8.000 USD|     25.500 |   204.000.000 |   206.560.000 | -2.560M |
+----+--------+--------------------+------------+------------+---------------+---------------+---------+
| TỔNG LÃI TỶ GIÁ (CÓ 4131): 4.750.000 đ     | TỔNG LỖ TỶ GIÁ (NỢ 4131): 2.560.000 đ                 |
| KẾT QUẢ RÒNG (LÃI THUẦN): +2.190.000 đ     | HẠCH TOÁN: Nợ TK 4131 / Có TK 515: 2.190.000 đ        |
+------------------------------------------------------------------------------------------------------+
| [ Nút: THỰC HIỆN ĐÁNH GIÁ LẠI & KẾT CHUYỂN SỔ CÁI ]                      [ Nút: ĐÓNG ]               |
+======================================================================================================+
```

---

## 6. SERVICE LAYER CONTRACTS & TESTING MATRIX

### 6.1. Service Contracts
```csharp
public interface IBadDebtProvisionService
{
    Task<BangTrichLapDuPhongResult> TaoBangTrichLapDuPhongAsync(DateTime ngayHachToan);
    Task<(bool ThanhCong, string? ThongBao)> GhiSoBangTrichLapAsync(long bangTrichLapId);
    Task<List<ChiTietTrichLapDuPhongViewModel>> LayDanhSachNoQuaHanTt48Async(DateTime mocThoiGian);
}

public interface IFxRevaluationService
{
    Task<FxRevaluationResult> ThucHienDanhGiaLaiCuoiKyAsync(DateTime ngayDanhGia, string loaiTien, decimal tyGiaMua, decimal tyGiaBan);
    Task<FxRevaluationPreviewModel> XemTruocDanhGiaLaiAsync(DateTime ngayDanhGia, string loaiTien, decimal tyGiaMua, decimal tyGiaBan);
    Task<(bool ThanhCong, string? ThongBao)> HuyDanhGiaLaiAsync(long id);
}
```

### 6.2. Testing Matrix (Unit & Integration Tests)
1. **UT-AGE-01 (TT48 Aging Buckets)**: Hóa đơn quá hạn 200 ngày $\rightarrow$ Tỷ lệ dự phòng 30%. Hóa đơn quá hạn 400 ngày $\rightarrow$ 50%. Quá hạn 800 ngày $\rightarrow$ 70%. Quá hạn 1200 ngày $\rightarrow$ 100%.
2. **UT-AGE-02 (Provision Net Adjustment)**: Mức dự phòng cần có kỳ này là 50tr. Số dư Có TK 2293 trước trích lập là 30tr $\rightarrow$ Bút toán trích lập bổ sung Nợ 6426 / Có 2293 = 20tr. Nếu số dư Có 2293 là 60tr $\rightarrow$ Bút toán hoàn nhập Nợ 2293 / Có 6426 = 10tr.
3. **UT-FX-01 (VAS 10 Account 413 Invariant)**: Đánh giá lại ngoại tệ cuối kỳ sinh bút toán chi tiết vào TK 4131, sau đó tự động sinh bút toán kết chuyển sạch số dư TK 4131 sang TK 515 hoặc 635. Số dư TK 4131 sau khi chạy đánh giá lại bắt buộc `DuNo == 0 && DuCo == 0`.
4. **UT-FX-02 (Book Lock on FX Revaluation)**: Cấm chạy đánh giá lại tỷ giá nếu `NgayChungTu <= NgayKhoaSo`.
