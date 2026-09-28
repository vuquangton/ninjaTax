# ĐẶC TẢ NGHIỆP VỤ & KỸ THUẬT PHÂN HỆ SỔ CÁI TỔNG HỢP (GENERAL LEDGER - GL)
## CHUẨN THÔNG TƯ 99/2025/TT-BTC & VAS/IFRS CHO DOANH NGHIỆP VIỆT NAM (ninjaTax)

- **Hệ thống**: `ninjaTax` (ASP.NET Core .NET 10, Multi-DB MariaDB/SQLite/PostgreSQL/SQL Server)
- **Tác giả thẩm định**: BA Lead (20+ năm ERP/Fintech) & Kế toán trưởng Doanh nghiệp (20+ năm thực chiến VAS/TT99/Thanh tra Thuế)
- **Cơ sở pháp lý**: Thông tư 99/2025/TT-BTC, Luật Quản lý Thuế 38/2019/QH14, TT 80/2021/TT-BTC, NĐ 123/2020/NĐ-CP, NĐ 91/2022/NĐ-CP.

---

## 🛑 PHẦN 1: ĐÁNH GIÁ SẴN SÀNG PRODUCTION (PROD ENV READINESS AUDIT)

### Câu hỏi: *Phân hệ General Ledger (GL) hiện tại có thể chạy trực tiếp trên PROD ENV được không?*
### Kết luận: **KHÔNG ĐƯỢC PHÉP CHẠY TRỰC TIẾP TRÊN PRODUCTION NGAY LẬP TỨC (RISK LEVEL: CRITICAL)**

### 7 Lỗ hổng Chí mạng & Rủi ro Pháp lý:
1. **Thiếu cơ chế Đóng sổ Kỳ kế toán & Khóa sổ đa chiều (Fiscal Period Lock & Hard Freeze)**:
   - Sổ cái hiện tại chỉ có trạng thái `TrangThaiButToan.DaGhiSo` và `BoGhiSo`. Người dùng vẫn có thể sửa/xóa/hạch toán lùi ngày (back-dating) vào kỳ đã nộp BCTC/Quyết toán thuế.
   - *Hậu quả*: Vi phạm Điều 13 Luật Kế toán 88/2015/QH13, sai lệch số liệu lũy kế báo cáo tài chính đã nộp cho Cơ quan Thuế.
2. **Thiếu Bút toán Tự động Cuối kỳ (Automated Year-End & Month-End Closing Engine)**:
   - Quy tắc TT99: **Nghiêm cấm dùng TK 911**. Kết chuyển toàn bộ TK Doanh thu (511, 515, 711) và Chi phí (632, 635, 641, 642, 811, 821) thẳng vào TK 4212 (hoặc 4211 cho năm trước).
   - Hiện tại hệ thống chỉ tính "ảo" trên view B02 mà chưa có transaction kết chuyển ghi sổ thật trong database GL (`ChiTietButToan`).
   - *Hậu quả*: Bảng cân đối tài khoản (Trial Balance) cuối năm không đóng về 0 cho các tài khoản loại 5, 6, 7, 8; số dư TK 4212 trên sổ cái không khớp với BCTC.
3. **Thiếu Cơ chế Đánh giá lại Ngoại tệ Cuối kỳ (FX Revaluation Engine - TT99/VAS 10)**:
   - Chưa tự động revalue số dư các tài khoản tiền tệ có gốc ngoại tệ (TK 1112, 1122, 131, 331) theo tỷ giá mua/bán của Ngân hàng thương mại nơi DN mở tài khoản tại ngày 31/12, định khoản qua TK 413 (Chênh lệch tỷ giá hối đoái).
4. **Thiếu Báo cáo Sổ Cái & Sổ Nhật ký Chung chuẩn in pháp lý (S03a-DN, S03b-DN)**:
   - Hệ thống mới chỉ có màn hình danh sách bút toán thô và B01/B02/B03. Thiếu Sổ Nhật ký chung S03a-DN, Sổ Cái S03b-DN, Bảng Cân đối Số phát sinh (Trial Balance 8 cột) theo quy định kiểm tra sổ sách kế toán của Đoàn thanh tra thuế.
