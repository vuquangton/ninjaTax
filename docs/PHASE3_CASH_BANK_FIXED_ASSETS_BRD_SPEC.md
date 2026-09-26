# TÀI LIỆU YÊU CẦU NGHIỆP VỤ & ĐẶC TẢ KỸ THUẬT CHI TIẾT (BRD & TECHNICAL SPEC)
## PHÂN HỆ TIỀN MẶT, TIỀN GỬI NGÂN HÀNG (CASH & BANK) VÀ TÀI SẢN CỐ ĐỊNH, CÔNG CỤ DỤNG CỤ (FIXED ASSETS & TOOLS)

---

- **Dự án**: `ninjaTax` - Nền tảng Kế toán & Thuế Doanh nghiệp Việt Nam
- **Căn cứ pháp lý cốt lõi**:
  - Thông tư **99/2025/TT-BTC** (Chế độ kế toán doanh nghiệp - Thay thế/cập nhật TT 133/2016, Không dùng TK 911).
  - Thông tư **45/2013/TT-BTC**, sửa đổi bổ sung bởi **TT 147/2016/TT-BTC** và **TT 28/2017/TT-BTC** (Chế độ quản lý, sử dụng và trích khấu hao TSCĐ).
  - Luật Thuế GTGT số 13/2008/QH12, Luật số 31/2013/QH13, Thông tư **219/2013/TT-BTC**, Thông tư **26/2015/TT-BTC** (Quy định bắt buộc thanh toán không dùng tiền mặt với hóa đơn >= 20.000.000 VNĐ).
  - Luật Thuế TNDN số 14/2008/QH12, Thông tư **78/2014/TT-BTC**, Thông tư **96/2015/TT-BTC** (Khống chế thời gian phân bổ CCDC không quá 3 năm / 36 tháng; điều kiện chi phí được trừ khi tính thuế TNDN).
  - Luật Kế toán số **88/2015/QH13** và Nghị định **123/2020/NĐ-CP** (Hóa đơn & Chứng từ kế toán).
- **Tác giả / Thẩm định**:
  - Chief Accountant Lead (20+ năm kinh nghiệm Kế toán trưởng & Quyết toán Thuế thực chiến).
  - Enterprise Solution Architect / Business Analyst Lead (20+ năm kinh nghiệm ERP VAS/IFRS).

---

