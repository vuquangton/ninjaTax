# ĐÁNH GIÁ CHUYÊN SÂU & KẾ HOẠCH HIỆU CHỈNH KIẾN TRÚC TÀI KHOẢN 911 (CALIBRATION PLAN)
## Tác giả: Lead Business Analyst (20+ năm ERP/Fintech) & Kế toán trưởng Doanh nghiệp

---

## 📌 PHẦN 1: BÁO CÁO PHÂN TÍCH THÔNG TIN & XUNG ĐỘT NGHIỆP VỤ (GAP ANALYSIS)

### 1. Thực trạng đối chiếu giữa Pháp luật Thực tế vs Codebase Hiện tại

```
+----------------------------------------------------------------------------------------------------+
|                                    BẢNG ĐỐI CHIẾU NGHIỆP VỤ PHÁP LÝ                               |
+====================================================================================================+
| NỘI DUNG                   | BỘ TÀI CHÍNH (MOF/VAS/TT 99)            | CODEBASE NINJATAX HIỆN TẠI   |
+----------------------------+-----------------------------------------+------------------------------+
| 1. Tài khoản 911           | TỒN TẠI CHÍNH THỨC làm tài khoản        | BỊ CẤM TUYỆT ĐỐI (Strict Ban)|
|                            | trung gian xác định KQKD.               | trong AGENTS.md, Entities,   |
|                            | Không có số dư cuối kỳ.                 | Services và Tests.           |
+----------------------------+-----------------------------------------+------------------------------+
| 2. Luồng kết chuyển DT/CP  | 5xx/7xx -> 911                          | 5xx/7xx -> 4212 trực tiếp    |
|                            | 911 -> 6xx/8xx/8211                     | 4212 -> 6xx/8xx trực tiếp    |
|                            | 911 -> 4212 (lãi) / 4212 -> 911 (lỗ)    | (Bỏ qua hoàn toàn TK 911)    |
+----------------------------+-----------------------------------------+------------------------------+
| 3. Báo cáo Sổ Cái S03b-DN  | Bắt buộc có Sổ Cái TK 911 để Thanh tra  | Không có Sổ Cái TK 911.      |
|                            | Thuế kiểm tra luồng bóc tách lãi/lỗ.    | Đoàn thuế sẽ xử phạt hành    |
|                            |                                         | chính theo NĐ 41/2018/NĐ-CP. |
+----------------------------+-----------------------------------------+------------------------------+
| 4. Bảng Cân Đối 8 Cột      | TK 911 xuất hiện ở cặp cột Phát sinh    | TK 911 hoàn toàn vắng mặt.   |
|    (Trial Balance)         | (Tổng PS Nợ = Tổng PS Có, Dư cuối = 0)  |                              |
+----------------------------+-----------------------------------------+------------------------------+
```

### 2. Nguyên nhân cốt lõi (Root Cause)
- Dự án `ninjaTax` trước đây áp dụng triết lý "Lean Accounting" nội bộ để tối giản số lượng bút toán, tự đặt ra quy tắc cấm TK 911 trong `AGENTS.md` và `GEMINI.md`.
- Tuy nhiên, theo quy chuẩn kế toán Việt Nam (Điều 9, 10 Luật Kế toán 88/2015/QH13 và Chế độ Kế toán Doanh nghiệp của Bộ Tài chính), việc lược bỏ tài khoản cấp 1 chuẩn quốc gia (TK 911) làm sai lệch cấu trúc hệ thống tài khoản pháp định, khiến báo cáo sổ cái không đủ tính pháp lý khi quyết toán và thanh tra thuế.

---

## 🏛️ PHẦN 2: THIẾT KẾ MÔ HÌNH MIỀN MỚI (DOMAIN MODEL CALIBRATION)

### 1. Kiến trúc Bút toán Kết chuyển Chuẩn Bộ Tài chính

```
                                  +-----------------------+
                                  |    TÀI KHOẢN 911      |
                                  | (Xác định KQKD - TT99)|
                                  +-----------+-----------+
                                              |
                     +------------------------+------------------------+
                     | (Bên Nợ)                                        | (Bên Có)
                     v                                                 v
       +----------------------------+                    +----------------------------+
       | KẾT CHUYỂN CHI PHÍ (6/8)   |                    | KẾT CHUYỂN DOANH THU (5/7) |
       | Nợ 911 / Có 632            |                    | Nợ 511 / Có 911            |
       | Nợ 911 / Có 635            |                    | Nợ 515 / Có 911            |
       | Nợ 911 / Có 641, 642       |                    | Nợ 711 / Có 911            |
       | Nợ 911 / Có 811            |                    +----------------------------+
       | Nợ 911 / Có 8211 (Thuế)    |                                  |
       +----------------------------+                                  |
                     |                                                 |
                     +------------------------+------------------------+
                                              |
                                     (Chênh lệch Lãi / Lỗ)
                                              |
                         +--------------------+--------------------+
                         | (Nếu Lãi: DT > CP)                      | (Nếu Lỗ: CP > DT)
                         v                                         v
            +------------------------+                +------------------------+
            | Nợ TK 911              |                | Nợ TK 4212             |
            |   Có TK 4212           |                |   Có TK 911            |
            +------------------------+                +------------------------+
                         \                                        /
                          \                                      /
                           +------------------------------------+
                           | TK 911 HOÀN TOÀN HẾT SỐ DƯ CUỐI KỲ |
                           +------------------------------------+
```

