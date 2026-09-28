# TÀI LIỆU YÊU CẦU NGHIỆP VỤ & ĐẶC TẢ KỸ THUẬT (BRD & TECHNICAL SPECIFICATION)
## CHUẨN HÓA TOÀN DIỆN THÔNG TƯ 99/2025/TT-BTC BỘ TÀI CHÍNH & TÀI KHOẢN 911 (PHASE 10)

- **Dự án**: `ninjaTax` - Nền tảng Kế toán & Thuế Doanh nghiệp Tinh gọn
- **Phiên bản tài liệu**: 10.0-RELEASE (MoF Alignment)
- **Tác giả**: Lead Business Analyst (20+ năm kinh nghiệm ERP/Fintech) & Kế toán trưởng Doanh nghiệp (20+ năm thực chiến VAS/TT99/Thanh tra Thuế)
- **Cơ sở pháp lý**: Thông tư số **99/2025/TT-BTC** (Bộ Tài chính ban hành 27/10/2025, hiệu lực 01/01/2026), Luật Kế toán số **88/2015/QH13**, Nghị định số **41/2018/NĐ-CP** (Xử phạt vi phạm hành chính trong lĩnh vực kế toán).

---

## 🛑 PHẦN 1: MỤC TIÊU VÀ SỰ CẦN THIẾT CỦA VIỆC CHUẨN HÓA

### 1. Vấn đề Pháp lý Sống còn
1. **Tài khoản 911 (Xác định Kết quả Kinh doanh)**:
   - Theo Phụ lục II Thông tư 99/2025/TT-BTC, Tài khoản 911 là tài khoản bắt buộc thuộc Loại 9 nhằm phản ánh đầy đủ doanh thu, thu nhập thuần và toàn bộ chi phí hợp lý phát sinh trong kỳ kế toán.
   - Sổ Cái TK 911 (Mẫu S03b-DN) là tài liệu bắt buộc phải in ra giấy khi quyết toán thuế TNDN và thanh tra kế toán.
   - Doanh nghiệp không sử dụng TK 911 sẽ bị phạt hành chính từ **20.000.000 đ đến 30.000.000 đ** theo Điều 8 Nghị định 41/2018/NĐ-CP về hành vi lập sổ kế toán không đúng phương pháp và có nguy cơ bị ấn định thuế.
2. **Bóc tách Lưỡng tính Hai chiều (Non-Offsetting Rule) trên Bảng Cân Đối Tài Khoản (Trial Balance)**:
   - Khoản 1 Điều 7 Thông tư 99/2025/TT-BTC nghiêm cấm bù trừ số dư giữa các đối tượng khác nhau trên tài khoản lưỡng tính (TK 131, 331, 333).
   - Trên Bảng Cân đối tài khoản 8 cột, số dư của TK 131 và TK 331 phải đồng thời phản ánh cả bên Nợ (tổng các khách hàng/NCC có dư Nợ) và bên Có (tổng các khách hàng/NCC có dư Có).

---

## 🏛️ PHẦN 2: THIẾT KẾ KIẾN TRÚC NGHIỆP VỤ (BUSINESS ARCHITECTURE)

