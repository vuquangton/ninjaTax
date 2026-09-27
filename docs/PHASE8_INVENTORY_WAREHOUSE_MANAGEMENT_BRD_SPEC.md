# PHASE 8: COMPREHENSIVE INVENTORY & WAREHOUSE MANAGEMENT MODULE
## SPECIFICATION & BUSINESS REQUIREMENTS DOCUMENT (BRD)
### Chuẩn Mực Kế Toán: VAS 02 / IFRS 2 (Hàng Tồn Kho), TT 99/2025/TT-BTC, TT 200/2014/TT-BTC, Nghị Định 123/2020/NĐ-CP

---

## 1. PRODUCTION OPERATIONAL ASSESSMENT (ĐÁNH GIÁ SẴN SÀNG PRODUCTION)

### 🚨 KẾT LUẬN: KHÔNG THỂ VẬN HÀNH TRÊN MÔI TRƯỜNG PRODUCTION (CANNOT OPERATE IN PROD)

| STT | Điểm nghẽn nghiêm trọng (Showstopper) | Căn cứ Pháp lý & Rủi ro Kế toán / Thuế | Hậu quả thực tế nếu chạy PROD |
|:---:|---|---|---|
| **1** | **Chưa có Bảng Quản lý Danh mục Kho (`Kho`)** | TT 99, TT 200 (Khoản 1 Điều 25): Hàng hóa phải theo dõi chi tiết theo từng kho bảo quản | Không thể xác định hàng nằm ở kho nào, kho chính hay chi nhánh. |
| **2** | **Chưa có Engine Tính Giá Vốn Tự Động (Valuation Engine)** | VAS 02, TT 200 (Điều 23), Luật Thuế TNDN (Điều 6 TT 78/2014): Phải áp dụng 1 phương pháp tính giá hàng tồn kho nhất quán (Bình quân gia quyền hoặc FIFO) | Người dùng đang phải nhập tay `TienGiaVon` ở HĐ bán hàng! Bị Cơ quan thuế bóc tách 100% chi phí giá vốn vì không có căn cứ tính toán. |
| **3** | **Không có Cơ chế Chống Xuất Âm Kho (Negative Stock Lock)** | Chuẩn mực Kế toán VN (VAS 02): Hàng tồn kho không bao giờ có số dư Có (Dư Có TK 152, 156 là sai bản chất kế toán) | Bị phạt vi phạm hành chính từ 20-30 triệu đồng theo NĐ 41/2018/NĐ-CP; Báo cáo tài chính bị kiểm toán từ chối đưa ý kiến chấp nhận toàn phần. |
| **4** | **Thiếu Hệ Thống Chứng Từ Nhập - Xuất - Điều Chuyển Độc Lập** | Mẫu 01-VT (Phiếu nhập kho), Mẫu 02-VT (Phiếu xuất kho), Mẫu 03-VT (Phiếu điều chuyển) theo TT 200 / TT 99 | Khi mua hàng chưa xuất bán ngay hoặc luân chuyển giữa các chi nhánh, kế toán không có màn hình và chứng từ hạch toán. |
| **5** | **Thiếu Sổ Sách Bắt Buộc: Báo cáo Nhập - Xuất - Tồn (S10-DN)** | Thông tư 200/2014/TT-BTC (Mẫu S10-DN): Bảng tổng hợp Nhập - Xuất - Tồn và Sổ chi tiết vật tư hàng hóa | Hồ sơ quyết toán thuế bị đình chỉ, không có bảng NXT đối chiếu với Sổ cái TK 152, 156. |
| **6** | **Thiếu Phân hệ Kiểm Kê & Xử Lý Thừa/Thiếu Hàng Tồn Kho** | Thông tư 99/2025/TT-BTC & TT 200: Cuối niên độ bắt buộc kiểm kê thực tế, xử lý qua TK 1381 (Thiếu) hoặc TK 3381 (Thừa) | Chênh lệch thực tế và sổ sách không có quy trình đối soát, che giấu thất thoát hàng hóa. |

---

