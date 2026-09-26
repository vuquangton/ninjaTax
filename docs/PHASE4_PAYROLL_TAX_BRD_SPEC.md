# TÀI LIỆU YÊU CẦU NGHIỆP VỤ & ĐẶC TẢ KỸ THUẬT CHI TIẾT (BRD & TECHNICAL SPEC)
## PHÂN HỆ TIỀN LƯƠNG, BẢO HIỂM BẮT BUỘC & THUẾ TNCN KHẤU TRỪ TẠI NGUỒN (PAYROLL, STATUTORY INSURANCE & PIT)

---

- **Dự án**: `ninjaTax` - Nền tảng Kế toán & Thuế Doanh nghiệp Việt Nam
- **Căn cứ pháp lý cốt lõi**:
  - Thông tư **99/2025/TT-BTC** (Chế độ kế toán doanh nghiệp - Thay thế/cập nhật TT 133/2016, Tuyệt đối không dùng TK 911).
  - Bộ luật Lao động số **45/2019/QH14** (Quy định hợp đồng lao động, tiền lương làm thêm giờ Điều 98, nghỉ phép năm, ngày lễ tết).
  - Nghị định **73/2024/NĐ-CP** (Quy định mức lương cơ sở 2.340.000 VNĐ/tháng áp dụng từ 01/07/2024 làm căn cứ tính trần đóng BHXH, BHYT).
  - Nghị định **74/2024/NĐ-CP** & Nghị định **293/2025/NĐ-CP** (Mức lương tối thiểu vùng theo tháng và theo giờ làm căn cứ tính sàn đóng BHXH và trần BHTN).
  - Luật Bảo hiểm xã hội số **41/2024/QH15** (Có hiệu lực từ 01/07/2025) và Luật Việc làm (Quy định tỷ lệ trích nộp BHXH, BHYT, BHTN, chế độ ốm đau thai sản).
  - Nghị định **191/2013/NĐ-CP** (Quy định chi tiết về tài chính công đoàn - Doanh nghiệp bắt buộc nộp 2% KPCĐ).
  - Luật Thuế Thu nhập cá nhân số **04/2007/QH12**, Luật số **26/2012/QH13**, Thông tư **111/2013/TT-BTC**, Thông tư **92/2015/TT-BTC**, Thông tư **80/2021/TT-BTC** (Biểu lũy tiến từng phần, khấu trừ 10% vãng lai, cam kết Mẫu 08/CK-TNCN).
  - Nghị quyết **954/2020/UBTVQH14** (Mức giảm trừ gia cảnh: 11.000.000 VNĐ/tháng cho bản thân và 4.400.000 VNĐ/tháng cho mỗi người phụ thuộc) và lộ trình cập nhật theo Luật Thuế TNCN mới.
  - Thông tư **26/2016/TT-BLĐTBXH** (Mức khống chế tiền ăn giữa ca tối đa 730.000 VNĐ/tháng được miễn thuế TNCN và không đóng bảo hiểm).
- **Tác giả / Thẩm định**:
  - Chief Accountant Lead (20+ năm kinh nghiệm Kế toán trưởng & Quyết toán Thuế/Bảo hiểm thực chiến).
  - Enterprise Solution Architect / Business Analyst Lead (20+ năm kinh nghiệm ERP VAS/IFRS).

---