```
+======================================================================================================+
|                             QUY TRÌNH KẾT CHUYỂN CUỐI KỲ CHUẨN TT99/2025/TT-BTC                      |
+======================================================================================================+
|                                                                                                      |
|   [1. KẾT CHUYỂN DOANH THU & THU NHẬP]        [2. KẾT CHUYỂN CHI PHÍ & THUẾ TNDN]                    |
|   - Nợ TK 511 (Doanh thu bán hàng)            - Nợ TK 911 / Có TK 632 (Giá vốn)                      |
|   - Nợ TK 515 (Doanh thu tài chính)           - Nợ TK 911 / Có TK 635 (Chi phí tài chính)            |
|   - Nợ TK 711 (Thu nhập khác)                 - Nợ TK 911 / Có TK 641, 642 (Chi phí bán hàng, QLDN)  |
|       --> Có TK 911 (Xác định KQKD)           - Nợ TK 911 / Có TK 811 (Chi phí khác)                 |
|                                               - Nợ TK 911 / Có TK 8211 (Chi phí thuế TNDN)           |
|                                                                                                      |
|                                        +-------------------+                                         |
|                                        |   TÀI KHOẢN 911   |                                         |
|                                        |  (Số dư trước KC) |                                         |
|                                        +---------+---------+                                         |
|                                                  |                                                   |
|                         +------------------------+------------------------+                          |
|                         | (Nếu Tổng DT > Tổng CP)                         | (Nếu Tổng CP > Tổng DT)  |
|                         v                                                 v                          |
|             +-------------------------+                       +-------------------------+            |
|             |     KẾT CHUYỂN LÃI      |                       |     KẾT CHUYỂN LỖ       |            |
|             | Nợ TK 911               |                       | Nợ TK 4212              |            |
|             |   Có TK 4212 (LNST)     |                       |   Có TK 911             |            |
|             +-------------------------+                       +-------------------------+            |
|                         \                                                 /                          |
|                          \                                               /                           |
|                           +---------------------------------------------+                            |
|                           |      BẤT BIẾN: SỐ DƯ TK 911 CUỐI KỲ == 0    |                            |
|                           |  (Đóng sạch 100% tài khoản loại 5,6,7,8,9)  |                            |
|                           +---------------------------------------------+                            |
|                                                                                                      |
+======================================================================================================+
```

---

## 📐 PHẦN 3: ĐẶC TẢ KỸ THUẬT VÀ THỰC THỂ (TECHNICAL & DATA SPECIFICATION)

### 1. Thực thể `TaiKhoan` (Models/Entities/TaiKhoan.cs)
- Cập nhật enum `LoaiTaiKhoan`:
  ```csharp
  public enum LoaiTaiKhoan
  {
      TaiSan = 1,
      NoPhaiTra = 2,
      VonChuSoHuu = 3,
      DoanhThu = 4,
      ChiPhi = 5,
      ThuNhapKhac = 7,
      ChiPhiKhac = 8,
      XacDinhKetQuaKinhDoanh = 9 // Chuẩn TT99/2025/TT-BTC
  }
  ```
- Thêm tài khoản `911 - Xác định kết quả kinh doanh` vào seed data `DbInitializer.cs`:
  - `MaTaiKhoan`: `"911"`
  - `TenTaiKhoan`: `"Xác định kết quả kinh doanh"`
  - `BacTaiKhoan`: 1
  - `LoaiTaiKhoan`: `LoaiTaiKhoan.XacDinhKetQuaKinhDoanh`
  - `TinhChat`: `TinhChatTaiKhoan.KhongCoSoDu`
  - `LaTaiKhoanSoCai`: `false`

### 2. Thuật toán Kết chuyển Cuối kỳ (`PeriodClosingService.cs`)
- **Bước 1**: Quét tổng phát sinh Có - Nợ của các tài khoản doanh thu (511, 515, 711).
  Tạo dòng bút toán: `Nợ 5xx, 7xx` / `Có 911`.
- **Bước 2**: Quét tổng phát sinh Nợ - Có của các tài khoản chi phí (632, 635, 641, 642, 811, 8211).
  Tạo dòng bút toán: `Nợ 911` / `Có 6xx, 8xx, 8211`.
- **Bước 3**: Tính chênh lệch thuần trên TK 911:
  - Nếu `TongDoanhThu > TongChiPhi`: Lãi thuần. Tạo dòng kết chuyển: `Nợ 911` / `Có 4212` với số tiền `TongDoanhThu - TongChiPhi`.
  - Nếu `TongChiPhi > TongDoanhThu`: Lỗ thuần. Tạo dòng kết chuyển: `Nợ 4212` / `Có 911` với số tiền `TongChiPhi - TongDoanhThu`.
- **Bảo đảm Bất biến**:
  - `TongNo == TongCo` trên toàn bộ chứng từ `PKT-KC-YYYYMM`.
  - Sổ Cái TK 911 có số dư cuối kỳ = 0.