## 2. KIẾN TRÚC PHÂN HỆ KHO TỔNG THỂ (INVENTORY ARCHITECTURE)

```
+----------------------------------------------------------------------------------------------------+
|                                    NINJATAX INVENTORY MODULE                                       |
+----------------------------------------------------------------------------------------------------+
|                                                                                                    |
|    [DANH MỤC]                     [CHỨNG TỪ KHO]                     [GIÁ VỐN & BÁO CÁO]           |
|  +--------------+            +-----------------------+            +-------------------------+     |
|  | Kho Hàng     |            | Phiếu Nhập Kho        |            | Engine Bình Quân        |     |
|  | (Kho, Loại,  |            | (Mua ngoài, Tự sản    |            | (Gia quyền cuối kỳ/     |     |
|  | Chi nhánh)   |            |  xuất, Thu hồi)       |            |  liên hoàn sau mỗi lần) |     |
|  +------+-------+            +-----------+-----------+            +------------+------------+     |
|         |                                |                                     |                   |
|         |                                v                                     v                   |
|         |                    +-----------------------+            +-------------------------+     |
|         +------------------->| Phiếu Xuất Kho        |<---------->| Báo Cáo Nhập Xuất Tồn   |     |
|         |                    | (Bán hàng, Sản xuất,  |            | (Mẫu S10-DN TT200/TT99) |     |
|         |                    |  Hao hụt, Nội bộ)     |            +-------------------------+     |
|         |                    +-----------+-----------+                         |                   |
|         |                                |                                     v                   |
|         |                                v                        +-------------------------+     |
|         |                    +-----------------------+            | Sổ Chi Tiết Vật Tư      |     |
|         +------------------->| Điều Chuyển Kho       |            | Thẻ Kho (Mẫu S12-DN)    |     |
|                              | (Giữa các kho nội bộ) |            +-------------------------+     |
|                              +-----------+-----------+                         |                   |
|                                          |                                     v                   |
|                                          v                        +-------------------------+     |
|                              +-----------------------+            | Kiểm Kê & Xử Lý Thừa/   |     |
|                              | SỔ NHẬT KÝ CHUNG      |<---------->| Thiếu (TK 1381 / 3381)  |     |
|                              | (Bút toán kép TT99)   |            +-------------------------+     |
|                              +-----------------------+                                             |
+----------------------------------------------------------------------------------------------------+
```

---

## 3. SƠ ĐỒ ĐỊNH KHOẢN KẾ TOÁN KHO THEO THÔNG TƯ 99/2025/TT-BTC

```
   1. MUA HÀNG NHẬP KHO                   2. XUẤT KHO BÁN HÀNG
      Nợ 152, 1561                          Nợ 632 (Giá vốn)
      Nợ 1331 (VAT)                             Có 152, 1561
          Có 331, 111, 112                      (Định giá theo BQGQ/FIFO)

   3. XUẤT KHO SẢN XUẤT / CÔNG TRÌNH      4. THU HỒI VẬT TƯ / NHẬP LẠI KHO
      Nợ 154 (Chi phí SXKD dở dang)         Nợ 152, 1561
          Có 152 (Nguyên vật liệu)              Có 154 / 632 / 6422

   5. ĐIỀU CHUYỂN NỘI BỘ GIỮA CÁC KHO     6. KIỂM KÊ PHÁT HIỆN THỪA / THIẾU
      Kho Đến: Nợ 1561 (Kho B)              - THIẾU: Nợ 1381 (Tài sản thiếu chờ xử lý)
      Kho Đi:      Có 1561 (Kho A)                     Có 152, 1561
      (Không làm thay đổi tổng tài sản)     - THỪA:  Nợ 152, 1561
                                                       Có 3381 (Tài sản thừa chờ xử lý)
```

---

## 4. CHI TIẾT THỰC THỂ DỮ LIỆU (DATABASE SCHEMA SPEC)