5. **Thiếu Đối soát Đa phân hệ với Sổ Cái (Subledger to GL Reconciliation & Integrity Check)**:
   - Chưa có engine chốt số kiểm tra tự động giữa Tổng hợp Công nợ Khách hàng (AR Subledger) vs TK 131, Nhà cung cấp (AP Subledger) vs TK 331, Sổ NXT Kho (Inventory S10-DN) vs TK 152/155/156, Bảng Khấu hao TSCĐ vs TK 211/214.
6. **Thiếu Audit Trail & Không thể Xóa vết (Immutability & Tamper-Evident Journal)**:
   - Thiếu ghi nhận `AuditLog` (ai sửa, sửa lúc nào, giá trị cũ/mới, lý do đảo sổ).
   - Với bút toán đã khóa sổ, chuẩn mực kế toán yêu cầu không được UPDATE trực tiếp mà phải phát hành Bút toán Đảo (Reversal Voucher) hoặc Bút toán Điều chỉnh (Adjustment Voucher).
7. **Thiếu Đánh số Chứng từ Tự động theo Quy tắc Chuẩn & Khóa đồng thời (Voucher Numbering Concurrency)**:
   - Cần cơ chế cấp số chứng từ liên tục `PK-YYYYMM-XXXXX` chống trùng lặp dưới môi trường multi-user/multi-branch bằng Atomic Sequence hoặc Table Lock.

---

## 🏗️ PHẦN 2: ĐẶC TẢ YÊU CẦU NGHIỆP VỤ (BRD) & QUY TẮC BẮT BUỘC TT99

```
+----------------------------------------------------------------------------------------------------+
|                                    SƠ ĐỒ TRỌNG TÂM GENERAL LEDGER (TT99)                           |
+====================================================================================================+
|                                                                                                    |
|   +--------------------+     +--------------------+     +--------------------+                     |
|   | Phân hệ Bán Hàng   |     | Phân hệ Mua Hàng   |     | Phân hệ Kho/TSCĐ   |                     |
|   | AR Subledger (131) |     | AP Subledger (331) |     | Inventory (152/156)|                     |
|   +---------+----------+     +---------+----------+     +---------+----------+                     |
|             |                          |                          |                                |
|             +--------------------------+--------------------------+                                |
|                                        | (Real-time Posting)                                       |
|                                        v                                                           |
|                          +---------------------------+                                             |
|                          |    SỔ NHẬT KÝ CHUNG       |                                             |
|                          | (Double Entry: Nợ == Có)  |                                             |
|                          |  * CẤM TÀI KHOẢN 911 *    |                                             |
|                          +-------------+-------------+                                             |
|                                        |                                                           |
|                     +------------------+------------------+                                        |
|                     v                                     v                                        |
|        +-------------------------+           +-------------------------+                           |
|        | Bút toán Định kỳ Tháng  |           | Bút toán Kết chuyển Năm |                           |
|        | - Phân bổ CCDC (242)    |           | - DT (511,515,711)->4212|                           |
|        | - Khấu hao TSCĐ (214)   |           | - CP (632,642,811)->4212|                           |
|        | - Đánh giá ngoại tệ 413 |           | - Thuế TNDN 8211 -> 4212|                           |
|        +------------+------------+           +------------+------------+                           |
|                     |                                     |                                        |
|                     +------------------+------------------+                                        |
|                                        |                                                           |
|                                        v                                                           |
|                          +---------------------------+                                             |
|                          | KIỂM TRA ĐỐI SOÁT TỰ ĐỘNG |                                             |
|                          | Subledger vs GeneralLedger|                                             |
|                          +-------------+-------------+                                             |
|                                        |                                                           |
|                     +------------------+------------------+                                        |
|                     v                                     v                                        |
|        +-------------------------+           +-------------------------+                           |
|        | BẢNG CÂN ĐỐI PHÁT SINH  |           | BỘ BÁO CÁO TÀI CHÍNH    |                           |
|        |    (Trial Balance 8 Cột)|           | - B01-DN (Tình hình TC) |                           |
|        |    SỔ CÁI TỔNG HỢP      |           | - B02-DN (KQKD)         |                           |
|        |    (Mẫu S03b-DN)        |           | - B03-DN (Lưu chuyển TT)|                           |
|        +-------------------------+           +-------------------------+                           |
|                                                                                                    |
+----------------------------------------------------------------------------------------------------+
```