### 3. Thuật toán Bóc tách Lưỡng tính trên Trial Balance 8 Cột (`GeneralLedgerService.cs`)
Đối với tài khoản lưỡng tính (TK 131, 331, 333, 421):
- Nhóm chi tiết theo `DoiTuongId`:
  - Khách hàng có $\sum \text{Nợ} > \sum \text{Có} \implies \text{Dư Nợ}$.
  - Khách hàng có $\sum \text{Có} > \sum \text{Nợ} \implies \text{Dư Có}$.
- Số dư Đầu kỳ và Cuối kỳ của TK lưỡng tính hiển thị đồng thời cả bên Nợ và bên Có trên Bảng Cân đối tài khoản.

---

## 🎨 PHẦN 4: WIREFRAME BẢNG CÂN ĐỐI TÀI KHOẢN VÀ SỔ CÁI 911 (ASCII ART)

### Sổ Cái Tài Khoản 911 (Mẫu S03b-DN)
```
+======================================================================================================+
| ninjaTax | SỔ CÁI TÀI KHOẢN 911 - XÁC ĐỊNH KẾT QUẢ KINH DOANH (MẪU S03b-DN)                          |
| Kỳ: Năm 2026 (Từ 01/01/2026 đến 31/12/2026)                                                          |
+======================================================================================================+
| SỐ DƯ ĐẦU KỲ:   NỢ: 0 đ                                          CÓ: 0 đ                             |
+----+------------+------------+-----------------------+--------+---------------+---------------+------+
| STT| NGÀY HT    | SỐ CT      | DIỄN GIẢI             | TK ĐƯ  | PHÁT SINH NỢ  | PHÁT SINH CÓ  | DƯ LŨY|
+----+------------+------------+-----------------------+--------+---------------+---------------+------+
|  1 | 31/12/2026 | PKT-KC-2026| Kết chuyển doanh thu  | 5111   |             0 | 2.400.000.000 | 2.400M|
|  2 | 31/12/2026 | PKT-KC-2026| Kết chuyển DT tài chí | 515    |             0 |    50.000.000 | 2.450M|
|  3 | 31/12/2026 | PKT-KC-2026| Kết chuyển giá vốn    | 632    | 1.500.000.000 |             0 |  950M |
|  4 | 31/12/2026 | PKT-KC-2026| Kết chuyển chi phí QL | 642    |   450.000.000 |             0 |  500M |
|  5 | 31/12/2026 | PKT-KC-2026| Kết chuyển thuế TNDN  | 8211   |   100.000.000 |             0 |  400M |
|  6 | 31/12/2026 | PKT-KC-2026| Kết chuyển lãi sang   | 4212   |   400.000.000 |             0 |    0M |
+----+------------+------------+-----------------------+--------+---------------+---------------+------+
| CỘNG PHÁT SINH TRONG KỲ:                              |        | 2.450.000.000 | 2.450.000.000 | [CÂN]|
| SỐ DƯ CUỐI KỲ:                                        |        | NỢ: 0 đ       | CÓ: 0 đ       | [HẾT]|
+======================================================================================================+
```

---

## 🗺️ PHẦN 5: KẾ HOẠCH TRIỂN KHAI VÀ NGHIỆM THU (ROADMAP)

1. **Sprint 1: Cập nhật Chính sách & Invariants**:
   - Chỉnh sửa `AGENTS.md` và `GEMINI.md`.
2. **Sprint 2: CSDL & Entities**:
   - Thêm `LoaiTaiKhoan.XacDinhKetQuaKinhDoanh = 9`.
   - Seed TK 911 vào `DbInitializer.cs`.
3. **Sprint 3: Nâng cấp Engine Kết chuyển Cuối kỳ**:
   - Viết lại `PeriodClosingService.cs` hạch toán qua TK 911.
4. **Sprint 4: Hoàn thiện Bóc tách Lưỡng tính trên Trial Balance**:
   - Bóc tách 2 bên TK 131, 331 trên `GeneralLedgerService.cs`.
5. **Sprint 5: Test Suite TDD & Xác minh**:
   - Cập nhật và bổ sung bài test xUnit trong `ninjaTax.Tests/`.
   - Đảm bảo `dotnet build` (0 warnings) và `dotnet test` (100% pass).