### 4.1. Thực thể `Kho` (Warehouse)
- `Id` (bigint, PK)
- `ChiNhanhId` (bigint, FK -> ChiNhanh)
- `MaKho` (nvarchar(50), Unique per ChiNhanh)
- `TenKho` (nvarchar(255), NotNull)
- `DiaChi` (nvarchar(500))
- `ThuKhoId` (bigint, Nullable, FK -> NhanVien)
- `TaiKhoanKhoMacDinhId` (bigint, Nullable, FK -> TaiKhoan) (VD: 152, 1561, 155)
- `DangHoatDong` (bool, default true)
- `NgayTao` (DateTime)

### 4.2. Thực thể `PhieuNhapKho` & `ChiTietNhapKho`
- **Header (`PhieuNhapKho`)**:
  - `Id` (bigint, PK)
  - `ChiNhanhId` (bigint, FK)
  - `KhoId` (bigint, FK -> Kho)
  - `SoPhieu` (nvarchar(50), Unique per Year)
  - `NgayNhap` (DateTime)
  - `NgayHachToan` (DateTime)
  - `LoaiNhapKho` (enum: MuaNgoai = 1, TuSanXuat = 2, ThuHoi = 3, NhapKhac = 4)
  - `HoaDonMuaHangId` (bigint, Nullable, FK -> HoaDonMuaHang)
  - `NhaCungCapId` (bigint, Nullable, FK -> DoiTuong)
  - `DienGiai` (nvarchar(500))
  - `TongSoLuong` (decimal 19, 4)
  - `TongTienHang` (decimal 19, 4)
  - `TrangThai` (enum: TamTinh = 0, DaGhiSo = 1, DaHuy = 2)
  - `ButToanId` (bigint, Nullable, FK -> ButToan)
- **Line Item (`ChiTietNhapKho`)**:
  - `Id` (bigint, PK)
  - `PhieuNhapKhoId` (bigint, FK)
  - `VatTuHangHoaId` (bigint, FK -> VatTuHangHoa)
  - `SoLuong` (decimal 19, 4)
  - `DonGia` (decimal 19, 4)
  - `ThanhTien` (decimal 19, 4)
  - `TaiKhoanNoId` (bigint, FK -> TaiKhoan - default 152/156)
  - `TaiKhoanCoId` (bigint, FK -> TaiKhoan - default 331/111/154)
  - `SoLo` (nvarchar(50), Nullable)
  - `HanSuDung` (DateTime, Nullable)

### 4.3. Thực thể `PhieuXuatKho` & `ChiTietXuatKho`
- **Header (`PhieuXuatKho`)**:
  - `Id` (bigint, PK)
  - `ChiNhanhId` (bigint, FK)
  - `KhoId` (bigint, FK -> Kho)
  - `SoPhieu` (nvarchar(50), Unique)
  - `NgayXuat` (DateTime)
  - `NgayHachToan` (DateTime)
  - `LoaiXuatKho` (enum: BanHang = 1, SanXuat = 2, SuDungNoiBo = 3, XuatKhac = 4)
  - `HoaDonBanHangId` (bigint, Nullable, FK -> HoaDonBanHang)
  - `KhachHangId` (bigint, Nullable, FK -> DoiTuong)
  - `PhongBanId` (bigint, Nullable, FK -> PhongBan)
  - `DienGiai` (nvarchar(500))
  - `TongSoLuong` (decimal 19, 4)
  - `TongTienGiaVon` (decimal 19, 4)
  - `TrangThai` (enum: TamTinh = 0, DaGhiSo = 1, DaHuy = 2)
  - `ButToanId` (bigint, Nullable, FK -> ButToan)
- **Line Item (`ChiTietXuatKho`)**:
  - `Id` (bigint, PK)
  - `PhieuXuatKhoId` (bigint, FK)
  - `VatTuHangHoaId` (bigint, FK -> VatTuHangHoa)
  - `SoLuong` (decimal 19, 4)
  - `DonGiaVon` (decimal 19, 4 - Tính tự động qua Valuation Engine)
  - `TienGiaVon` (decimal 19, 4)
  - `TaiKhoanNoId` (bigint, FK -> TaiKhoan - 632 / 154 / 642)
  - `TaiKhoanCoId` (bigint, FK -> TaiKhoan - 152 / 1561)