### 1. Invariant Bất biến trong TT99 & VAS:
- **Cấm Tuyệt đối TK 911**: Tất cả luồng kết chuyển doanh thu chi phí hạch toán trực tiếp vào `TK 4212`.
- **Cân đối Nợ - Có**: $\sum \text{Nợ} \equiv \sum \text{Có}$ trên từng chứng từ (`TongNo == TongCo`).
- **Khóa sổ Kỳ (Period Locking)**: Không được phép ghi sổ, sửa đổi hoặc hủy ghi sổ vào các tháng/năm có `TrangThaiKhoaSo == DaKhoaSo`.
- **Bóc tách Lưỡng tính**: TK 131, 331, 333, 421 không được cấn trừ bù trừ số dư chéo giữa các pháp nhân đối tượng khác nhau khi lên Bảng Cân đối và Sổ cái chi tiết.

---

## ⚙️ PHẦN 3: LUỒNG NGHIỆP VỤ & STATE MACHINE (WORKFLOWS & STATE MACHINE)

### 1. State Machine: Vòng đời Chứng từ & Kỳ Kế toán Sổ Cái

```
[Mới tạo: ChuaGhiSo] ---> (Kiểm tra hợp lệ & Double-entry) ---> [DaGhiSo]
         |                                                          |
         +<----------- (Hủy ghi sổ / Mở khóa nếu kỳ mở) <-----------+
         |                                                          |
         v (Xóa chứng từ nháp)                                      v (Khóa sổ tháng/năm)
     [Đã Xóa]                                                [ĐÃ KHÓA SỔ (HARD LOCK)]
                                                                    |
                                                     (Chỉ được tạo Bút toán Đảo / Điều chỉnh)
                                                                    v
                                                            [Bút toán Điều chỉnh]
```

### 2. Bảng Chuyển trạng thái (State Transition Table)

| Trạng thái hiện tại | Sự kiện / Hành động | Điều kiện tiên quyết | Trạng thái kế tiếp | Hành vi hệ thống |
|---|---|---|---|---|
| `ChuaGhiSo` | Lưu chứng từ | `TongNo == TongCo`, không có TK 911 | `ChuaGhiSo` | Lưu nháp thành công |
| `ChuaGhiSo` | Ghi sổ (`GhiSo`) | Kỳ kế toán chưa khóa, TK con hợp lệ | `DaGhiSo` | Cập nhật số dư Sổ cái, ghi AuditLog |
| `DaGhiSo` | Bỏ ghi sổ (`BoGhiSo`) | Kỳ kế toán chưa khóa, BCTC chưa khóa | `ChuaGhiSo` | Trừ số dư Sổ cái, ghi AuditLog |
| `DaGhiSo` | Khóa kỳ kế toán | Đã chạy kết chuyển cuối kỳ, không lệch Nợ-Có | `DaKhoaSo` | Đóng băng toàn bộ chứng từ trong kỳ |
| `DaKhoaSo` | Yêu cầu sửa | Bị chặn tuyệt đối | `DaKhoaSo` | Yêu cầu tạo chứng từ điều chỉnh (Adjustment Voucher) |

---

## 📊 PHẦN 4: USE CASES CHI TIẾT