## MỤC LỤC
1. [Đánh giá Vận hành Môi trường Production (Production Readiness Audit)](#1-đánh-giá-vận-hành-môi-trường-production-production-readiness-audit)
2. [Ma Trận Bẫy Thanh Tra Thuế & Cơ Quan BHXH (Audit Trap Matrix)](#2-ma-trận-bẫy-thanh-tra-thuế--cơ-quan-bhxh-audit-trap-matrix)
3. [Kiến Trúc Nghiệp Vụ Chấm Công & Tiền Lương (Timesheet & Payroll Engine)](#3-kiến-trúc-nghiệp-vụ-chấm-công--tiền-lương-timesheet--payroll-engine)
   - 3.1. Sơ đồ Luồng Nghiệp vụ Chấm Công & Tính Lương (ASCII Art)
   - 3.2. Bảng Chấm Công & Phân loại Thời Gian Lao Động (Timesheet Types)
   - 3.3. Thuật toán Lương Làm Thêm Giờ (Overtime - OT) & Tách Phần Miễn Thuế
   - 3.4. Các Khoản Phụ Cấp & Khung Miễn Thuế TNCN / Miễn Đóng BHXH
4. [Đặc Tả Trích Bảo Hiểm Bắt Buộc & Kinh Phí Công Đoàn (Statutory Deductions)](#4-đặc-tả-trích-bảo-hiểm-bắt-buộc--kinh-phí-công-đoàn-statutory-deductions)
   - 4.1. Bảng Tỷ Lệ & Trần/Sàn Đóng BHXH, BHYT, BHTN, KPCĐ
   - 4.2. Xử lý Chế độ Thai sản & Ốm đau (Mẫu C70a-HD)
5. [Đặc Tả Khấu Trừ Thuế Thu Nhập Cá Nhân (PIT Withholding Engine)](#5-đặc-tả-khấu-trừ-thuế-thu-nhập-cá-nhân-pit-withholding-engine)
   - 5.1. Phân loại Hợp đồng Lao động & Phương pháp Khấu trừ
   - 5.2. Công thức Biểu thuế Lũy tiến Từng phần (7 Bậc & 5 Bậc mới)
   - 5.3. Khấu trừ 10% tại nguồn & Quản lý Cam kết Mẫu 08/CK-TNCN
6. [Hạch Toán Kép Chuẩn Thông Tư 99/2025/TT-BTC (GL Double-Entry Postings)](#6-hạch-toán-kép-chuẩn-thông-tư-992025tt-btc-gl-double-entry-postings)
7. [Mô Hình Thực Thể CSDL (Entity Relationship & Multi-DB Schema)](#7-mô-hình-thực-thể-csdl-entity-relationship--multi-db-schema)
8. [Thiết Kế Giao Diện Người Dùng & Phiếu Lương (UI/UX Wireframes)](#8-thiết-kế-giao-diện-người-dùng--phiếu-lương-uiux-wireframes)
9. [Bộ Kịch Bản Kiểm Thử TDD (Test-Driven Development Specs)](#9-bộ-kịch-bản-kiểm-thử-tdd-test-driven-development-specs)
10. [Lộ Trình Triển Khai Chi Tiết (Implementation Roadmap)](#10-lộ-trình-triển-khai-chi-tiết-implementation-roadmap)

---

## 1. ĐÁNH GIÁ VẬN HÀNH MÔI TRƯỜNG PRODUCTION (PRODUCTION READINESS AUDIT)

### Câu hỏi cốt tử: *Mô tả ban đầu (Flat rates: 8%, 1.5%, 1%, 2%, Nợ 642/Có 334, Nợ 334/Có 3335, Chi lương 334/111/112) có thể đưa ngay vào chạy Production được không?*

> 🚨 **KẾT LUẬN CỦA KẾ TOÁN TRƯỞNG & BA LEAD (20+ NĂM KINH NGHIỆM):**  
> **TUYỆT ĐỐI KHÔNG THỂ VẬN HÀNH TRONG MÔI TRƯỜNG DOANH NGHIỆP THỰC TẾ (FATAL PRODUCTION FAILURE)!**  
> Nếu triển khai với đặc tả thô sơ trên, hệ thống sẽ gây ra các thảm họa pháp lý, bị Cơ quan Thuế loại trừ hàng tỷ đồng chi phí lương và bị Cơ quan BHXH xử phạt truy thu lãi chậm nộp nặng nề.

---

### Bảng Phân Tích Lỗ Hổng Tử Huyệt (Fatal Audit Failure Points):

| STT | Lỗ hổng chết người nếu làm theo spec thô sơ | Hậu quả thực tế khi Quyết toán Thuế & Kiểm toán BHXH | Giải pháp thiết kế bắt buộc của `ninjaTax` |
|---|---|---|---|
| **1** | **Bỏ qua Mức Trần đóng BHXH/BHYT (20 lần Mức lương cơ sở)** | Lương cơ sở từ 01/07/2024 là **2.340.000 VNĐ** (NĐ 73/2024) $\rightarrow$ **Trần đóng BHXH/BHYT = 46.800.000 VNĐ**. Nếu nhân sự lương 100M mà trừ 8% BHXH trên 100M $\rightarrow$ Trừ sai tiền NLĐ, hồ sơ D02-LT gửi BHXH điện tử bị từ chối 100%. | **Insurance Cap Rule**: Tự động áp trần `Min(LuongDongBH, 20 * LuongCoSo)`. |
| **2** | **Bỏ qua Mức Trần đóng BHTN (20 lần Lương tối thiểu vùng)** | Trần BHTN tính theo lương tối thiểu vùng (Vùng 1: 4.960.000đ $\times$ 20 = **99.200.000 VNĐ** theo NĐ 74/2024). Nếu áp chung trần của BHXH sẽ tính sai tiền BHTN. | **Unemployment Cap Rule**: Tách riêng trần BHTN theo Vùng địa lý của Doanh nghiệp. |
| **3** | **Bỏ qua Mức Sàn đóng Bảo hiểm** | Lương đóng bảo hiểm không được thấp hơn mức lương tối thiểu vùng (và cộng 7% với lao động qua đào tạo nếu thỏa ước quy định). Khai báo thấp hơn sẽ bị cơ quan BHXH phạt hành chính. | **Insurance Floor Rule**: Cảnh báo và chặn lập bảng lương nếu lương đóng BH < Lương tối thiểu vùng. |
| **4** | **Tính thuế TNCN cào bằng (Không có Giảm trừ gia cảnh & Biểu lũy tiến)** | Người có thu nhập dưới 11M vẫn bị trừ thuế, hoặc người có 3 người phụ thuộc (giảm trừ $11M + 3 \times 4.4M = 24.2M$) bị trừ sai. Doanh nghiệp bị người lao động kiện và phạt vi phạm hành chính về thuế. | **PIT Engine**: Xây dựng thuật toán tính thuế TNCN 7 bậc lũy tiến từng phần, tích hợp giảm trừ gia cảnh bản thân (11M) và người phụ thuộc (4.4M/người). |
| **5** | **Đánh đồng HĐLĐ $\ge 3$ tháng và HĐLĐ thử việc / thời vụ $< 3$ tháng** | Người thử việc hoặc cộng tác viên không ký HĐLĐ trên 3 tháng có thu nhập $\ge 2.000.000$ đ/lần phải **khấu trừ 10% tại nguồn** (trừ khi có Cam kết 08/CK-TNCN theo TT 80/2021). Nếu tính theo lũy tiến sẽ bị truy thu thuế TNCN kèm phạt 20%. | **Contract Classifier**: Phân loại hợp đồng: `HdLaoDongDaiHan` (Lũy tiến), `HdThoiVu_ThuViec` (10% hoặc Cam kết 08), `ChuyenGiaNuocNgoai` (20% không cư trú). |
| **6** | **Không tách Khoản Phụ Cấp Miễn Thuế & Tiền Làm Thêm Giờ (OT)** | Tiền ăn trưa $\le 730.000$ đ, tiền trang phục $\le 5.000.000$ đ/năm, và **phần tiền lương trả cao hơn do làm thêm giờ (50% ngày thường, 100% ngày nghỉ, 200% ngày lễ)** được **miễn thuế TNCN** (Điểm i Khoản 1 Điều 3 TT 111/2013). Nếu đánh thuế cả phần này là vi phạm luật. | **Tax-Exempt Allowance Engine**: Tự động bóc tách thu nhập chịu thuế và thu nhập tính thuế theo đúng quy chế lương. |
| **7** | **Bẫy Âm Quỹ khi Chi Lương Tiền Mặt** | Doanh nghiệp chi tiền mặt trả lương (TK 334 / TK 1111) dẫn đến âm sổ quỹ tiền mặt trong ngày trả lương. | **Cash Guard Integration**: Bắt buộc kiểm tra `TonQuyKhaDung` từ Phase 3 trước khi duyệt phiếu chi lương tiền mặt. |
| **8** | **Kinh phí Công đoàn (2% KPCĐ)** | Kể cả doanh nghiệp chưa có tổ chức công đoàn cơ sở vẫn bắt buộc phải nộp 2% KPCĐ tính trên quỹ lương đóng BHXH theo Nghị định 191/2013/NĐ-CP. | **Trade Union Rule**: Tự động tính 2% KPCĐ vào chi phí doanh nghiệp (Nợ 642 / Có 3382). |

---

## 2. MA TRẬN BẪY THANH TRA THUẾ & CƠ QUAN BHXH (AUDIT TRAP MATRIX)

```
+---------------------------------------------------------------------------------------------------+
|                        BẢN ĐỒ KIỂM SOÁT RỦI RO LƯƠNG, BẢO HIỂM & THUẾ TNCN                        |
+---------------------------------------------------------------------------------------------------+
|                                                                                                   |
|  [ THỜI GIAN LAO ĐỘNG ]             [ BẢO HIỂM BẮT BUỘC ]                 [ THUẾ THU NHẬP CÁ NHÂN ]|
|            |                                  |                                       |           |
|     Bảng chấm công:                   Lương đóng BHXH:                       Loại Hợp đồng LĐ:    |
|   - Ngày công chuẩn (22/26)         - Sàn: >= Lương TT Vùng               - HĐLĐ >= 3 tháng:      |
|   - Nghỉ phép năm (100% lương)      - Trần BHXH/BHYT: <= 20*LươngCS          Biểu lũy tiến 7 bậc  |
|   - Nghỉ ốm/thai sản (BHXH trả)     - Trần BHTN: <= 20*Lương vùng            Giảm trừ: 11M + 4.4M |
|   - Làm thêm giờ OT:                          |                           - HĐLĐ < 3 tháng / CTV: |
|     * Ngày thường: 150%             Tỷ lệ trích nộp:                         Khấu trừ 10% tại chỗ |
|     * Ngày nghỉ tuần: 200%          * DN: 17.5% + 3% + 1% + 2% KPCĐ          (Hoặc Cam kết 08)    |
|     * Lễ/Tết: 300%                  * NLĐ: 8% + 1.5% + 1%                 - Miễn thuế TNCN:       |
|            |                                  |                              * Tiền ăn <= 730k    |
|  [TÁCH PHẦN VƯỢT OT MIỄN THUẾ]      [CHỐT BẢNG TRÍCH 338]                    * Chênh lệch lương OT|
|                                                                                                   |
+---------------------------------------------------------------------------------------------------+
```

---

## 3. KIẾN TRÚC NGHIỆP VỤ CHẤM CÔNG & TIỀN LƯƠNG (TIMESHEET & PAYROLL ENGINE)

### 3.1. Sơ đồ Luồng Nghiệp vụ Chấm Công & Tính Lương (ASCII Art)

```
[DANH SÁCH NHÂN SỰ & HỢP ĐỒNG] (HĐLĐ, Mức lương, Người phụ thuộc, Mã số thuế)
               |
               v
[BẢNG CHẤM CÔNG THÁNG] (Công chuẩn, Ngày đi làm thực tế, Nghỉ phép, Nghỉ lễ, Giờ OT)
               |
               v
[TÍNH THU NHẬP (EARNINGS)]
   ├── Lương thời gian = (Lương chính / Ngày công chuẩn) * Ngày công thực tế
   ├── Lương làm thêm giờ OT = Đơn giá giờ * (Giờ thường * 1.5 + Giờ nghỉ * 2.0 + Giờ lễ * 3.0)
   ├── Phụ cấp đóng BH (Trách nhiệm, chức vụ...)
   └── Phụ cấp miễn thuế & miễn BH (Ăn trưa <= 730k, điện thoại, trang phục <= 5M/năm)
               |
               +----------------------------------+
               |                                  |
               v                                  v
[TÍNH BẢO HIỂM & KPCĐ (DEDUCTIONS)]    [TÍNH THUẾ TNCN KHẤU TRỪ (PIT)]
   ├── Kiểm tra Sàn/Trần BHXH             ├── Thu nhập chịu thuế = Tổng TN - Thu nhập miễn thuế
   ├── DN gánh (23.5% -> Nợ 642/Có 338)   ├── Thu nhập tính thuế = TN chịu thuế - Giảm trừ gia cảnh - BH
   └── NLĐ trừ lương (10.5% -> Nợ 334/338)└── Thuế TNCN (Lũy tiến 7 bậc hoặc Khấu trừ 10%)
               |                                  |
               +----------------+-----------------+
                                |
                                v
               [LƯƠNG THỰC LĨNH = TỔNG THU NHẬP - BẢO HIỂM NLĐ - THUẾ TNCN - TẠM ỨNG]
                                |
                                v
               [TỰ ĐỘNG SINH BÚT TOÁN CORE GL TT99]
               (Nợ 642 / Có 334, Nợ 334 / Có 338, Nợ 334 / Có 3335, Nợ 334 / Có 1111, 1121)
```

---

### 3.2. Bảng Chấm Công & Ký Hiệu Chuẩn (Timesheet Types)
- **Công chuẩn tháng ($N_c$)**: Thường là 22 ngày (nghỉ thứ 7, CN) hoặc 26 ngày (nghỉ CN).
- **Ký hiệu chấm công**:
  - `+` hoặc `X`: Đi làm đủ ngày (hưởng 100% lương thời gian).
  - `P`: Nghỉ phép năm có hưởng nguyên lương (tối thiểu 12 ngày phép/năm theo Điều 113 BLLĐ 2019).
  - `L`: Nghỉ lễ, tết theo quy định Nhà nước (hưởng 100% lương theo Điều 112 BLLĐ 2019).
  - `Ro`: Nghỉ không lương (trừ lương thời gian).
  - `Om`: Nghỉ ốm đau hưởng trợ cấp BHXH (Doanh nghiệp không trả lương, BHXH chi trả theo chế độ 75% lương đóng BH).
  - `TS`: Nghỉ thai sản (BHXH chi trả 100% bình quân 6 tháng lương đóng BH).
  - `OT1`: Làm thêm giờ ngày thường.
  - `OT2`: Làm thêm giờ ngày nghỉ hàng tuần (thứ 7, CN).
  - `OT3`: Làm thêm giờ ngày lễ, tết.

---

### 3.3. Thuật toán Lương Làm Thêm Giờ (Overtime) & Tách Phần Miễn Thuế TNCN
Theo quy định tại **Điều 98 Bộ luật Lao động 2019** và **Điểm i Khoản 1 Điều 3 Thông tư 111/2013/TT-BTC**:
- Đơn giá giờ làm việc chuẩn: $R_{gio} = \frac{\text{Lương chính}}{N_c \times 8}$.
- **Tiền lương OT ngày thường**:
  $$\text{TienOT}_{thuong} = \text{GioOT}_1 \times R_{gio} \times 150\%$$
  - Phần chịu thuế TNCN (100%): $\text{GioOT}_1 \times R_{gio} \times 100\%$.
  - **Phần được miễn thuế TNCN (50% chênh lệch)**: $\text{GioOT}_1 \times R_{gio} \times 50\%$.
- **Tiền lương OT ngày nghỉ hàng tuần**:
  $$\text{TienOT}_{tuan} = \text{GioOT}_2 \times R_{gio} \times 200\%$$
  - Phần chịu thuế (100%): $\text{GioOT}_2 \times R_{gio} \times 100\%$.
  - **Phần miễn thuế (100% chênh lệch)**: $\text{GioOT}_2 \times R_{gio} \times 100\%$.
- **Tiền lương OT ngày Lễ, Tết**:
  $$\text{TienOT}_{le} = \text{GioOT}_3 \times R_{gio} \times 300\%$$
  - Phần chịu thuế (100%): $\text{GioOT}_3 \times R_{gio} \times 100\%$.
  - **Phần miễn thuế (200% chênh lệch)**: $\text{GioOT}_3 \times R_{gio} \times 200\%$.

---

### 3.4. Các Khoản Phụ Cấp & Khung Miễn Thuế TNCN / Miễn Đóng BHXH

| Khoản thu nhập | Tính chất BHXH | Tính chất Thuế TNCN | Căn cứ pháp lý |
|---|---|---|---|
| **Lương cơ bản / Lương chức danh** | Bắt buộc đóng BHXH | Chịu thuế 100% | Luật BHXH, TT 111/2013 |
| **Phụ cấp chức vụ, trách nhiệm** | Bắt buộc đóng BHXH | Chịu thuế 100% | Thông tư 10/2020/TT-BLĐTBXH |
| **Tiền ăn giữa ca / ăn trưa** | **Không đóng BHXH** | **Miễn thuế tối đa 730.000 VNĐ/tháng**. Phần vượt 730k chịu thuế TNCN. | TT 26/2016/TT-BLĐTBXH, TT 111/2013 |
| **Tiền trang phục** | **Không đóng BHXH** | **Miễn thuế tối đa 5.000.000 VNĐ/người/năm** nếu chi bằng tiền mặt. Chi bằng hiện vật miễn toàn bộ. | Khoản 2 Điều 2 Thông tư 111/2013 |
| **Tiền phụ cấp điện thoại** | **Không đóng BHXH** | **Miễn thuế theo Quy chế tài chính công ty** (có hạn mức cụ thể cho từng vị trí). | Công văn Tổng cục Thuế |
| **Tiền hỗ trợ xăng xe, đi lại** | **Không đóng BHXH** | **Chịu thuế TNCN** (trừ trường hợp là công tác phí có hóa đơn chứng từ). | TT 111/2013/TT-BTC |
| **Tiền thưởng lễ tết, thưởng KPI** | **Không đóng BHXH** | Chịu thuế 100% | Khoản 2 Điều 2 TT 111/2013 |

---

## 4. ĐẶC TẢ TRÍCH BẢO HIỂM BẮT BUỘC & KINH PHÍ CÔNG ĐOÀN (STATUTORY DEDUCTIONS)

### 4.1. Bảng Tỷ Lệ & Trần/Sàn Đóng BHXH, BHYT, BHTN, KPCĐ (Cập nhật mới nhất)

```
+-------------------------------------------------------------------------------------------+
|                          BẢNG TỶ LỆ TRÍCH THEO LƯƠNG HIỆN HÀNH                            |
+-------------------+--------------------+-------------------+------------------------------+
| Quỹ bảo hiểm      | Doanh nghiệp chịu  | Người lao động    | Căn cứ tính & Mức trần/sàn   |
+-------------------+--------------------+-------------------+------------------------------+
| BHXH (Hưu trí, TS)| 17.5% (TK 3383)    | 8.0% (Trừ lương)  | Lương đóng BH (Trần 20*MLCS) |
| BHYT (Y tế)       | 3.0%  (TK 3384)    | 1.5% (Trừ lương)  | Lương đóng BH (Trần 20*MLCS) |
| BHTN (Thất nghiệp)| 1.0%  (TK 3386)    | 1.0% (Trừ lương)  | Lương đóng BH (Trần 20*LTT Vùng)|
| KPCĐ (Công đoàn)  | 2.0%  (TK 3382)    | 0% (Đoàn viên 1%) | Lương đóng BHXH (Toàn bộ DN) |
+-------------------+--------------------+-------------------+------------------------------+
| TỔNG CỘNG TRÍCH   | 23.5% (Tính CP 642)| 10.5% (Trừ lương) |                              |
+-------------------+--------------------+-------------------+------------------------------+
```

#### Quy tắc chặn biên (Caps & Floors):
1. **Mức lương cơ sở ($L_{cs}$)**: `2.340.000 VNĐ/tháng` (Nghị định 73/2024/NĐ-CP).
   $$\text{Trần BHXH, BHYT} = 20 \times 2.340.000 = \mathbf{46.800.000\text{ VNĐ}}$$
2. **Mức lương tối thiểu vùng ($L_{ttv}$)** (Nghị định 74/2024/NĐ-CP):
   - **Vùng I**: `4.960.000 VNĐ` $\rightarrow$ Trần BHTN = $20 \times 4.960.000 = \mathbf{99.200.000\text{ VNĐ}}$.
   - **Vùng II**: `4.410.000 VNĐ` $\rightarrow$ Trần BHTN = $20 \times 4.410.000 = \mathbf{88.200.000\text{ VNĐ}}$.
   - **Vùng III**: `3.860.000 VNĐ` $\rightarrow$ Trần BHTN = $20 \times 3.860.000 = \mathbf{77.200.000\text{ VNĐ}}$.
   - **Vùng IV**: `3.450.000 VNĐ` $\rightarrow$ Trần BHTN = $20 \times 3.450.000 = \mathbf{69.000.000\text{ VNĐ}}$.
3. **Mức sàn đóng**: Lương đóng bảo hiểm của nhân sự phải thỏa mãn:
   $$\text{LuongDongBH} \ge L_{ttv}$$

---

### 4.2. Xử lý Chế độ Thai sản & Ốm đau (Mẫu C70a-HD)
- Khi nhân sự nghỉ ốm đau hoặc thai sản:
  - Doanh nghiệp **không trả lương** thời gian này (trừ khi có chế độ đãi ngộ riêng).
  - Doanh nghiệp lập danh sách C70a-HD gửi cơ quan BHXH điện tử.
  - Khi cơ quan BHXH chuyển tiền trợ cấp vào tài khoản công ty:
    $$\text{Nợ 1121 / Có 3383 (Thu hộ tiền bảo hiểm)}$$
  - Khi công ty chi trả tiền chế độ cho nhân viên:
    $$\text{Nợ 3383 / Có 1121 hoặc 1111}$$
  - **Tuyệt đối không hạch toán vào Chi phí doanh nghiệp (TK 642)** vì đây là tiền từ quỹ an sinh xã hội chi trả.

---

## 5. ĐẶC TẢ KHẤU TRỪ THUẾ THU NHẬP CÁ NHÂN (PIT WITHHOLDING ENGINE)

### 5.1. Phân loại Hợp đồng Lao động & Phương pháp Khấu trừ

```
                      [NHÂN SỰ NHẬN THU NHẬP]
                                 |
                 +---------------+---------------+
                 |                               |
        (HĐLĐ >= 3 THÁNG)               (HĐLĐ < 3 THÁNG / VÃNG LAI)
                 |                               |
                 v                               v
       [BIỂU LŨY TIẾN 7 BẬC]             Thu nhập >= 2 Triệu/lần?
                 |                               |
        Giảm trừ gia cảnh:             +---------+---------+
        - Bản thân: 11M/tháng          | (CÓ)              | (KHÔNG)
        - Phụ thuộc: 4.4M/người        v                   v
        - Trừ bảo hiểm NLĐ        Có Mẫu 08/CK-TNCN?   Khấu trừ = 0đ
                 |                     |
                 v             +-------+-------+
       Áp biểu thuế 5% - 35%   | (CÓ)          | (KHÔNG)
                               v               v
                         Tạm miễn trừ    Khấu trừ 10% tại chỗ
```

---

### 5.2. Công thức Biểu thuế Lũy tiến Từng phần (7 Bậc)

- **Thu nhập chịu thuế (TNCT)**:
  $$\text{TNCT} = \text{Tổng thu nhập} - \text{Thu nhập miễn thuế (Ăn ca, OT vượt, trang phục)}$$
- **Thu nhập tính thuế (TNTT)**:
  $$\text{TNTT} = \text{TNCT} - \text{Giảm trừ bản thân (11M)} - (\text{Số NPT} \times 4.4\text{M}) - \text{Bảo hiểm bắt buộc NLĐ (10.5\%)}$$
  *(Nếu $\text{TNTT} \le 0 \rightarrow \text{Thuế TNCN} = 0$)*

- **Bảng tính thuế rút gọn (Biểu thuế 7 bậc)**:

| Bậc | Thu nhập tính thuế/tháng ($X$) | Thuế suất | Cách tính số thuế phải nộp nhanh |
|:---:|---|:---:|---|
| **1** | Đến 5 triệu VNĐ | 5% | $0.05 \times X$ |
| **2** | Trên 5 triệu đến 10 triệu VNĐ | 10% | $0.10 \times X - 250.000$ VNĐ |
| **3** | Trên 10 triệu đến 18 triệu VNĐ | 15% | $0.15 \times X - 750.000$ VNĐ |
| **4** | Trên 18 triệu đến 32 triệu VNĐ | 20% | $0.20 \times X - 1.650.000$ VNĐ |
| **5** | Trên 32 triệu đến 52 triệu VNĐ | 25% | $0.25 \times X - 3.250.000$ VNĐ |
| **6** | Trên 52 triệu đến 80 triệu VNĐ | 30% | $0.30 \times X - 5.850.000$ VNĐ |
| **7** | Trên 80 triệu VNĐ | 35% | $0.35 \times X - 9.850.000$ VNĐ |

---

### 5.3. Khấu trừ 10% Tại Nguồn & Cam Kết Mẫu 08/CK-TNCN
- Áp dụng cho: Lao động thời vụ, thử việc, thuê ngoài, cộng tác viên không ký HĐLĐ hoặc ký HĐLĐ dưới 3 tháng.
- Nếu chi trả từ **2.000.000 VNĐ/lần trở lên**: Bắt buộc khấu trừ 10% trên tổng thu nhập chi trả.
  $$\text{ThueTNCN}_{10\%} = \text{TongThuNhap} \times 10\%$$
- **Trường hợp ngoại lệ (Cam kết Mẫu 08/CK-TNCN theo TT 80/2021/TT-BTC)**:
  - Cá nhân chỉ có duy nhất thu nhập tại 1 nơi và ước tính tổng mức thu nhập chịu thuế sau khi trừ gia cảnh chưa đến mức phải nộp thuế (dưới 132 triệu đồng/năm).
  - Cá nhân **bắt buộc phải có Mã số thuế cá nhân** tại thời điểm cam kết.
  - Khi có cam kết 08 hợp lệ, Doanh nghiệp tạm thời **không khấu trừ 10% thuế TNCN** khi chi trả.

---

## 6. HẠCH TOÁN KÉP CHUẨN THÔNG TƯ 99/2025/TT-BTC (GL DOUBLE-ENTRY POSTINGS)

Theo chế độ kế toán Thông tư 99/2025/TT-BTC, toàn bộ chi phí nhân công được theo dõi qua **Tài khoản 642 (Chi phí quản lý doanh nghiệp)**, các khoản trích theo lương theo dõi qua **Tài khoản 338**, thuế TNCN qua **Tài khoản 3335**, và thanh toán lương qua **Tài khoản 334**. **Tuyệt đối nghiêm cấm mở Tài khoản 911.**

```
                                  SƠ ĐỒ HẠCH TOÁN KÉP TIỀN LƯƠNG TT99
    TK 1111/1121                     TK 334                            TK 642
          |                             |                                 |
          |<==== (5) Chi trả lương =====|                                 |
          |      Nợ 334 / Có 1111/1121  |<====== (1) Tổng lương phải trả ==|
          |                             |        Nợ 642 / Có 334          |
          |                             |                                 |
          |                             |====== (2) BHXH NLĐ gánh =======>|  TK 338 (3383, 3384, 3386)
          |                             |       (10.5%: Nợ 334/Có 338)    |       ^
          |                             |                                 |       |
          |                             |====== (4) Thuế TNCN khấu trừ ==>|  TK 3335
          |                             |       (Nợ 334 / Có 3335)        |       |
          |                             |                                 |       |
          |                             |                                 |==== (3) BHXH & KPCĐ DN gánh ==
          |                                                               |     (23.5%: Nợ 642/Có 338)
          |<======================== (6) Nộp BHXH, BHYT, BHTN, KPCĐ =============|
          |                          Nợ 338 (3382,3383,3384,3386) / Có 1121      |
          |                                                                       |
          |<======================== (7) Nộp Thuế TNCN Nhà nước =================|
                                     Nợ 3335 / Có 1121
```

### Bộ 7 Bút toán Tiền Lương & Trích theo lương chuẩn TT99:

1. **Bút toán 1: Tính tiền lương và phụ cấp phải trả trong tháng**:
   - Nợ TK 642 - Chi phí quản lý doanh nghiệp (Tổng lương & phụ cấp chịu tính chi phí).
   - Có TK 334 - Phải trả người lao động.

2. **Bút toán 2: Trích Bảo hiểm và KPCĐ phần Doanh nghiệp chịu tính vào chi phí (23.5%)**:
   - Nợ TK 642 - Chi phí quản lý doanh nghiệp (Tổng 23.5% lương đóng BH).
     - Có TK 3383 - Bảo hiểm xã hội (17.5%).
     - Có TK 3384 - Bảo hiểm y tế (3.0%).
     - Có TK 3386 - Bảo hiểm thất nghiệp (1.0%).
     - Có TK 3382 - Kinh phí công đoàn (2.0%).

3. **Bút toán 3: Trích Bảo hiểm phần Người lao động chịu khấu trừ vào lương (10.5%)**:
   - Nợ TK 334 - Phải trả người lao động (Tổng 10.5% lương đóng BH).
     - Có TK 3383 - Bảo hiểm xã hội (8.0%).
     - Có TK 3384 - Bảo hiểm y tế (1.5%).
     - Có TK 3386 - Bảo hiểm thất nghiệp (1.0%).

4. **Bút toán 4: Khấu trừ Thuế Thu nhập cá nhân tại nguồn**:
   - Nợ TK 334 - Phải trả người lao động.
   - Có TK 3335 - Thuế thu nhập cá nhân.

5. **Bút toán 5: Chi trả lương thực lĩnh cho người lao động (Tiền mặt / Chuyển khoản)**:
   - Nợ TK 334 - Phải trả người lao động.
   - Có TK 1121 - Tiền Việt Nam gửi ngân hàng (Chuyển khoản qua UNC Phase 3).
   - Có TK 1111 - Tiền mặt tại quỹ (Chi qua Phiếu chi Phase 3 - Kiểm tra không âm quỹ).

6. **Bút toán 6: Nộp tiền Bảo hiểm và Kinh phí công đoàn cho Cơ quan BHXH / Công đoàn**:
   - Nợ TK 3383 (BHXH 25.5%).
   - Nợ TK 3384 (BHYT 4.5%).
   - Nợ TK 3386 (BHTN 2.0%).
   - Nợ TK 3382 (KPCĐ 2.0%).
   - Có TK 1121 - Tiền gửi ngân hàng.

7. **Bút toán 7: Nộp Thuế TNCN vào Ngân sách Nhà nước**:
   - Nợ TK 3335 - Thuế thu nhập cá nhân.
   - Có TK 1121 - Tiền gửi ngân hàng.

---

## 7. MÔ HÌNH THỰC THỂ CSDL (ENTITY RELATIONSHIP & MULTI-DB SCHEMA)

### 7.1. Entity `NhanVien` (Nhân sự)
```csharp
public class NhanVien
{
    public long Id { get; set; }
    public string MaNhanVien { get; set; } = string.Empty; // Unique
    public string HoTen { get; set; } = string.Empty;
    public string SoCccd { get; set; } = string.Empty;
    public string? MaSoThue { get; set; } // Mã số thuế cá nhân (Bắt buộc nếu làm Cam kết 08)
    public string? SoSoBhxh { get; set; }
    public string? PhongBan { get; set; }
    public string? ChucVu { get; set; }
    public LoaiHopDongLaoDong LoaiHopDong { get; set; } // DaiHan >= 3T, ThuViec, ThoiVu
    public DateTime NgayVaoLam { get; set; }
    public DateTime? NgayKetThucHd { get; set; }
    
    // Mức lương & Chế độ
    public decimal LuongCoBan { get; set; } // Lương ký hợp đồng
    public decimal LuongDongBaoHiem { get; set; } // Lương căn cứ đóng BHXH
    public decimal PhuCapAnTrua { get; set; } // Miễn thuế <= 730k
    public decimal PhuCapTrachNhiem { get; set; } // Đóng BHXH + chịu thuế
    public decimal PhuCapDienThoai { get; set; } // Miễn thuế theo quy chế
    public decimal PhuCapTrangPhuc { get; set; } // Miễn thuế <= 5M/năm
    public int SoNguoiPhuThuoc { get; set; } = 0; // Đã đăng ký MST NPT
    public bool CoCamKet08 { get; set; } = false; // Áp dụng cho thời vụ < 3T
    public bool DongBaoHiem { get; set; } = true;
    public bool LaDoanVienCongDoan { get; set; } = false;
    
    // Tài khoản nhận lương
    public string? SoTaiKhoanNganHang { get; set; }
    public string? TenNganHang { get; set; }
    public bool DangLamViec { get; set; } = true;
}
```

### 7.2. Entity `BangChamCongThang` & `ChiTietChamCong`
```csharp
public class BangChamCongThang
{
    public long Id { get; set; }
    public string KyKeToan { get; set; } = string.Empty; // YYYY-MM
    public int Nam { get; set; }
    public int Thang { get; set; }
    public int SoNgayCongChuan { get; set; } = 22; // 22 hoặc 26 ngày
    public TrangThaiChamCong TrangThai { get; set; } // DangCham, DaChot, DaKhoa
    public virtual ICollection<ChiTietChamCong> ChiTiets { get; set; }
}

public class ChiTietChamCong
{
    public long Id { get; set; }
    public long BangChamCongThangId { get; set; }
    public long NhanVienId { get; set; }
    public decimal SoNgayDiLam { get; set; } // Đi làm thực tế
    public decimal SoNgayNghiPhep { get; set; } // Hưởng 100% lương
    public decimal SoNgayNghiLe { get; set; } // Hưởng 100% lương
    public decimal SoNgayNghiKhongLuong { get; set; }
    public decimal SoNgayNghiOmBhxh { get; set; } // BHXH trả
    public decimal SoNgayNghiThaiSan { get; set; } // BHXH trả
    public decimal GioLamThemNgayThuong { get; set; } // OT 150%
    public decimal GioLamThemNgayNghi { get; set; } // OT 200%
    public decimal GioLamThemNgayLe { get; set; } // OT 300%
    public decimal TongCongTinhLuong { get; set; } // = DiLam + NghiPhep + NghiLe
}
```

### 7.3. Entity `BangLuongThang` & `ChiTietLuongNhanVien`
```csharp
public class BangLuongThang
{
    public long Id { get; set; }
    public string SoChungTu { get; set; } = string.Empty; // BL-YYYY-MM
    public string KyKeToan { get; set; } = string.Empty; // YYYY-MM
    public DateTime NgayLap { get; set; }
    public int SoNgayCongChuan { get; set; } = 22;
    
    // Tổng hợp tài chính
    public decimal TongQuyLuong { get; set; } // Nợ 642 / Có 334
    public decimal TongBaoHiemDnGanh { get; set; } // Nợ 642 / Có 338 (23.5%)
    public decimal TongBaoHiemNldGanh { get; set; } // Nợ 334 / Có 338 (10.5%)
    public decimal TongThueTncn { get; set; } // Nợ 334 / Có 3335
    public decimal TongThucLinh { get; set; } // Nợ 334 / Có 1111, 1121
    
    // Liên kết Sổ Cái Core GL
    public long? ButToanChiPhiLuongId { get; set; } // Bút toán 1: Nợ 642 / Có 334
    public long? ButToanBaoHiemDnId { get; set; } // Bút toán 2: Nợ 642 / Có 338
    public long? ButToanKhauTruLuongId { get; set; } // Bút toán 3+4: Nợ 334 / Có 338, 3335
    
    public TrangThaiBangLuong TrangThai { get; set; } // ChoDuyet, DaDuyet, DaGhiSo, DaChiTra
    public virtual ICollection<ChiTietLuongNhanVien> ChiTiets { get; set; }
}

public class ChiTietLuongNhanVien
{
    public long Id { get; set; }
    public long BangLuongThangId { get; set; }
    public long NhanVienId { get; set; }
    
    // Thu nhập
    public decimal LuongThoiGian { get; set; }
    public decimal LuongLamThemGio { get; set; }
    public decimal LuongOtMienThue { get; set; } // Phần chênh lệch 50%, 100%, 200%
    public decimal PhuCapChiuThue { get; set; }
    public decimal PhuCapMienThue { get; set; } // Ăn trưa, trang phục...
    public decimal TienThuong { get; set; }
    public decimal TongThuNhap { get; set; }
    
    // Bảo hiểm trích trừ lương NLĐ (10.5%)
    public decimal LuongDongBaoHiem { get; set; }
    public decimal BhxhNld { get; set; } // 8%
    public decimal BhytNld { get; set; } // 1.5%
    public decimal BhtnNld { get; set; } // 1%
    public decimal TongBaoHiemNld { get; set; }
    
    // Bảo hiểm DN gánh tính vào CP 642 (23.5%)
    public decimal BhxhDn { get; set; } // 17.5%
    public decimal BhytDn { get; set; } // 3%
    public decimal BhtnDn { get; set; } // 1%
    public decimal KpcdDn { get; set; } // 2%
    public decimal TongBaoHiemDn { get; set; }
    
    // Thuế TNCN
    public decimal GiamTruBanThan { get; set; } = 11000000m;
    public decimal GiamTruNguoiPhuThuoc { get; set; }
    public decimal ThuNhapTinhThue { get; set; }
    public decimal ThueTncnKhauTru { get; set; }
    
    // Giảm trừ khác & Thực lĩnh
    public decimal TamUng { get; set; }
    public decimal ThucLinh { get; set; }
}
```

---

## 8. THIẾT KẾ GIAO DIỆN NGƯỜI DÙNG & PHIẾU LƯƠNG (UI/UX WIREFRAMES)

### 8.1. Wireframe Bảng Tính Lương Tháng (`Views/TienLuong/BangLuong.cshtml`)

```
+-----------------------------------------------------------------------------------------------------------------------------------------+
| ninjaTax - BẢNG TÍNH LƯƠNG & BẢO HIỂM THÁNG 09/2026                                                                    [IN BẢNG LƯƠNG]   |
| Số chứng từ: BL-2026-09 | Công chuẩn: 22 ngày | Trạng thái: [ĐÃ DUYỆT - SẴN SÀNG GHI SỔ GL]               [DUYỆT & GHI SỔ GL]   |
+-----------------------------------------------------------------------------------------------------------------------------------------+
| TỔNG QUỸ LƯƠNG:        | BẢO HIỂM DN GÁNH (23.5%): | BẢO HIỂM NLĐ TRỪ (10.5%): | THUẾ TNCN KHẤU TRỪ:    | TỔNG THỰC LĨNH CHI TRẢ:       |
| 350.000.000 đ          | 58.750.000 đ              | 26.250.000 đ              | 14.500.000 đ          | 309.250.000 đ                 |
+-----------------------------------------------------------------------------------------------------------------------------------------+
| STT | Mã NV  | Họ và tên     | HĐLĐ   | Lương HĐ   | Công | Tổng TN     | BHXH (8%) | BHYT(1.5%)| BHTN(1%) | Giảm trừ | Thuế TNCN | Thực lĩnh   |
+-----+--------+---------------+--------+------------+------+-------------+-----------+-----------+----------+----------+-----------+-------------+
| 1   | NV001  | Nguyễn Văn An | Dài hạn| 50.000.000 | 22/22| 52.500.000  | 3.744.000*|   702.000*|  496.000*| 19.800.00| 3.250.000 | 44.308.000  |
|     |        |               |        |            |      | (*Chạm trần)|(Trần 46.8M|(Trần 46.8M| (Trần99M)|(11M+2NPT)|(Lũy tiến) |             |
| 2   | NV002  | Trần Thị Bình | Dài hạn| 15.000.000 | 22/22| 16.500.000  | 1.200.000 |   225.000 |  150.000 | 11.000.00|   392.500 | 14.532.500  |
| 3   | NV003  | Lê Văn Cường  | Thời vụ|  8.000.000 | 15/22|  8.000.000  |         0 |         0 |        0 |        0 |   800.000 |  7.200.000  |
|     |        |               |        |            |      | (Kèm cam kết|           |           |          |          |  (10% TN) |             |
+-----+--------+---------------+--------+------------+------+-------------+-----------+-----------+----------+----------+-----------+-------------+
|     | TỔNG CỘNG               |        |            |      | 350.000.000 |           |           |          |          | 14.500.000| 309.250.000 |
+-----------------------------------------------------------------------------------------------------------------------------------------+
```

---

### 8.2. Wireframe Mẫu Phiếu Lương Cá Nhân Điện Tử (Payslip)

```
+-------------------------------------------------------------------------+
|                  CÔNG TY TNHH NINJATAX VIỆT NAM                         |
|                   PHIẾU LƯƠNG THÁNG 09/2026                             |
+-------------------------------------------------------------------------+
| Mã nhân viên: NV001                | Họ và tên: NGUYỄN VĂN AN           |
| Chức vụ: Giám đốc Công nghệ        | Phòng ban: Khối Kỹ Thuật           |
| Số ngày công thực tế: 22 / 22      | Số người phụ thuộc: 02 người       |
+------------------------------------+------------------------------------+
| 1. CÁC KHOẢN THU NHẬP (VNĐ)        | 2. CÁC KHOẢN TRÍCH TRỪ (VNĐ)       |
+------------------------------------+------------------------------------+
| - Lương chính hợp đồng: 50.000.000 | - BHXH (8.0% - Trần 46.8M): 3.744.000|
| - Phụ cấp ăn trưa:         730.000 | - BHYT (1.5% - Trần 46.8M):   702.000|
| - Phụ cấp điện thoại:    1.000.000 | - BHTN (1.0%):                496.000|
| - Lương làm thêm giờ OT:   770.000 | - Thuế TNCN khấu trừ:       3.250.000|
|   (Trong đó miễn thuế:     256.667)| - Tạm ứng trong kỳ:                 0|
+------------------------------------+------------------------------------+
| TỔNG THU NHẬP:         52.500.000  | TỔNG CÁC KHOẢN TRỪ:         8.192.000|
+------------------------------------+------------------------------------+
| THỰC LĨNH CHUYỂN KHOẢN:                                 44.308.000 VNĐ  |
| (Bằng chữ: Bốn mươi bốn triệu ba trăm linh tám nghìn đồng chẵn)          |
| Tài khoản nhận: 00110099887766 - Vietcombank                            |
+-------------------------------------------------------------------------+
```

---

## 9. BỘ KỊCH BẢN KIỂM THỬ TDD (TEST-DRIVEN DEVELOPMENT SPECS)

### 9.1. Kịch bản Kiểm thử Bảo Hiểm Bắt Buộc (`StatutoryInsuranceTests.cs`)
- **Test 1 (`ApTranBaoHiem_Luong100M_CapDung20LanLuongCoSo`)**:
  - Input: Nhân sự lương 100.000.000 VNĐ. Lương cơ sở 2.340.000 VNĐ. Lương tối thiểu Vùng 1 là 4.960.000 VNĐ.
  - Expected:
    - Lương tính BHXH/BHYT = 46.800.000 VNĐ. NLĐ nộp BHXH 8% = 3.744.000 VNĐ; BHYT 1.5% = 702.000 VNĐ.
    - Lương tính BHTN = 99.200.000 VNĐ. NLĐ nộp BHTN 1% = 992.000 VNĐ.
    - DN nộp BHXH 17.5% = 8.190.000 VNĐ; BHYT 3% = 1.404.000 VNĐ; BHTN 1% = 992.000 VNĐ; KPCĐ 2% = 936.000 VNĐ.
- **Test 2 (`ChanLuongDongBaoHiem_DuoiMucLuongToiThieuVung_ThrowsException`)**:
  - Input: Khai báo lương đóng BHXH 4.000.000 VNĐ ở Vùng 1 (Sàn 4.960.000 VNĐ).
  - Expected: Ném `InvalidOperationException` từ chối lưu.

### 9.2. Kịch bản Kiểm thử Thuế TNCN (`PersonalIncomeTaxTests.cs`)
- **Test 3 (`TinhThueLuyTien7Bac_ChuanXacTungDong`)**:
  - Input: HĐLĐ dài hạn, Lương 30.000.000 VNĐ, 1 người phụ thuộc (4.4M), Giảm trừ bản thân (11M), Bảo hiểm trừ lương (3.150.000 VNĐ).
  - Thu nhập tính thuế = $30.000.000 - 11.000.000 - 4.400.000 - 3.150.000 = 11.450.000$ VNĐ (Bậc 3).
  - Thuế bậc 3: $11.450.000 \times 15\% - 750.000 = \mathbf{967.500}$ VNĐ.
  - Expected: Số thuế TNCN khấu trừ = 967.500 VNĐ.
- **Test 4 (`KhauTru10PhanTram_HopDongThoiVu_KhongCamKet08`)**:
  - Input: Cộng tác viên lương 10.000.000 VNĐ/lần, không ký HĐLĐ dài hạn, không có Cam kết 08.
  - Expected: Khấu trừ đúng $10\% = 1.000.000$ VNĐ. Thực lĩnh = 9.000.000 VNĐ.
- **Test 5 (`TachLuongOvertime_PhanVuotMienThueTncn`)**:
  - Input: Nhân sự làm thêm giờ ngày Chủ nhật hưởng 200% lương, tổng tiền OT là 2.000.000 VNĐ.
  - Expected: 1.000.000 VNĐ chịu thuế, 1.000.000 VNĐ (chênh lệch 100%) được đưa vào thu nhập miễn thuế TNCN.

### 9.3. Kịch bản Kiểm thử Hạch toán Sổ Cái Core GL (`PayrollPostingGlTests.cs`)
- **Test 6 (`SinhButToanLuongKhepKin_CanDoiTongNoTongCo_TuyetDoiCam911`)**:
  - Chạy ghi sổ bảng lương tháng:
    - Bút toán 1: Nợ 642 / Có 334.
    - Bút toán 2: Nợ 642 / Có 3383, 3384, 3386, 3382.
    - Bút toán 3: Nợ 334 / Có 3383, 3384, 3386.
    - Bút toán 4: Nợ 334 / Có 3335.
  - Expected: Mọi bút toán sinh ra đều có `TongNo == TongCo`, `TrangThai == DaGhiSo`, và trong toàn bộ hệ thống không có bất kỳ dòng nào liên quan đến TK 911.

---

## 10. LỘ TRÌNH TRIỂN KHAI CHI TIẾT (IMPLEMENTATION ROADMAP)

### Giai đoạn 1: Thiết kế Thực thể & Migration CSDL (Entities & Multi-DB EF Core)
- [ ] Tạo `NhanVien.cs`, `BangChamCongThang.cs`, `ChiTietChamCong.cs`.
- [ ] Tạo `BangLuongThang.cs`, `ChiTietLuongNhanVien.cs`.
- [ ] Cấu hình Fluent API `AppDbContext.cs`, thiết lập độ chính xác `decimal(19,4)` và kiểu `TEXT` cho SQLite.
- [ ] Chạy EF Core migration `AddPayrollAndTaxModules`.

### Giai đoạn 2: Lập trình Dịch vụ Nghiệp vụ Lõi (Payroll & Tax Engines)
- [ ] `ITimesheetService.cs` & `TimesheetService.cs`: Chấm công, tính tổng công, tính giờ OT ngày thường/nghỉ/lễ.
- [ ] `IPayrollService.cs` & `PayrollService.cs`:
  - Thuật toán trần/sàn BHXH, BHYT, BHTN, KPCĐ.
  - Thuật toán bóc tách thu nhập miễn thuế (Ăn trưa $\le 730k$, trang phục $\le 5M$, OT chênh lệch).
  - Thuật toán biểu thuế lũy tiến từng phần 7 bậc và khấu trừ 10% vãng lai.
  - Sinh 4 bút toán kế toán Sổ cái GL tự động cân đối `TongNo == TongCo`, nghiêm cấm TK 911.
- [ ] Tích hợp chi trả lương qua Phiếu chi / Ủy nhiệm chi của Phase 3 (`IThuChiService`).

### Giai đoạn 3: Xây dựng Giao diện Người dùng (Razor UI & Payslip)
- [ ] `Controllers/NhanVienController.cs` & Views: Quản lý hồ sơ nhân sự, mức lương, người phụ thuộc, hợp đồng.
- [ ] `Controllers/ChamCongController.cs` & Views: Bảng chấm công dạng lưới ma trận tháng, cập nhật công/phép/OT.
- [ ] `Controllers/TienLuongController.cs` & Views:
  - Bảng lương tổng hợp tháng, chạy tính lương tự động từ chấm công.
  - Chi tiết phiếu lương từng nhân sự (In Payslip cá nhân).
  - Duyệt và ghi sổ đồng loạt vào Sổ cái Core GL.
- [ ] Cập nhật menu điều hướng `_Layout.cshtml`.

### Giai đoạn 4: Bộ Kiểm thử TDD Toàn diện & Xác thực Quyết toán
- [ ] Viết test suite `StatutoryInsuranceTests.cs` (Test trần 20 lần MLCS, trần BHTN vùng, sàn lương).
- [ ] Viết test suite `PersonalIncomeTaxTests.cs` (Test biểu 7 bậc, giảm trừ gia cảnh, khấu trừ 10%, OT miễn thuế).
- [ ] Viết test suite `PayrollPostingGlTests.cs` (Test hạch toán kép TT99, cân đối nợ có, không dùng TK 911).
- [ ] Chạy `dotnet test` bảo đảm 100% test cases pass.
- [ ] Git commit và push remote `origin/master`.