### 4.4. Thực thể `PhieuChuyenKho` (Internal Warehouse Transfer)
- `Id` (bigint, PK)
- `SoPhieu` (nvarchar(50))
- `NgayChuyen` (DateTime)
- `KhoXuatId` (bigint, FK -> Kho)
- `KhoNhapId` (bigint, FK -> Kho)
- `TrangThai` (enum: DangChuyen = 1, DaNhan = 2, DaHuy = 3)
- `ChiTietChuyenKhos`: Danh sách vật tư, số lượng xuất và nhập

### 4.5. Thực thể `BienBanKiemKeKho` (Inventory Audit Sheet)
- `Id` (bigint, PK)
- `KhoId` (bigint, FK -> Kho)
- `NgayKiemKe` (DateTime)
- `ChiTietKiemKes`:
  - `VatTuHangHoaId`
  - `SoLuongSoSach` (Hệ thống tính)
  - `SoLuongThucTe` (Người dùng đếm)
  - `SoLuongChenhLech` (Thực tế - Sổ sách)
  - `DonGia`
  - `GiaTriChenhLech`
  - `XuLy` (enum: ChuaXuLy = 0, GhiNhanThua3381 = 1, GhiNhanThieu1381 = 2)

---

## 5. THUẬT TOÁN TÍNH GIÁ XUẤT KHO (INVENTORY VALUATION ENGINES)

### 5.1. Bình Quân Gia Quyền Cuối Kỳ (Periodic Weighted Average)

$$\text{Đơn giá BQGQ} = \frac{\text{Giá trị tồn đầu kỳ} + \sum \text{Giá trị nhập trong kỳ}}{\text{Số lượng tồn đầu kỳ} + \sum \text{Số lượng nhập trong kỳ}}$$

$$\text{Tiền giá vốn xuất kho} = \text{Số lượng xuất} \times \text{Đơn giá BQGQ}$$

### 5.2. Chống Xuất Âm Kho (Anti-Negative Stock Invariant)
```csharp
public async Task ValidateStockAvailabilityAsync(long khoId, long vatTuId, decimal soLuongXuat, DateTime ngayXuat)
{
    var currentBalance = await GetCurrentStockBalanceAsync(khoId, vatTuId, ngayXuat);
    if (currentBalance < soLuongXuat)
    {
        throw new InvalidOperationException(
            $"Không thể xuất kho! Vật tư ID {vatTuId} tại Kho ID {khoId} chỉ còn tồn {currentBalance:N2}, yêu cầu xuất {soLuongXuat:N2}. Vi phạm bất biến chống xuất âm kho.");
    }
}
```

---

## 6. USE CASES & LUỒNG NGHIỆP VỤ (MAIN FLOWS, ALTERNATIVE, EXCEPTION)

### UC-INV-01: Nhập Kho Mua Hàng
- **Actor**: Thủ kho / Kế toán vật tư
- **Pre-condition**: Đã tạo Hóa đơn mua hàng hoặc tạo phiếu nhập độc lập
- **Main Flow**:
  1. Chọn Chi nhánh, Kho nhận hàng, Nhà cung cấp.
  2. Thêm dòng vật tư, số lượng, đơn giá mua.
  3. Hệ thống kiểm tra hợp lệ và tạo chứng từ ở trạng thái `TamTinh`.
  4. Người dùng bấm **"Ghi Sổ"**:
     - Cập nhật số dư tồn kho tức thời.
     - Tự động sinh `ButToan`: Nợ 152/156 / Có 331 (hoặc liên kết HĐ mua).
     - Cập nhật đơn giá mua gần nhất vào `VatTuHangHoa`.
- **Exception Flow**:
  - Mã vật tư bị khóa ngừng kinh doanh: Chặn, yêu cầu kích hoạt lại.