### 2. Các Bất biến Mới (Calibrated Domain Invariants)
1. **TK 911 là Tài khoản Tập hợp & Phân phối (Clearing Account)**:
   - Thuộc nhóm Loại 9 (`LoaiTaiKhoan.XacDinhKetQuaKinhDoanh = 9`).
   - Tính chất: `TinhChatTaiKhoan.KhongCoSoDu`.
   - Bất biến: Sau khi chạy kết chuyển cuối kỳ, $\text{Số dư cuối kỳ của TK 911} \equiv 0$.
2. **Double-Entry Equality**:
   - $\sum \text{Phát sinh Nợ TK 911} \equiv \sum \text{Phát sinh Có TK 911}$.
3. **Cơ chế Dual-Mode (Option Linh hoạt)**:
   - Cung cấp cờ cấu hình `SuDungTaiKhoan911` trong `CauHinhKeToan`:
     * Mode `True` (Mặc định chuẩn MoF/VAS): Đi qua TK 911.
     * Mode `False` (Lean internal): Đi trực tiếp 4212.

---

## 🗺️ PHẦN 3: KẾ HOẠCH THI CÔNG HIỆU CHỈNH TỪNG BƯỚC (CALIBRATION IMPLEMENTATION PLAN)

### Bước 1: Hiệu chỉnh Văn bản Chỉ đạo & Nguyên tắc Repo (AGENTS.md & GEMINI.md)
- Cập nhật mục *Core Accounting Invariants* trong `AGENTS.md`: Chuyển từ "Cấm tuyệt đối 911" sang "Chuẩn hóa quy trình kết chuyển qua TK 911 (MoF compliant) với số dư cuối kỳ = 0".
- Đồng bộ `GEMINI.md`.

### Bước 2: Hiệu chỉnh CSDL & Seeding Data (`DbInitializer.cs` & `TaiKhoan.cs`)
- Mở rộng enum `LoaiTaiKhoan` trong `TaiKhoan.cs` thêm `XacDinhKetQua = 9`.
- Bổ sung tài khoản `911 - Xác định kết quả kinh doanh` vào danh mục seed chuẩn trong `DbInitializer.cs`.

### Bước 3: Nâng cấp `PeriodClosingService.cs`
- Thay thế luồng kết chuyển trực tiếp bằng luồng 3 chặng chuẩn:
  1. Chặng 1: Kết chuyển toàn bộ DT (5xx, 7xx) sang `Có 911`.
  2. Chặng 2: Kết chuyển toàn bộ CP (6xx, 8xx, 8211) sang `Nợ 911`.
  3. Chặng 3: Kết chuyển chênh lệch thuần giữa Nợ và Có của 911 sang `TK 4212` (Lãi: `Nợ 911 / Có 4212`; Lỗ: `Nợ 4212 / Có 911`).
- Đảm bảo TK 911 đóng sạch về 0 sau bút toán.

### Bước 4: Cập nhật Báo cáo Sổ Cái & Trial Balance (`GeneralLedgerService.cs`)
- Sổ Cái S03b-DN: Cho phép xem chi tiết toàn bộ các dòng phát sinh của TK 911 trong kỳ.
- Bảng Cân đối 8 Cột: Thể hiện dòng TK 911 có phát sinh Nợ và Có bằng nhau, số dư đầu kỳ = 0, số dư cuối kỳ = 0.

### Bước 5: Cập nhật Test Suite Toàn Diện (TDD)
- Cập nhật các bài test cũ từng assert cấm 911 (`PeriodClosingTt99Tests`, `MultiDatabaseTests`, `DepartmentPayrollPostingTests`, v.v.).
- Viết test mới khẳng định:
  * TK 911 có mặt và ghi nhận đúng đối ứng.
  * Số dư cuối kỳ của TK 911 luôn bằng 0.
  * Tổng Nợ == Tổng Có trên toàn bộ bút toán kết chuyển.

### Bước 6: Kiểm tra Build, Chạy Full Suite & Review
- Chạy `dotnet build` (0 warnings).
- Chạy `dotnet test` (100% Green).
- Review chéo Standards & Spec.