### UC-GL-01: Lập và Ghi sổ Bút toán Tổng hợp (General Journal Voucher)
- **Actor**: Kế toán Tổng hợp / Kế toán viên.
- **Preconditions**: Tài khoản hạch toán là tài khoản chi tiết cấp cuối cùng, đang hoạt động.
- **Main Flow**:
  1. Người dùng chọn ngày hạch toán, ngày chứng từ, diễn giải.
  2. Thêm các dòng định khoản Nợ / Có, chọn đối tượng (nếu là TK công nợ), chọn bộ phận (Cost Center).
  3. Hệ thống kiểm tra: Tổng Nợ == Tổng Có, Không chứa TK 911, Ngày hạch toán nằm trong kỳ mở.
  4. Người dùng nhấn Ghi sổ (hoặc bấm F9).
  5. Hệ thống ghi nhận vào `ButToan` và `ChiTietButToan`, trạng thái chuyển `DaGhiSo`.
- **Alternative Flow**: Người dùng chọn lưu nháp (Ctrl+S) -> Trạng thái giữ `ChuaGhiSo`.
- **Exception Flow**:
  - Lệch Nợ - Có: Hệ thống hiển thị Toast lỗi "Tổng Nợ phải bằng Tổng Có (Chênh lệch: X đ)".
  - Chứa TK 911: Hệ thống chặn ngay tại tầng giao diện và Service với cảnh báo vi phạm Thông tư 99.
  - Ngày rơi vào kỳ đã khóa sổ: Báo lỗi "Kỳ kế toán [Tháng/Năm] đã khóa sổ, không được ghi thêm nghiệp vụ".

### UC-GL-02: Kết chuyển Tự động Doanh thu & Chi phí Cuối kỳ (Period-End Closing Voucher)
- **Actor**: Kế toán trưởng.
- **Preconditions**: Toàn bộ chứng từ trong kỳ đã ghi sổ (`DaGhiSo`).
- **Main Flow**:
  1. Kế toán trưởng chọn Kỳ kết chuyển (Tháng X hoặc Năm Y).
  2. Nhấn "Tạo Bút toán Kết chuyển Tự động".
  3. Hệ thống quét toàn bộ số dư phát sinh:
     - Nợ TK 511, 515, 711 / Có TK 4212 (Kết chuyển Doanh thu & Thu nhập).
     - Nợ TK 4212 / Có TK 632, 635, 641, 642, 811 (Kết chuyển Chi phí).
     - Nợ TK 8211 / Có TK 3334 và Nợ TK 4212 / Có TK 8211 (Kết chuyển Thuế TNDN).
  4. Tự động sinh chứng từ kết chuyển `KC-YYYYMM-001`, ghi sổ tự động vào Sổ cái.

### UC-GL-03: Khóa Sổ Kỳ Kế toán (Fiscal Period Freeze)
- **Actor**: Kế toán trưởng / Quản trị viên.
- **Preconditions**: Đã chạy kiểm tra đối soát (Subledger vs GL) không có chênh lệch.
- **Main Flow**:
  1. Chọn Tháng/Năm cần khóa.
  2. Hệ thống kiểm tra 3 điều kiện: (1) Toàn bộ chứng từ đã ghi sổ; (2) Không có tài khoản doanh thu/chi phí còn số dư; (3) Tổng Nợ == Tổng Có trên Trial Balance.
  3. Xác nhận khóa sổ -> Cập nhật trạng thái `CauHinhKeToan.NgayKhoaSo` = ngày cuối kỳ.
  4. Hệ thống khóa quyền tạo/sửa/xóa đối với mọi nghiệp vụ có ngày <= Ngày khóa sổ.

---

## 🖥️ PHẦN 5: GIAO DIỆN NGƯỜI DÙNG & WIREFRAMES (UI/UX ASCII ART)

### 1. Wireframe: Sổ Cái Tổng Hợp & Tra Cứu Phát Sinh (S03b-DN)