### UC-INV-02: Xuất Kho Bán Hàng & Tính Giá Vốn
- **Actor**: Kế toán bán hàng / Thủ kho
- **Pre-condition**: Đã có số dư tồn kho tại kho xuất.
- **Main Flow**:
  1. Lập phiếu xuất (hoặc tích chọn "Kiêm xuất kho" tại Hóa đơn bán hàng).
  2. Chọn vật tư và số lượng xuất.
  3. Hệ thống kiểm tra tồn kho tại thời điểm xuất:
     - Nếu $\text{Tồn kho} < \text{Số lượng xuất} \rightarrow$ **EXCEPTION (Báo lỗi xuất âm kho, chặn ghi sổ)**.
  4. Hệ thống áp đơn giá vốn theo phương pháp BQGQ.
  5. Sinh `ButToan`: Nợ 632 / Có 1561 (gắn `PhongBanId` để phân bổ P&L bộ phận).
  6. Ghi giảm số lượng tồn kho.

### UC-INV-03: Báo Cáo Nhập - Xuất - Tồn (S10-DN)
- **Actor**: Kế toán trưởng / Giám đốc
- **Main Flow**:
  1. Chọn kỳ báo cáo (Tháng, Quý, Năm), Chi nhánh, Kho.
  2. Hệ thống tổng hợp:
     - Tồn đầu kỳ: Số lượng, Đơn giá, Thành tiền.
     - Nhập trong kỳ: Số lượng, Thành tiền.
     - Xuất trong kỳ: Số lượng, Thành tiền.
     - Tồn cuối kỳ: Số lượng, Thành tiền.
  3. Đối soát bất biến:
     $$\text{Tồn Cuối} \equiv \text{Tồn Đầu} + \text{Nhập} - \text{Xuất}$$
     $$\text{Tổng Thành Tiền Tồn Cuối (152+156)} \equiv \text{Số Dư Nợ Sổ Cái TK 152, 156}$$

---

## 7. GIAO DIỆN NGƯỜI DÙNG & WIREFRAMES (ASCII ART UI/UX)

### 7.1. Màn Hình Danh Sách & Báo Cáo Nhập Xuất Tồn (`/Inventory/StockBalance`)
```
+----------------------------------------------------------------------------------------------------+
|  [LOGO] NINJATAX ERP - BÁO CÁO NHẬP XUẤT TỒN KHO (MẪU S10-DN)                Chi nhánh: Hà Nội [v]  |
+----------------------------------------------------------------------------------------------------+
| Kỳ: [ Tháng 10/2026 v ]   Kho: [ Tất Cả Kho v ]   Nhóm: [ Hàng Hóa v ]   [ Lọc Dữ Liệu ] [ Xuất Excel ]
+----------------------------------------------------------------------------------------------------+
| [KPI Cards]                                                                                        |
| +---------------------+ +---------------------+ +---------------------+ +----------------------+ |
| | TỒN ĐẦU KỲ          | | NHẬP TRONG KỲ       | | XUẤT TRONG KỲ       | | TỒN CUỐI KỲ          | |
| | 1.250.000.000 đ     | | 850.000.000 đ       | | 620.000.000 đ       | | 1.480.000.000 đ      | |
| +---------------------+ +---------------------+ +---------------------+ +----------------------+ |
|                                                                                                    |
| [BẢNG TỔNG HỢP NHẬP XUẤT TỒN S10-DN]                                                               |
+----+------------+----------------------+-----+---------+-------------+---------+-------------+-----+
|STT | Mã Hàng    | Tên Hàng Hóa         | ĐVT | SL Tồn  | TT Tồn Đầu  | SL Nhập | TT Nhập     | ... |
+----+------------+----------------------+-----+---------+-------------+---------+-------------+-----+
| 1  | HH-CISCO-01| Switch Cisco C9200L  | Cái |   10    | 250.000.000 |    5    | 125.000.000 | ... |
| 2  | HH-DELL-R7 | Server Dell R750     | Bộ  |    4    | 320.000.000 |    2    | 160.000.000 | ... |
| 3  | VT-CAP-CAT6| Cáp Mạng Cat6 Comm   | Thùng| 50    |  75.000.000 |   20    |  30.000.000 | ... |
+----+------------+----------------------+-----+---------+-------------+---------+-------------+-----+
|    | TỔNG CỘNG  |                      |     |         |1.250.000.000|         | 850.000.000 | ... |
+----+------------+----------------------+-----+---------+-------------+---------+-------------+-----+
```