## MỤC LỤC
1. [Đánh giá Môi trường Vận hành Thực tế (Production Readiness Review)](#1-đánh-giá-môi-trường-vận-hành-thực-tế-production-readiness-review)
2. [Ma Trận Nghiệp Vụ & Bẫy Thanh Tra Thuế (Tax Audit Trap Analysis)](#2-ma-trận-nghiệp-vụ--bẫy-thanh-tra-thuế-tax-audit-trap-analysis)
3. [Phân Hệ Tiền Mặt & Tiền Gửi (Cash & Bank Subsystem)](#3-phân-hệ-tiền-mặt--tiền-gửi-cash--bank-subsystem)
   - 3.1. Sơ đồ Luồng Nghiệp vụ Thu - Chi - Ngân hàng (ASCII Art)
   - 3.2. Phiếu Thu (Cash Receipt) & Báo Có (Bank Credit Advice)
   - 3.3. Phiếu Chi (Cash Payment) & Ủy Nhiệm Chi (Bank Payment Order - UNC)
   - 3.4. Kiểm soát Quy tắc 20 Triệu & Đối chiếu Sổ phụ Ngân hàng (Bank Reconciliation)
   - 3.5. Báo cáo Sổ Quỹ Tiền Mặt & Sổ Tiền Gửi Chuẩn TT99
4. [Phân Hệ Tài Sản Cố Định & Phân Bổ CCDC (Fixed Assets & Depreciation)](#4-phân-hệ-tài-sản-cố-định--phân-bổ-ccdc-fixed-assets--depreciation)
   - 4.1. Sơ đồ Vòng đời TSCĐ & CCDC (ASCII Art Lifecycle)
   - 4.2. Tiêu chuẩn Phân loại TSCĐ (>= 30 Triệu) vs CCDC (< 30 Triệu)
   - 4.3. Thuật toán Trích Khấu hao TSCĐ (Thông tư 45/2013) & Phân bổ 36 tháng
   - 4.4. Tự động Sinh Bút toán Khấu hao / Phân bổ Định kỳ (Nợ 642 / Có 214, Có 242)
   - 4.5. Điều chuyển, Đánh giá lại, Thanh lý & Nhượng bán TSCĐ
5. [Kiến Trúc Dữ Liệu & Entity Relationship (ERD)](#5-kiến-trúc-dữ-liệu--entity-relationship-erd)
6. [Thiết Kế Giao Diện Người Dùng & Trải Nghiệm (UI/UX Wireframes)](#6-thiết-kế-giao-diện-người-dùng--trải-nghiệm-uiux-wireframes)
7. [Kế Hoạch Kiểm Thử TDD & Kịch Bản Nghiệp Vụ](#7-kế-hoạch-kiểm-thử-tdd--kịch-bản-nghiệp-vụ)
8. [Lộ Trình Triển Khai Chi Tiết (Implementation Roadmap)](#8-lộ-trình-triển-khai-chi-tiết-implementation-roadmap)

---

## 1. ĐÁNH GIÁ MÔI TRƯỜNG VẬN HÀNH THỰC TẾ (PRODUCTION READINESS REVIEW)

### Câu hỏi trọng tâm: *Liệu tính năng hiện tại nếu chỉ làm CRUD đơn giản có thể chạy trong môi trường Production thực tế tại Doanh nghiệp Việt Nam?*
> **TRẢ LỜI NGAY CỦA KẾ TOÁN TRƯỞNG & BA LEAD: TUYỆT ĐỐI CHƯA THỂ CHẠY (NOT PRODUCTION READY) NẾU THIẾU CÁC CHỐT CHẶN PHÁP LÝ SAU:**

| Rủi ro / Bẫy nghiệp vụ thực tế | Hậu quả nếu phần mềm không chặn | Giải pháp của `ninjaTax` Phase 3 |
|---|---|---|
| **Bẫy Thanh toán Tiền mặt >= 20 Triệu** (Thông tư 219/2013 & TT 26/2015) | Cơ quan Thuế loại toàn bộ Thuế GTGT đầu vào được khấu trừ và loại chi phí hợp lý khi tính Thuế TNDN (truy thu thuế + phạt 20% + tiền chậm nộp 0.03%/ngày). | **Hard Rule / Warning**: Tự động phát hiện chi tiền mặt cho hóa đơn >= 20.000.000 đ hoặc nhiều hóa đơn cùng 1 NCC trong 1 ngày có tổng >= 20 triệu. Bắt buộc chuyển sang Ủy Nhiệm Chi (TK 1121). |
| **Bẫy Quỹ Tiền Mặt Âm (Negative Cash balance)** | Khi thanh tra, số dư TK 1111 bị âm tại bất kỳ ngày nào trong sổ quỹ sẽ bị Thuế nghi ngờ trốn doanh thu hoặc lập chi khống, phạt hành chính nghiêm trọng. | **Pre-commit Cash Check**: Chặn ghi sổ Phiếu chi nếu `Số dư tồn quỹ đầu ngày + Phát sinh thu trong ngày < Phát sinh chi`. Cảnh báo tức thì cho Thủ quỹ/Kế toán. |
| **Bẫy Phân Bổ CCDC Quá 36 Tháng** (Thông tư 78/2014 & TT 96/2015) | Kế toán phân bổ chi phí CCDC (TK 242) kéo dài hơn 36 tháng bị cơ quan thuế bóc tách chi phí vượt khung, tăng thu nhập chịu thuế TNDN. | **36-Month Hard Limit**: Giới hạn tối đa `SoThangPhanBo <= 36`. Thuật toán tự động tính mức phân bổ hàng tháng chuẩn xác đến từng đồng. |
| **Bẫy Nhầm Lẫn Tiêu Chuẩn TSCĐ vs CCDC** (Thông tư 45/2013) | Đưa tài sản < 30 triệu vào TK 211 trích khấu hao, hoặc đưa tài sản >= 30 triệu dùng > 1 năm vào chi phí 1 lần gây biến động lợi nhuận ảo. | **Validation Rule**: Bắt buộc tài sản trích khấu hao qua TK 211/214 phải có Nguyên giá >= 30.000.000 đ. Dưới 30 triệu tự động đưa vào CCDC (TK 153/242). |
| **Bẫy Lệch Sổ Kế Toán với Sổ Phụ Ngân Hàng** (Bank Statement Mismatch) | Sai lệch số dư tiền gửi TK 112 giữa sổ cái và sao kê ngân hàng do sót phí dịch vụ, lãi tiền gửi, séc chưa khớp. | **Bank Reconciliation Engine**: Module đối chiếu từng dòng giao dịch ngân hàng theo mã tham chiếu / số UNC / mã điện chuyển tiền. |

---

## 2. MA TRẬN NGHIỆP VỤ & BẪY THANH TRA THUẾ (TAX AUDIT TRAP ANALYSIS)

```
+-----------------------------------------------------------------------------------------------+
|                           BẢN ĐỒ RỦI RO THUẾ & CHỐT CHẶN HẠCH TOÁN                           |
+-----------------------------------------------------------------------------------------------+
|                                                                                               |
|   [ GIAO DỊCH TIỀN MẶT ]                  [ THANH TOÁN NGÂN HÀNG ]             [ TSCĐ & CCDC ]|
|             |                                        |                                |       |
|    Số tiền >= 20 Triệu?                     Khớp Sổ phụ Ngân hàng?             Nguyên giá:    |
|       /           \                              /          \                  >= 30 Triệu?   |
|     (CÓ)          (KHÔNG)                     (KHỚP)     (LỆCH)                 /        \    |
|      |               |                           |          |                 (CÓ)      (KHÔNG)|
| [CHẶN PHIẾU CHI]  [CHO PHÉP CHI]              [XÁC NHẬN] [CẢNH BÁO UNRECONCILED]|           | |
| Bắt buộc dùng UNC  Kiểm tra Tồn Quỹ >= 0                 Truy vấn lệch dòng   [TSCĐ-211] [CCDC-242]|
| Tránh bóc thuế     Tránh bẫy âm quỹ                                            Khấu hao  Max 36 tháng|
+-----------------------------------------------------------------------------------------------+
```

---

## 3. PHÂN HỆ TIỀN MẶT & TIỀN GỬI (CASH & BANK SUBSYSTEM)

### 3.1. Sơ đồ Luồng Nghiệp vụ Thu - Chi - Ngân hàng (ASCII Art)

```
 [ĐỐI TƯỢNG (KH/NCC/NV)]
       |
       |--- (1) Nộp tiền mặt --------> [PHIẾU THU - TK 1111] -----> SỔ QUỸ TIỀN MẶT
       |                                      |                     (Thủ quỹ ký nhận)
       |                                      v
       |                               [GHI SỔ GL TT99] (Nợ 1111 / Có 131, 511, 711)
       |
       |--- (2) Đề nghị thanh toán --> [KIỂM TRA HẠN MỨC]
       |                                      |
       |                     +----------------+----------------+
       |                     | >= 20 Triệu hoặc Chuyển khoản   | < 20 Triệu (Tiền mặt)
       |                     v                                 v
       |               [ỦY NHIỆM CHI (UNC)]             [PHIẾU CHI - TK 1111]
       |               (Ngân hàng - TK 1121)                   | (Kiểm tra quỹ >= Chi)
       |                     |                                 v
       |                     v                          SỔ QUỸ TIỀN MẶT
       |            SỔ TIỀN GỬI NGÂN HÀNG                      |
       |                     |                                 v
       |                     +----------------> [GHI SỔ GL TT99] (Nợ 331, 152, 642 / Có 1111, 1121)
       |                                               |
       v                                               v
[HÓA ĐƠN GỐC (PHASE 2)] <-------------------- [ĐỐI TRỪ CÔNG NỢ] (Cập nhật DaThanhToan, DaThuTien)
```

### 3.2. Phiếu Thu (Cash Receipt) & Báo Có (Bank Credit Advice)
- **Tài khoản Nợ**:
  - `1111`: Tiền Việt Nam tại quỹ.
  - `1121`: Tiền Việt Nam gửi ngân hàng (kèm thông tin Tài khoản ngân hàng doanh nghiệp).
- **Tài khoản Có**:
  - `131`: Thu nợ khách hàng (liên kết Hóa đơn bán hàng Phase 2).
  - `5111, 5112, 5113`: Thu tiền bán hàng ngay không qua công nợ.
  - `141`: Hoàn ứng nhân viên.
  - `711`: Thu nhập khác.
- **Ràng buộc nghiệp vụ**:
  - Tự động sinh số chứng từ theo định dạng: `PT-YYYY-XXXXX` (Phiếu thu) và `BC-YYYY-XXXXX` (Báo có).
  - Cho phép chọn đích danh Hóa đơn bán ra cần thu tiền để tự động gọi `CongNoService.DoiTruHoaDonBanAsync`.

### 3.3. Phiếu Chi (Cash Payment) & Ủy Nhiệm Chi (Bank Payment Order - UNC)
- **Tài khoản Nợ**:
  - `331`: Trả nợ nhà cung cấp (liên kết Hóa đơn mua hàng Phase 2).
  - `152, 156`: Mua vật tư, hàng hóa trả tiền ngay.
  - `642`: Chi phí quản lý doanh nghiệp (tiền điện, nước, văn phòng phẩm, tiếp khách...).
  - `141`: Tạm ứng công tác phí cho nhân viên.
  - `334`: Chi trả lương cán bộ công nhân viên.
- **Tài khoản Có**:
  - `1111`: Chi tiền mặt.
  - `1121`: Chi chuyển khoản qua Ngân hàng.
- **Quy tắc Kiểm soát Quỹ (Cash Balance Safety)**:
  $$\text{Tồn khả dụng} = \text{Tồn đầu kỳ} + \sum \text{Thu trong kỳ} - \sum \text{Chi đã ghi sổ}$$
  *Nếu `Số tiền chi > Tồn khả dụng` $\rightarrow$ Chặn thao tác Ghi sổ, đưa ra cảnh báo "Không đủ tiền mặt trong quỹ để chi trả".*

### 3.4. Kiểm soát Quy tắc 20 Triệu & Đối chiếu Sổ Phụ Ngân Hàng
- **Quy tắc 20 Triệu**:
  - Khi lập Phiếu chi tiền mặt (Có TK 1111) thanh toán cho Hóa đơn mua hàng:
    - Nếu Hóa đơn có `TongThanhToan >= 20.000.000 VNĐ`: **BẬT CẢNH BÁO ĐỎ**. Kế toán buộc phải chọn phương thức Ủy Nhiệm Chi (Có TK 1121).
    - Nếu cố tình chi bằng tiền mặt: Đánh dấu cờ `ViPhamQuyTac20Tr = true` để phục vụ tự động loại trừ thuế GTGT đầu vào và chi phí TNDN khi lập tờ khai quyết toán cuối năm.
- **Bank Reconciliation Engine**:
  - Lưu trữ thông tin: Ngân hàng giao dịch, Số tài khoản ngân hàng, Mã điện chuyển tiền / Số chứng từ ngân hàng.
  - Báo cáo đối chiếu số phát sinh trên Sổ kế toán vs Sổ phụ sao kê ngân hàng.

---

## 4. PHÂN HỆ TÀI SẢN CỐ ĐỊNH & PHÂN BỔ CCDC (FIXED ASSETS & DEPRECIATION)

### 4.1. Sơ đồ Vòng đời TSCĐ & CCDC (ASCII Art Lifecycle)

```
[MUA MỚI / ĐẦU TƯ / TỰ CHẾ] (Hóa đơn mua Phase 2 / Phiếu chi)
           |
           +---> Nguyên giá >= 30 Triệu & TGSD > 1 năm?
           |                |
           |         +------+------+
           |         | (CÓ)        | (KHÔNG)
           |         v             v
           |    [GHI TĂNG TSCĐ]  [GHI TĂNG CCDC]
           |    (TK 211 / 213)   (TK 153 -> 242)
           |         |                 |
           |         |                 v
           |         |       [PHÂN BỔ CHI PHÍ 242]
           |         |       (Tối đa <= 36 tháng)
           |         v                 |
           +----> [HÀNG THÁNG: CHẠY BẢNG KHẤU HAO / PHÂN BỔ]
                     |                 |
                     | Nợ 642          | Nợ 642
                     | Có 214          | Có 242
                     v                 v
                 [SỔ CÁI BÚT TOÁN CHUẨN TT99]
                     |
         +-----------+-----------+
         |                       |
         v                       v
[THANH LÝ / NHƯỢNG BÁN]     [ĐIỀU CHUYỂN BỘ PHẬN]
Nợ 214 (Hao mòn lũy kế)     Cập nhật đối tượng / bộ phận chịu chi phí
Nợ 811 (Giá trị còn lại)
Có 211 (Xóa nguyên giá)
```

### 4.2. Tiêu chuẩn Phân loại TSCĐ (>= 30 Triệu) vs CCDC (< 30 Triệu)
Căn cứ Điều 3 Thông tư **45/2013/TT-BTC**:
1. **Tài sản cố định hữu hình (TK 211)**:
   - Chắc chắn thu được lợi ích kinh tế trong tương lai từ việc sử dụng tài sản đó.
   - Có thời gian sử dụng trên 01 năm trở lên.
   - **Nguyên giá tài sản phải được xác định một cách tin cậy và có giá trị từ 30.000.000 đồng (Ba mươi triệu đồng) trở lên.**
2. **Công cụ dụng cụ / Chi phí trả trước (TK 242)**:
   - Những tư liệu lao động có giá trị < 30.000.000 đồng hoặc thời gian sử dụng <= 1 năm.
   - Khi xuất dùng đưa qua TK 242 để phân bổ dần vào chi phí quản lý (TK 642).
   - **Thời hạn phân bổ tối đa không quá 36 tháng (3 năm)** theo Thông tư 78/2014/TT-BTC & TT 96/2015/TT-BTC.

### 4.3. Thuật toán Trích Khấu hao TSCĐ (Thông tư 45/2013) & Phân bổ 36 tháng
- **Phương pháp khấu hao đường thẳng (Straight-line Method)**:
  $$\text{Mức trích khấu hao năm} = \frac{\text{Nguyên giá}}{\text{Thời gian trích khấu hao (năm)}}$$
  $$\text{Mức trích khấu hao tháng} = \frac{\text{Mức trích khấu hao năm}}{12}$$
- **Khấu hao tài sản tăng / giảm trong tháng (Prudential Daily Calculation)**:
  $$\text{Số ngày sử dụng trong tháng} = \text{Số ngày của tháng} - \text{Ngày bắt đầu tính khấu hao} + 1$$
  $$\text{Khấu hao tháng đầu tiên} = \frac{\text{Mức trích khấu hao tháng}}{\text{Số ngày trong tháng}} \times \text{Số ngày sử dụng trong tháng}$$
- **Độ chính xác số học**:
  - Áp dụng `decimal(19, 4)` cho mọi biến tính toán.
  - Làm tròn tiền VNĐ chính thức khi hạch toán vào sổ sách (Round Midpoint Away From Zero).

### 4.4. Tự động Sinh Bút toán Khấu hao / Phân bổ Định kỳ
- **Hạch toán Khấu hao TSCĐ hàng tháng (TT99)**:
  - **Nợ TK 642**: Chi phí quản lý doanh nghiệp (Bộ phận văn phòng / Quản trị).
  - **Có TK 214**: Hao mòn tài sản cố định (TK 2141: TSCĐ hữu hình).
- **Hạch toán Phân bổ CCDC hàng tháng (TT99)**:
  - **Nợ TK 642**: Chi phí quản lý doanh nghiệp.
  - **Có TK 242**: Chi phí trả trước dài hạn (giảm số dư chưa phân bổ).
- **Invariants**:
  - Tổng Nợ == Tổng Có.
  - Không phân bổ vượt quá Nguyên giá / Giá trị còn lại.
  - Khi `GiaTriConLai == 0` $\rightarrow$ Tự động chuyển trạng thái tài sản sang `DaKhauHaoHet` và dừng trích tháng tiếp theo.

---

## 5. KIẾN TRÚC DỮ LIỆU & ENTITY RELATIONSHIP (ERD)

```
+-----------------------------------------------------------------------------------------------+
|                                    SƠ ĐỒ ENTITY RELATIONSHIP                                  |
+-----------------------------------------------------------------------------------------------+

  +-----------------------+              +-----------------------+
  |    TaiKhoanNganHang   | 1          * |      ChungTuThuChi    |
  |-----------------------|--------------|-----------------------|
  | Id (long) PK          |              | Id (long) PK          |
  | SoTaiKhoan (string)   |              | SoChungTu (string)    |
  | TenNganHang (string)  |              | LoaiChungTu (enum)    |---> 1: Thu TM, 2: Chi TM,
  | ChiNhanh (string)     |              | NgayHachToan (Date)   |     3: Báo Có, 4: Báo Nợ/UNC
  | ChuTaiKhoan (string)  |              | NgayChungTu (Date)    |
  +-----------------------+              | DoiTuongId (long) FK  |
                                         | NguoiNopNhan (string) |
                                         | LyDoThuChi (string)   |
                                         | TongTien (decimal)    |
                                         | ButToanId (long?) FK  |---> Core GL (Phase 1)
                                         | HoaDonId (long?) FK   |---> HĐ Mua/Bán (Phase 2)
                                         +-----------------------+
                                                     | 1
                                                     | *
                                         +-----------------------+
                                         | ChiTietChungTuThuChi  |
                                         |-----------------------|
                                         | Id (long) PK          |
                                         | ChungTuThuChiId (long)|
                                         | TaiKhoanNoId (long) FK|
                                         | TaiKhoanCoId (long) FK|
                                         | SoTien (decimal 19,4) |
                                         | DienGiai (string)     |
                                         +-----------------------+

  +-----------------------+ 1          * +-----------------------+
  |       TaiSanCoDinh    |--------------|   BangTinhKhauHao     |
  |-----------------------|              |-----------------------|
  | Id (long) PK          |              | Id (long) PK          |
  | MaTaiSan (string)     |              | TaiSanCoDinhId (long) |
  | TenTaiSan (string)    |              | KyKeToan (string MM/Y)|
  | Loai (enum: TS, CCDC) |              | NguyenGia (decimal)   |
  | NgayGhiTang (Date)    |              | KhauHaoThang (decimal)|
  | NguyenGia (decimal)   |              | LuyKeKhauHao (decimal)|
  | ThoiGianSuDungThang   |              | GiaTriConLai (decimal)|
  | GiaTriConLai (decimal)|              | ButToanId (long?) FK  |---> Core GL (Phase 1)
  | TaiKhoanNguyenGiaId   | (211, 242)   +-----------------------+
  | TaiKhoanKhauHaoId     | (214, 242)
  | TaiKhoanChiPhiId      | (642)
  | TrangThai (enum)      | (DangSD, DaHet, ThanhLy)
  +-----------------------+
```

---

## 6. THIẾT KẾ GIAO DIỆN NGƯỜI DÙNG & TRẢI NGHIỆM (UI/UX WIREFRAMES)

### 6.1. Giao diện Lập Phiếu Thu / Báo Có (`Views/ThuTien/Create.cshtml`)
```
+-----------------------------------------------------------------------------------------------+
|  ninjaTax > Quản lý Tiền > [ Lập Phiếu Thu / Báo Có Ngân Hàng ]                               |
+-----------------------------------------------------------------------------------------------+
|  (*) Hình thức: [ (o) Tiền mặt (TK 1111) ]   [ ( ) Tiền gửi ngân hàng (Báo Có - TK 1121) ]    |
|  Tài khoản ngân hàng nhận: [ VCB - 001100456789 - Vietcombank SGD ---------------------- [v] ]|
+-----------------------------------------------------------------------------------------------+
|  Số chứng từ: [ PT-2026-00015 ]   Ngày HT: [ 26/09/2026 ]   Ngày chứng từ: [ 26/09/2026 ]     |
|  Đối tượng:   [ KH001 - Công ty CP Công Nghệ Sao Mai --------------------------------- [v] ] |
|  Người nộp:   [ Nguyễn Văn A ]     Địa chỉ: [ Tầng 5, Tòa Keangnam, Hà Nội ]                  |
|  Lý do thu:   [ Thu tiền thanh toán đợt 2 theo HĐĐT C26T-00000001                            ]|
+-----------------------------------------------------------------------------------------------+
|  Hóa đơn bán đối trừ: [ HĐĐT C26T-00000001 (Còn nợ: 88.000.000 đ) --------------------- [v] ] |
+-----------------------------------------------------------------------------------------------+
|  [#] | Diễn giải dòng               | TK Nợ  | TK Có  | Số tiền (VNĐ)        | Thao tác       |
|  [1] | Thu tiền bán hàng HĐ C26T    | 1111   | 131    |        88,000,000    | [Xóa]          |
+-----------------------------------------------------------------------------------------------+
|                                                TỔNG THU:       88,000,000 đ                   |
|                                       (Bằng chữ: Tám mươi tám triệu đồng chẵn)                |
|                                                                                               |
|  [ Hủy bỏ ]                                              [ In Phiếu Thu ]  [ LƯU & GHI SỔ ]   |
+-----------------------------------------------------------------------------------------------+
```

### 6.2. Giao diện Lập Phiếu Chi / Ủy Nhiệm Chi (`Views/ChiTien/Create.cshtml`)
```
+-----------------------------------------------------------------------------------------------+
|  ninjaTax > Quản lý Tiền > [ Lập Phiếu Chi / Ủy Nhiệm Chi (UNC) ]                             |
+-----------------------------------------------------------------------------------------------+
|  (*) Phương thức: [ ( ) Tiền mặt (TK 1111) ]   [ (o) Ủy Nhiệm Chi Ngân Hàng (TK 1121) ]        |
|  Tài khoản trích nợ: [ TCB - 1902888999001 - Techcombank Hà Nội ---------------------- [v] ] |
|  Tồn quỹ tiền mặt hiện tại: 45,200,000 đ | Số dư ngân hàng khả dụng: 520,000,000 đ            |
+-----------------------------------------------------------------------------------------------+
|  [!] CẢNH BÁO QUY TẮC 20 TRIỆU: Hóa đơn mua MH-002 có tổng thanh toán 55.000.000 đ            |
|      Hệ thống tự động kích hoạt chế độ Ủy Nhiệm Chi để bảo toàn thuế GTGT và TNDN.            |
+-----------------------------------------------------------------------------------------------+
|  Số chứng từ: [ UNC-2026-0008 ]   Ngày HT: [ 26/09/2026 ]   Số UNC Ngân hàng: [ 8849201 ]      |
|  Đơn vị nhận: [ NCC001 - Công ty TNHH Thiết Bị Mạng Cisco VN -------------------------- [v] ] |
|  Tài khoản nhận: [ 0451000332211 tại VCB Thăng Long ]                                         |
|  Nội dung:    [ Thanh toán tiền mua máy chủ Server Dell theo HĐ MH-2026-0002                  ]|
+-----------------------------------------------------------------------------------------------+
|  [#] | Diễn giải chi tiết            | TK Nợ  | TK Có  | Số tiền (VNĐ)        | Thao tác       |
|  [1] | Trả tiền NCC Cisco VN        | 331    | 1121   |        55,000,000    | [Xóa]          |
+-----------------------------------------------------------------------------------------------+
|                                                TỔNG CHI:       55,000,000 đ                   |
|  [ Hủy bỏ ]                                          [ In Mẫu UNC Ngân Hàng ] [ LƯU & GHI SỔ ]|
+-----------------------------------------------------------------------------------------------+
```

### 6.3. Giao diện Quản Lý & Tính Khấu Hao TSCĐ / Phân Bổ CCDC (`Views/TaiSan/Index.cshtml`)
```
+-----------------------------------------------------------------------------------------------+
|  ninjaTax > Tài Sản Cố Định & Công Cụ Dụng Cụ > [ Bảng Tính Khấu Hao Kỳ 09/2026 ]             |
+-----------------------------------------------------------------------------------------------+
|  [ + Khai Báo TSCĐ Mới ]   [ + Khai Báo CCDC ]   [ CHẠY KHẤU HAO THÁNG ]   [ Xuất Báo Cáo ]  |
+-----------------------------------------------------------------------------------------------+
| Mã TS  | Tên tài sản / CCDC        | Loại | Ngày mua | Nguyên giá   | Đã KH lũy kế | Giá trị còn |
| TS001  | Máy chủ Dell R750xs       | TSCĐ | 15/01/26 |   65,000,000 |    8,125,000 |  56,875,000 |
| TS002  | Router Cisco C9200        | CCDC | 01/03/26 |   21,000,000 |    3,500,000 |  17,500,000 |
| TS003  | Laptop Dell XPS Kế toán   | CCDC | 10/05/26 |   28,000,000 |    3,111,111 |  24,888,889 |
+-----------------------------------------------------------------------------------------------+
| TỔNG CỘNG NGUYÊN GIÁ:  114,000,000 đ | MỨC TRÍCH THÁNG NÀY:  2,854,167 đ                      |
| ĐÃ GHI SỔ KỲ NÀY: [ Bút toán PKT-KH-092026: Nợ 642 / Có 214, 242: 2,854,167 đ (Đã ghi sổ) ]  |
+-----------------------------------------------------------------------------------------------+
```

---

## 7. KẾ HOẠCH KIỂM THỬ TDD & KỊCH BẢN NGHIỆP VỤ

### 7.1. Danh Sách Unit & Integration Tests Cần Triển Khai
1. **`CashReceiptTests.cs`**:
   - Kiểm tra lập phiếu thu tiền mặt (Nợ 1111 / Có 131) tăng số dư tiền mặt chính xác.
   - Kiểm tra báo có ngân hàng (Nợ 1121 / Có 131, 511) tăng số dư ngân hàng và đối trừ hóa đơn bán.
2. **`CashPaymentSafetyTests.cs`**:
   - Kiểm tra bẫy âm quỹ tiền mặt: Cố tình chi vượt quá tồn quỹ $\rightarrow$ Throw Exception / Báo lỗi không cho ghi sổ.
   - Kiểm tra bẫy 20 triệu: Hóa đơn mua hàng 25.000.000 đ chi bằng tiền mặt $\rightarrow$ Kích hoạt cảnh báo đỏ vi phạm điều kiện khấu trừ thuế GTGT.
   - Kiểm tra lập UNC ngân hàng thành công: Nợ 331 / Có 1121.
3. **`FixedAssetDepreciationTests.cs`**:
   - Kiểm tra quy tắc phân loại: Tài sản < 30 triệu không được khai báo vào TK 211 (buộc vào TK 242 CCDC).
   - Kiểm tra giới hạn 36 tháng: CCDC có thời gian phân bổ > 36 tháng $\rightarrow$ Throw Validation Error.
   - Kiểm tra thuật toán khấu hao đường thẳng: Tính đúng số tiền khấu hao tròn tháng và số tiền khấu hao tháng đầu tiên lẻ ngày.
   - Kiểm tra tự động sinh bút toán khấu hao: Nợ 642 / Có 214 & Có 242 (`TongNo == TongCo`).
   - Kiểm tra điểm dừng khấu hao: Khi Giá trị còn lại về 0 $\rightarrow$ Không tiếp tục trích ở tháng kế tiếp.

---

## 8. LỘ TRÌNH TRIỂN KHAI CHI TIẾT (IMPLEMENTATION ROADMAP)

| Bước | Hạng mục công việc | Mô tả kỹ thuật | Deliverables |
|---|---|---|---|
| **Step 1** | **Entity Modeling & Database Migration** | Thêm các entity `TaiKhoanNganHang`, `ChungTuThuChi`, `ChiTietChungTuThuChi`, `TaiSanCoDinh`, `BangTinhKhauHao`. Cấu hình `decimal(19,4)` và quan hệ FK. | EF Core Migration Phase 3 |
| **Step 2** | **Cash & Bank Engine Services** | `IThuChiService` & `ThuChiService`: Quản lý thu/chi, kiểm tra âm quỹ, kiểm soát quy tắc 20 triệu, sinh bút toán Sổ cái TT99, đối trừ hóa đơn. | `Models/Services/ThuChiService.cs` |
| **Step 3** | **Fixed Assets & Depreciation Services** | `ITaiSanService` & `TaiSanService`: Đăng ký tài sản, kiểm soát ngưỡng 30M/36 tháng, công thức khấu hao đường thẳng, sinh bút toán Nợ 642 / Có 214, 242. | `Models/Services/TaiSanService.cs` |
| **Step 4** | **TDD Test Suite Implementation** | Viết trọn bộ unit tests bao phủ 100% các ca nghiệp vụ nhạy cảm thuế: Quỹ âm, Chạm ngưỡng 20M, Phân bổ CCDC vượt 36 tháng, Khấu hao lẻ ngày. | `ninjaTax.Tests/CashAndAssetTests.cs` |
| **Step 5** | **Controllers & ViewModels** | `ThuTienController`, `ChiTienController`, `TaiSanController`. | `Controllers/` & `ViewModels/` |
| **Step 6** | **Razor Views & UI Integration** | Màn hình lập phiếu thu, lập phiếu chi, in UNC ngân hàng, sổ quỹ tiền mặt, sổ tiền gửi, bảng tính khấu hao định kỳ. | `Views/ThuChi/`, `Views/TaiSan/` |
| **Step 7** | **Final Build, Test & Git Sync** | Chạy `dotnet build` (0 warnings), `dotnet test` (100% pass), commit & push lên GitHub repository. | Git Commit & Push |