```
+======================================================================================================+
| ninjaTax TT99 | [Dashboard] [Mua Hàng] [Bán Hàng] [Kho] [Thu/Chi] [TSCĐ] [Lương] | [SỔ CÁI TỔNG HỢP]  |
+======================================================================================================+
| KỲ BÁO CÁO: [ Tháng 12/2026 v ]  TÀI KHOẢN: [ 1121 - Tiền gửi ngân hàng v ]   BỘ PHẬN: [ Tất cả v ]   |
| TỪ NGÀY: [ 01/12/2026 ]  ĐẾN NGÀY: [ 31/12/2026 ]   [ Tra Cứu ]  [ Xuất Excel ]  [ In Sổ S03b-DN ]   |
+------------------------------------------------------------------------------------------------------+
| SỐ DƯ ĐẦU KỲ:  NỢ: 1.500.000.000 đ                      CÓ: 0 đ                                      |
+------------------------------------------------------------------------------------------------------+
| [ AG GRID TABLE: SỔ CÁI TÀI KHOẢN 1121 ]                                                             |
| +----+------------+------------+-----------------------+--------+---------------+---------------+    |
| | STT| NGÀY HT    | SỐ CT      | DIỄN GIẢI             | TK ĐƯ  | PHÁT SINH NỢ  | PHÁT SINH CÓ  |    |
| +----+------------+------------+-----------------------+--------+---------------+---------------+    |
| |  1 | 05/12/2026 | BC-0012    | Thu tiền KH Công ty A | 131    |   250.000.000 |             0 |    |
| |  2 | 10/12/2026 | UNC-0045   | Thanh toán NCC Cty B  | 331    |             0 |   180.000.000 |    |
| |  3 | 20/12/2026 | UNC-0046   | Chi trả lương T11     | 334    |             0 |   420.000.000 |    |
| |  4 | 31/12/2026 | BC-0013    | Lãi tiền gửi tháng 12 | 515    |     4.500.000 |             0 |    |
| +----+------------+------------+-----------------------+--------+---------------+---------------+    |
| CỘNG PHÁT SINH TRONG KỲ:                               |        |   254.500.000 |   600.000.000 |    |
| SỐ DƯ CUỐI KỲ:                                         |        | NỢ: 1.154.500.000 đ           |    |
+------------------------------------------------------------------------------------------------------+
| [Phím tắt]: [F2] Bút toán mới | [Ctrl+P] In sổ | [F9] Ghi sổ | [Esc] Quay lại                       |
+======================================================================================================+
```

### 2. Wireframe: Bảng Cân Đối Phát Sinh Các Tài Khoản (Trial Balance 8 Cột)