### 7.2. Màn Hình Lập Phiếu Xuất Kho (`/Inventory/CreateOutward`)
```
+----------------------------------------------------------------------------------------------------+
|  TẠO PHIẾU XUẤT KHO VẬT TƯ / HÀNG HÓA                                          [ Lưu Tạm ] [ Ghi Sổ ]|
+----------------------------------------------------------------------------------------------------+
| Số Phiếu: [ XK-2026-0012   ]  Ngày Xuất: [ 27/09/2026 ]  Kho Xuất: [ Kho Tổng Hà Nội           [v] ] |
| Loại Xuất:[ Xuất Bán Hàng v]  Hóa Đơn:   [ HĐ-001294  ]  Bộ Phận:  [ Phòng Kinh Doanh Dự Án    [v] ] |
| Diễn Giải:[ Xuất kho thiết bị switch cisco cho dự án Viettel Telecom                             ] |
+----------------------------------------------------------------------------------------------------+
| CHI TIẾT HÀNG XUẤT KHO                                                       [ + Thêm Hàng ]       |
+----+------------+--------------------+-----+---------+-----------+-----------+----------+----------+
|STT | Mã Hàng    | Tên Vật Tư         | ĐVT | Tồn Kho | SL Xuất   | Đơn Giá   | Nợ TK    | Có TK    |
+----+------------+--------------------+-----+---------+-----------+-----------+----------+----------+
| 1  | HH-CISCO-01| Switch Cisco C9200 | Cái |   15    |    5      | [Tự tính] | 632      | 1561     |
| 2  | VT-CAP-CAT6| Cáp Mạng Cat6      |Thùng|   40    |   10      | [Tự tính] | 632      | 152      |
+----+------------+--------------------+-----+---------+-----------+-----------+----------+----------+
| Cảnh báo: Tồn kho đáp ứng 100%. Không có mục nào bị âm kho.                                       |
+----------------------------------------------------------------------------------------------------+
```

---

## 8. KẾ HOẠCH TRIỂN KHAI THEO SLICES (INCREMENTAL TDD ROADMAP)

```
   [Phase 8: Inventory & Warehouse Module Roadmap]
   |
   +---> Slice 1: Warehouse Entity & Schema (`Kho`, `PhieuNhapKho`, `PhieuXuatKho`)
   |            * TDD: Test các ràng buộc DB, Multi-branch warehouse mapping.
   |
   +---> Slice 2: Stock Ledger & Real-Time Balance Service (`IInventoryService`)
   |            * TDD: Kiểm tra tính tồn kho lũy kế, bất biến chống xuất âm kho.
   |
   +---> Slice 3: Valuation Engine (Bình quân gia quyền & FIFO)
   |            * TDD: Tự động gán `DonGiaVon` vào bút toán Nợ 632 / Có 1561.
   |
   +---> Slice 4: Stock Movement UI & Integrated Sales/Purchase Flow
   |            * Razor Views: Nhập kho, Xuất kho, Điều chuyển, liên kết HĐĐT.
   |
   +---> Slice 5: Statutory Reports (S10-DN Nhập Xuất Tồn, S12-DN Thẻ Kho) & Kiểm Kê
                * Reconciliation với B01-DN (Chỉ tiêu Hàng tồn kho) & B02-DN (Giá vốn).
```

---

## 9. QUY TẮC BẢO TOÀN KIẾN TRÚC & PHÁP LÝ (COMPLIANCE INVARIANTS)

1. **Tuyệt đối cấm TK 911**: Giá vốn xuất thẳng Nợ 632 / Có 1561; cuối kỳ kết chuyển Nợ 421 / Có 632.
2. **Khóa sổ kỳ kế toán**: Không được thêm/sửa/xóa phiếu nhập/xuất trong kỳ đã khóa sổ (`NgayKhoaSo`).
3. **Độ chính xác tiền tệ**: Toàn bộ số lượng và đơn giá lưu `decimal(19, 4)` bảo vệ làm tròn số.