```
+======================================================================================================+
| ninjaTax | BẢNG CÂN ĐỐI SỐ PHÁT SINH TÀI KHOẢN (TRIAL BALANCE - TT99)                                |
| Kỳ báo cáo: Cả năm 2026 (Từ 01/01/2026 đến 31/12/2026)                                               |
+======================================================================================================+
| MÃ TK | TÊN TÀI KHOẢN        |   SỐ DƯ ĐẦU NĂM   |  SỐ PHÁT SINH NĂM |   SỐ DƯ CUỐI NĂM  | TRẠNG THÁI|
|       |                      |    NỢ   |   CÓ    |   NỢ    |   CÓ    |    NỢ   |   CÓ    |           |
+-------+----------------------+---------+---------+---------+---------+---------+---------+-----------+
| 1111  | Tiền mặt             |    50M  |      0  |   800M  |   720M  |   130M  |      0  | [ Khớp ]  |
| 1121  | Tiền gửi ngân hàng   |   500M  |      0  | 2.500M  | 1.800M  | 1.200M  |      0  | [ Khớp ]  |
| 131   | Phải thu khách hàng  |   200M  |      0  | 3.200M  | 2.900M  |   500M  |      0  | [ Lưỡng ] |
| 1561  | Hàng hóa             |   450M  |      0  | 1.600M  | 1.400M  |   650M  |      0  | [ Khớp ]  |
| 331   | Phải trả người bán   |      0  |   180M  | 1.500M  | 1.700M  |      0  |   380M  | [ Lưỡng ] |
| 4111  | Vốn đầu tư CSH       |      0  | 1.000M  |      0  |      0  |      0  | 1.000M  | [ Khớp ]  |
| 4212  | LNST chưa PP năm nay |      0  |    20M  | 1.100M  | 1.600M  |      0  |   520M  | [ Kết chuyển]
| 511   | Doanh thu BH & CCDV  |      0  |      0  | 3.000M  | 3.000M  |      0  |      0  | [ Hết dư ]|
| 632   | Giá vốn hàng bán     |      0  |      0  | 1.400M  | 1.400M  |      0  |      0  | [ Hết dư ]|
| 642   | Chi phí QLDN         |      0  |      0  |   600M  |   600M  |      0  |      0  | [ Hết dư ]|
+-------+----------------------+---------+---------+---------+---------+---------+---------+-----------+
| TỔNG CỘNG:                   | 1.200M  | 1.200M  | 15.700M | 15.700M | 2.480M  | 2.480M  | [ 100% CÂN|
+======================================================================================================+
```

---

## 🗺️ PHẦN 6: LỘ TRÌNH TRIỂN KHAI & KẾ HOẠCH BÀN GIAO (ROADMAP)

### Sprint 1: Fiscal Period Lock & Hard Freeze Engine
- Mở rộng Entity `CauHinhKeToan` thêm các trường: `NgayKhoaSo`, `NguoiKhoaSoId`, `ChoPhepSuaChungTuDaKhoaSo` (mặc định = false).
- Tạo `FiscalPeriodGuard` can thiệp vào `ButToanService`, `ThuChiService`, `HachToanBanHangService`, `HachToanMuaHangService`, `InventoryService` để chặn mọi thao tác write/update/delete vào ngày <= `NgayKhoaSo`.

### Sprint 2: Automated Closing Entry Service (Kết chuyển Cuối kỳ TT99)
- Tạo service `IPeriodClosingService` và `PeriodClosingService`.
- Tự động sinh bút toán định khoản Nợ/Có không qua 911 đưa thẳng vào 4212.
- Kiểm tra tính trọn vẹn số dư và cập nhật `BaoCaoTaiChinhNam`.

### Sprint 3: Sổ Cái (S03b-DN) & Bảng Cân Đối Phát Sinh (Trial Balance) Controller & Views
- Tạo `GeneralLedgerController` phục vụ:
  - Xem Sổ Cái S03b-DN theo tài khoản và thời gian.
  - Xem Sổ Nhật ký chung S03a-DN.
  - Xem Bảng Cân đối tài khoản (Trial Balance) 8 cột.
- Cập nhật menu điều hướng `menu.json` thêm phân hệ Sổ Cái Tổng Hợp.

### Sprint 4: Subledger Reconciliation Engine & Audit Trail
- Xây dựng công cụ đối soát số dư Subledger (AR, AP, Kho, TSCĐ) vs GL Accounts.
- Tạo màn hình cảnh báo lệch số liệu giữa các phân hệ trước khi khóa sổ.

### Sprint 5: TDD Test Suite & Hardening Verification
- Viết test suite xUnit:
  - `PeriodLockingEnforcementTests`: Chứng minh chặn 100% việc thêm/sửa/xóa trong kỳ đã khóa sổ.
  - `AutomatedClosingTt99Tests`: Chứng minh kết chuyển doanh thu chi phí không sinh TK 911 và số dư sau kết chuyển về 0.
  - `TrialBalanceEqualityTests`: Đảm bảo Nợ == Có tuyệt đối.
- Xác thực `dotnet build` (0 warnings) & `dotnet test` (all pass).
