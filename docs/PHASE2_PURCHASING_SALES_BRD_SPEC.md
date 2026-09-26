# TÀI LIỆU YÊU CẦU NGHIỆP VỤ & ĐẶC TẢ KỸ THUẬT CHI TIẾT (BRD & SPEC)
## PHÂN HỆ MUA HÀNG & BÁN HÀNG (PURCHASING & SALES MODULES) - CHUẨN THÔNG TƯ TT99 & HĐĐT

- **Dự án**: ninjaTax Accounting System (MISA SME / FAST Equivalent)
- **Tác giả**: Hội đồng Kế toán trưởng (20+ năm kinh nghiệm VAS/TT99) & Trưởng ban Phân tích Nghiệp vụ ERP (20+ năm kinh nghiệm)
- **Cơ sở pháp lý**:
  - Thông tư 99/2025/TT-BTC (Chế độ kế toán doanh nghiệp mới nhất áp dụng từ 01/01/2026, thay thế TT200/2014/TT-BTC).
  - Nghị định 123/2020/NĐ-CP, Nghị định 254/2026/NĐ-CP & Thông tư 78/2021/TT-BTC về Hóa đơn điện tử (HĐĐT).
  - Chuẩn mực Kế toán Việt Nam (VAS 02 - Hàng tồn kho, VAS 14 - Doanh thu và thu nhập khác, VAS 16 - Chi phí đi vay).
  - Chuẩn định dạng dữ liệu Hóa đơn điện tử XML (Quyết định 1450/QĐ-TCT của Tổng cục Thuế).

---

## MỤC LỤC
1. [Đánh giá Khả năng Vận hành Môi trường Thực tế (Production Audit)](#1-đánh-giá-khả-năng-vận-hành-môi-trường-thực-tế-production-audit)
2. [Cơ sở Pháp lý & Ma trận Định khoản Kế toán TT99](#2-cơ-sở-pháp-lý--ma-trận-định-khoản-kế-toán-tt99)
3. [Kiến trúc Dữ liệu & Sơ đồ ASCII ART](#3-kiến-trúc-dữ-liệu--sơ-đồ-ascii-art)
4. [Đặc tả Thực thể (Entity Model Specifications)](#4-đặc-tả-thực-thể-entity-model-specifications)
5. [Quy trình Nghiệp vụ (Workflows) & Use Cases](#5-quy-trình-nghiệp-vụ-workflows--use-cases)
6. [Quản trị Công nợ Phải thu / Phải trả (AR/AP Subledger & Aging)](#6-quản-trị-công-nợ-phải-thu--phải-trả-arap-subledger--aging)
7. [Thiết kế Giao diện Người dùng (UI/UX Wireframes ASCII)](#7-thiết-kế-giao-diện-người-dùng-uiux-wireframes-ascii)
8. [Kế hoạch & Lộ trình Thực thi Chi tiết (Execution Roadmap)](#8-kế-hoạch--lộ-trình-thực-thi-chi-tiết-execution-roadmap)

---

## 1. ĐÁNH GIÁ KHẢ NĂNG VẬN HÀNH MÔI TRƯỜNG THỰC TẾ (PRODUCTION AUDIT)

### 1.1 Câu hỏi: Hệ thống hiện tại có thể chạy được trên Môi trường Production cho Mua/Bán hàng chưa?
**TRẢ LỜI: KHÔNG THỂ (CRITICAL GAP).**

### 1.2 Bảng Phân tích Lỗ hổng Trọng yếu (Gap Analysis):

| TT | Hạng mục | Trạng thái Phase 1 | Yêu cầu Bắt buộc Production (TT99 & Thuế) | Rủi ro nếu đưa vào Prod |
|---|---|---|---|---|
| 1 | **Hóa đơn điện tử (HĐĐT)** | Chưa có | Quản lý Ký hiệu mẫu số (1C26T), Ký hiệu (C26TNN), Số HĐ (8 số), Mã CQT, Trạng thái phát hành/ký số/gửi CQT. | Bị Cục Thuế phạt hành chính hóa đơn không hợp pháp, xuất hóa đơn khống. |
| 2 | **Danh mục Vật tư / Dịch vụ** | Chưa có | `VatTuHangHoa`: Mã, tên, ĐVT, Thuế suất ngầm định, Tài khoản kho (152/156), Tài khoản Doanh thu (511), Giá vốn (632). | Không thể kiểm soát số lượng tồn kho, giá vốn và tỷ lệ lãi gộp. |
| 3 | **Tách biệt Kho & Tài chính** | Gộp chung vào GL | Phải hỗ trợ: Mua hàng nhập kho (tăng số lượng kho), Mua hàng không qua kho (chi phí thẳng), Bán hàng kiêm xuất kho, Bán hàng chưa xuất kho. | Xuất hiện số dư ảo trên sổ kho, lệch kiểm kê thủ kho và kế toán kho. |
| 4 | **Tính giá vốn tự động** | Chưa có | Xuất kho bán hàng bắt buộc tự động sinh cặp bút toán Giá vốn (`Nợ 632 / Có 156`) theo phương pháp Bình quân gia quyền / FIFO. | Báo cáo KQKD sai lệch hoàn toàn tỷ suất lợi nhuận gộp. |
| 5 | **Đối trừ Công nợ (Matching AR/AP)** | Chưa có | Phải theo dõi công nợ chi tiết theo từng hóa đơn, hạn thanh toán, chiết khấu thanh toán, tuổi nợ (Aging). | Không thể đòi nợ khách hàng, trả nhầm nhà cung cấp, kiểm toán từ chối số dư 131/331. |
| 6 | **3-Way Matching (Mua hàng)** | Chưa có | Đối chiếu 3 bên: Đơn mua hàng (PO) <=> Phiếu nhập kho (GRN) <=> Hóa đơn mua hàng (Invoice). | Doanh nghiệp bị thất thoát tiền, thanh toán vượt khối lượng thực nhận. |

---

## 2. CƠ SỞ PHÁP LÝ & MA TRẬN ĐỊNH KHOẢN KẾ TOÁN TT99

### 2.1 Quy tắc Bất biến TT99 trong Mua/Bán hàng:
1. **Tuyệt đối không dùng TK 911**: Toàn bộ doanh thu hàng bán (TK 511), giảm trừ doanh thu (TK 521 hoặc ghi giảm trực tiếp 511), giá vốn (TK 632), chi phí bán hàng (TK 641), QLDN (TK 642) cuối kỳ kết chuyển thẳng về TK 421.
2. **Chứng từ gốc bắt buộc**: Mọi hóa đơn, phiếu nhập/xuất đều phải gắn chặt `SoChungTuGoc` và `NgayChungTuGoc` vào Header của Bút toán GL để phục vụ đoàn thanh tra thuế.
3. **Độ chính xác tiền tệ 19, 4**: Số lượng x Đơn giá có thể ra số lẻ, thuế GTGT làm tròn theo nguyên tắc kế toán Việt Nam (làm tròn số nguyên đồng đối với VNĐ).

### 2.2 Ma trận Định khoản Chuẩn TT99:

```
+----------------------------------------------------------------------------------------------------+
| NGHIỆP VỤ MUA HÀNG (PURCHASING)                                                                    |
+------------------------------------+--------------------------+--------------------+---------------+
| Tình huống                         | Tài khoản Nợ             | Tài khoản Có       | Chứng từ gốc  |
+------------------------------------+--------------------------+--------------------+---------------+
| Mua hàng hóa nhập kho chưa trả tiền| Nợ 1561 (Tiền hàng)      | Có 331 (Phải trả)  | HĐĐT đầu vào, |
|                                    | Nợ 1331 (Thuế GTGT 8/10%)|                    | Phiếu nhập kho|
+------------------------------------+--------------------------+--------------------+---------------+
| Mua vật liệu thanh toán ngay (CK/TM)| Nợ 152                   | Có 1111 / 1121     | HĐĐT, Giấy báo|
|                                    | Nợ 1331                  |                    | nợ / Phiếu chi|
+------------------------------------+--------------------------+--------------------+---------------+
| Chi phí mua hàng (vận chuyển, bốc) | Nợ 1562 / 152            | Có 331 / 111 / 112 | HĐ vận chuyển |
|                                    | Nợ 1331                  |                    |               |
+------------------------------------+--------------------------+--------------------+---------------+
| Chiết khấu thương mại được hưởng   | Nợ 331                   | Có 1561 (giảm giá) | HĐ điều chỉnh |
|                                    |                          | Có 1331            | của NCC       |
+------------------------------------+--------------------------+--------------------+---------------+
| Trả lại hàng mua cho người bán     | Nợ 331                   | Có 1561            | HĐ xuất trả,  |
|                                    |                          | Có 1331            | Phiếu xuất kho|
+------------------------------------+--------------------------+--------------------+---------------+

+----------------------------------------------------------------------------------------------------+
| NGHIỆP VỤ BÁN HÀNG (SALES)                                                                         |
+------------------------------------+--------------------------+--------------------+---------------+
| Tình huống                         | Tài khoản Nợ             | Tài khoản Có       | Chứng từ gốc  |
+------------------------------------+--------------------------+--------------------+---------------+
| Ghi nhận Doanh thu bán hàng hóa    | Nợ 131 (Chưa thu tiền)   | Có 5111 (Doanh thu)| Hóa đơn điện  |
|                                    | Nợ 1121/1111 (Thu tiền)  | Có 33311 (Thuế VAT)| tử bán ra     |
+------------------------------------+--------------------------+--------------------+---------------+
| Ghi nhận Giá vốn hàng bán          | Nợ 632 (Giá vốn)         | Có 1561 (Kho hàng) | Phiếu xuất kho|
+------------------------------------+--------------------------+--------------------+---------------+
| Hàng bán bị trả lại (Doanh thu)    | Nợ 5212 (hoặc giảm 511)  | Có 131 / 111 / 112 | Biên bản trả, |
|                                    | Nợ 33311 (giảm thuế GTGT)|                    | HĐ điều chỉnh |
+------------------------------------+--------------------------+--------------------+---------------+
| Hàng bán bị trả lại (Nhập lại kho) | Nợ 1561                  | Có 632             | Phiếu nhập kho|
+------------------------------------+--------------------------+--------------------+---------------+
| Chiết khấu thanh toán cho khách    | Nợ 635 (Chi phí tài chính| Có 131 (giảm nợ)   | Chứng từ bù trừ|
+------------------------------------+--------------------------+--------------------+---------------+
```

---

## 3. KIẾN TRÚC DỮ LIỆU & SƠ ĐỒ ASCII ART

### 3.1 Sơ đồ Luồng Nghiệp vụ Mua hàng (3-Way Matching Flow):

```
[Phòng Thu Mua]               [Thủ Kho]                   [Kế Toán Mua Hàng]
       |                          |                                |
       |-- (1) Lập Đơn Đặt Hàng ->|                                |
       |       (Purchase Order)   |                                |
       |                          |                                |
       |                          |-- (2) Nhận Hàng & Kiểm đếm --->|
       |                          |       Lập Phiếu Nhập Kho (GRN) |
       |                          |                                |-- (3) Tiếp nhận HĐĐT XML
       |                          |                                |       (Nghị định 123/78)
       |                          |                                |
       |                          |<==== (4) 3-WAY MATCHING ======>|
       |                          |   [PO] <===> [GRN] <===> [INV] |
       |                          |   (Khớp: SL, Đơn giá, Thuế)    |
       |                          |                                |
       |                          |                                |-- (5) TỰ ĐỘNG SINH GL
       |                          |                                |   Nợ 1561 / Nợ 1331
       |                          |                                |   Có 331 (Chi tiết NCC)
       |                          |                                |
       |                          |                                |-- (6) Sổ Chi Tiết Công Nợ
```

### 3.2 Sơ đồ Luồng Nghiệp vụ Bán hàng & Xuất Hóa đơn Điện tử:

```
[Khách Hàng]            [Kế Toán Bán Hàng]               [Thủ Kho]               [Tổng Cục Thuế]
     |                          |                            |                          |
     |-- (1) Đặt mua hàng ----->|                            |                          |
     |                          |-- (2) Lập Đơn Bán Hàng     |                          |
     |                          |       (Sales Order)        |                          |
     |                          |                            |                          |
     |                          |-- (3) Yêu cầu xuất kho --->|                          |
     |                          |                            |-- (4) Lập Phiếu Xuất Kho |
     |                          |                            |       Tự động: Nợ 632    |
     |                          |                            |                Có 1561   |
     |                          |<--- Xác nhận đã xuất ------|                          |
     |                          |                                                       |
     |                          |-- (5) Phát hành HĐĐT XML (Ký số điện tử HSM) -------->|
     |                          |                                                       |-- Cấp mã CQT
     |                          |<-- (6) Nhận Mã CQT (Hóa đơn hợp lệ) <-----------------|
     |                          |
     |                          |-- (7) TỰ ĐỘNG SINH BÚT TOÁN DOANH THU GL
     |                          |       Nợ 131 (Chi tiết KH)
     |                          |       Có 5111 (Doanh thu bán hàng)
     |                          |       Có 33311 (Thuế GTGT 8%/10%)
     |<-- (8) Gửi HĐĐT Email ---|
```

### 3.3 Sơ đồ Quan hệ Thực thể Phase 2 (ERD ASCII):

```
+-------------------+           1:N         +------------------------+
|  VatTuHangHoa     |---------------------->| ChiTietHoaDonMua       |
|-------------------|                       |------------------------|
| Id (PK, bigint)   |                       | Id (PK, bigint)        |
| MaVatTu (varchar) |                       | HoaDonMuaHangId (FK)   |
| TenVatTu (varchar)|                       | VatTuHangHoaId (FK)    |
| DVT (varchar)     |                       | SoLuong (19,4)         |
| TaiKhoanKhoId     |                       | DonGia (19,4)          |
| TaiKhoanDoanhThuId|                       | TienHang (19,4)        |
| TaiKhoanGiaVonId  |                       | ThueSuatVat (decimal)  |
| ThueSuatVatMacDinh|                       | TienThueVat (19,4)     |
+-------------------+                       +------------------------+
         |                                               |
         | 1:N                                           | N:1
         v                                               v
+------------------------+                  +------------------------+
| ChiTietHoaDonBan       |                  | HoaDonMuaHang          |
|------------------------|                  |------------------------|
| Id (PK, bigint)        |                  | Id (PK, bigint)        |
| HoaDonBanHangId (FK)   |                  | SoHoaDon (varchar)     |
| VatTuHangHoaId (FK)    |                  | KyHieuHoaDon (varchar) |
| SoLuong (19,4)         |                  | NgayHoaDon (datetime)  |
| DonGia (19,4)          |                  | NhaCungCapId (FK->DoiTuong)
| ThanhTien (19,4)       |                  | TongTienHang (19,4)    |
| ThueSuatVat (decimal)  |                  | TongTienThue (19,4)    |
| TienThueVat (19,4)     |                  | TongThanhToan (19,4)   |
| TiLeChietKhau (decimal)|                  | ButToanId (FK, bigint) |
| TienChietKhau (19,4)   |                  | TrangThaiThanhToan     |
+------------------------+                  +------------------------+
         |                                               |
         | N:1                                           | 1:1
         v                                               v
+------------------------+                  +------------------------+
| HoaDonBanHang          |                  | ButToan (GL Core)      |
|------------------------|                  |------------------------|
| Id (PK, bigint)        | 1:1              | Id (PK, bigint)        |
| SoHoaDon (varchar)     |----------------->| SoChungTu              |
| KHMauSo (varchar)      |                  | SoChungTuGoc (TT99)    |
| KyHieu (varchar)       |                  | NgayChungTuGoc (TT99)  |
| NgayHoaDon (datetime)  |                  | TongNo (19,4)          |
| KhachHangId (FK)       |                  | TongCo (19,4)          |
| MaCoQuanThue (varchar) |                  +------------------------+
| TrangThaiHDDT (enum)   |                               ^
| TongTienHang (19,4)    |                               | 1:N
| TongTienThue (19,4)    |                  +------------------------+
| TongThanhToan (19,4)   |                  | ChiTietButToan         |
| ButToanDoanhThuId (FK) |                  |------------------------|
| ButToanGiaVonId (FK)   |                  | Id (PK, bigint)        |
+------------------------+                  | ButToanId (FK)         |
                                            | TaiKhoanNoId (FK)      |
                                            | TaiKhoanCoId (FK)      |
                                            | SoTien (19,4)          |
                                            | DoiTuongId (FK)        |
                                            +------------------------+
```

---

## 4. ĐẶC TẢ THỰC THỂ (ENTITY MODEL SPECIFICATIONS)

### 4.1 Thực thể `VatTuHangHoa.cs` (Danh mục Vật tư, Hàng hóa, Dịch vụ):
```csharp
namespace ninjaTax.Models.Entities;

public enum LoaiVatTuHangHoa
{
    VatTu = 1,          // Nguyên vật liệu (TK 152)
    CongCuDungCu = 2,   // CCDC (TK 153)
    ThanhPham = 3,      // Thành phẩm sản xuất (TK 155)
    HangHoa = 4,        // Hàng hóa thương mại (TK 156)
    DichVu = 5          // Dịch vụ tiêu dùng/vận chuyển (Không tính tồn kho)
}

/// <summary>
/// Danh mục Vật tư hàng hóa và dịch vụ chuẩn hóa ERP.
/// </summary>
public class VatTuHangHoa
{
    public long Id { get; set; }
    public string MaVatTu { get; set; } = string.Empty;
    public string TenVatTu { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public LoaiVatTuHangHoa LoaiVatTu { get; set; } = LoaiVatTuHangHoa.HangHoa;

    // Thiết lập tài khoản ngầm định theo TT99
    public long? TaiKhoanKhoId { get; set; }        // 152, 1561
    public TaiKhoan? TaiKhoanKho { get; set; }

    public long? TaiKhoanDoanhThuId { get; set; }   // 5111, 5112, 5113
    public TaiKhoan? TaiKhoanDoanhThu { get; set; }

    public long? TaiKhoanGiaVonId { get; set; }      // 632
    public TaiKhoan? TaiKhoanGiaVon { get; set; }

    public decimal ThueSuatVatMacDinh { get; set; } = 10m; // 0, 5, 8, 10
    public decimal DonGiaMuaGanNhat { get; set; }
    public decimal DonGiaBanTieuChuan { get; set; }
    public bool DangTheoDoiTonKho { get; set; } = true;
    public bool DangHoatDong { get; set; } = true;
}
```

### 4.2 Thực thể `HoaDonMuaHang.cs` & `ChiTietHoaDonMua.cs`:
```csharp
namespace ninjaTax.Models.Entities;

public enum TrangThaiHoaDonMua
{
    ChoNhanHang = 0,
    DaNhanHangChuaHoaDon = 1,
    DaNhanHoaDon = 2,
    DaGhiSo = 3,
    DaHuy = 4
}

public class HoaDonMuaHang
{
    public long Id { get; set; }
    public string SoChungTu { get; set; } = string.Empty; // Mã chứng từ nội bộ: MH-2026-0001
    public DateTime NgayChungTu { get; set; } = DateTime.Today;
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    // Thông tin HĐĐT đầu vào theo Nghị định 123/2020/NĐ-CP
    public string KHMauSoHoaDon { get; set; } = "1C26TAA";
    public string KyHieuHoaDon { get; set; } = "C26T";
    public string SoHoaDon { get; set; } = string.Empty; // 8 chữ số
    public DateTime NgayHoaDon { get; set; } = DateTime.Today;
    public string? MaTraCuuHdt { get; set; }

    // Nhà cung cấp
    public long NhaCungCapId { get; set; }
    public DoiTuong? NhaCungCap { get; set; }
    public string? MaSoThueNCC { get; set; }
    public string? TenNCC { get; set; }
    public string? DiaChiNCC { get; set; }

    public string DienGiai { get; set; } = string.Empty;
    public DateTime HanThanhToan { get; set; } = DateTime.Today.AddDays(30);

    // Tiền tệ (Độ chính xác 19, 4)
    public decimal TongTienHang { get; set; }
    public decimal TongTienChietKhau { get; set; }
    public decimal TongTienThueVat { get; set; }
    public decimal TongThanhToan { get; set; }
    public decimal DaThanhToan { get; set; }
    public decimal ConPhaiTra => TongThanhToan - DaThanhToan;

    public bool MuaHangKiemKho { get; set; } = true; // true: tự động tạo Phiếu Nhập Kho
    public TrangThaiHoaDonMua TrangThai { get; set; } = TrangThaiHoaDonMua.DaNhanHoaDon;

    // Khóa ngoại liên kết tới Bút toán Sổ Cái Core GL
    public long? ButToanId { get; set; }
    public ButToan? ButToan { get; set; }

    public ICollection<ChiTietHoaDonMua> ChiTietHangs { get; set; } = new List<ChiTietHoaDonMua>();
}

public class ChiTietHoaDonMua
{
    public long Id { get; set; }
    public long HoaDonMuaHangId { get; set; }
    public HoaDonMuaHang? HoaDonMuaHang { get; set; }
    public int DongSo { get; set; } = 1;

    public long VatTuHangHoaId { get; set; }
    public VatTuHangHoa? VatTuHangHoa { get; set; }

    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }

    public decimal TiLeChietKhau { get; set; }
    public decimal TienChietKhau { get; set; }

    public decimal ThueSuatVat { get; set; } // 0, 5, 8, 10
    public decimal TienThueVat { get; set; }
    public decimal TongTien => ThanhTien - TienChietKhau + TienThueVat;

    // Chỉ định tài khoản hạch toán chi tiết
    public long TaiKhoanNoId { get; set; }      // 152, 1561, 642...
    public long TaiKhoanThueId { get; set; }    // 1331
    public long TaiKhoanCoId { get; set; }      // 331, 111, 112
}
```

### 4.3 Thực thể `HoaDonBanHang.cs` & `ChiTietHoaDonBan.cs` (HĐĐT Chuẩn QĐ 1450):
```csharp
namespace ninjaTax.Models.Entities;

public enum TrangThaiHddt
{
    MoiTao = 0,             // Bản nháp chưa ký
    DaKySo = 1,             // Đã ký số điện tử
    DaGuiCoQuanThue = 2,    // Đang chờ cấp mã
    CoQuanThueCapMa = 3,    // Hợp lệ, đã có mã CQT
    CoQuanThueTuChoi = 4,   // Bị lỗi từ chối
    DaHuy = 5,              // Đã lập biên bản hủy
    BiDieuChinh = 6,        // Có HĐ điều chỉnh
    BiThayThe = 7           // Có HĐ thay thế
}

public class HoaDonBanHang
{
    public long Id { get; set; }
    public string SoChungTu { get; set; } = string.Empty; // BH-2026-0001
    public DateTime NgayChungTu { get; set; } = DateTime.Today;
    public DateTime NgayHachToan { get; set; } = DateTime.Today;

    // Thông số Hóa đơn điện tử chuẩn Nghị định 123
    public string KHMauSo { get; set; } = "1C26TBB";
    public string KyHieu { get; set; } = "C26T";
    public string SoHoaDon { get; set; } = string.Empty;
    public DateTime NgayHoaDon { get; set; } = DateTime.Today;
    public string? MaCoQuanThue { get; set; } // Chuỗi mã băm CQT cấp
    public string? ChuKySo { get; set; }      // X.509 Certificate Hash
    public TrangThaiHddt TrangThai { get; set; } = TrangThaiHddt.MoiTao;

    // Khách hàng
    public long KhachHangId { get; set; }
    public DoiTuong? KhachHang { get; set; }
    public string? MaSoThueKH { get; set; }
    public string? TenKhachHang { get; set; }
    public string? DiaChiKH { get; set; }

    public string DienGiai { get; set; } = string.Empty;
    public DateTime HanThanhToan { get; set; } = DateTime.Today.AddDays(15);

    // Tổng tiền thanh toán
    public decimal TongTienHang { get; set; }
    public decimal TongTienChietKhau { get; set; }
    public decimal TongTienThueVat { get; set; }
    public decimal TongThanhToan { get; set; }
    public decimal DaThuTien { get; set; }
    public decimal ConPhaiThu => TongThanhToan - DaThuTien;

    public bool BanHangKiemXuatKho { get; set; } = true;

    // Bút toán GL (1 bút toán Doanh thu, 1 bút toán Giá vốn)
    public long? ButToanDoanhThuId { get; set; }
    public ButToan? ButToanDoanhThu { get; set; }

    public long? ButToanGiaVonId { get; set; }
    public ButToan? ButToanGiaVon { get; set; }

    public ICollection<ChiTietHoaDonBan> ChiTietBans { get; set; } = new List<ChiTietHoaDonBan>();
}

public class ChiTietHoaDonBan
{
    public long Id { get; set; }
    public long HoaDonBanHangId { get; set; }
    public HoaDonBanHang? HoaDonBanHang { get; set; }
    public int DongSo { get; set; } = 1;

    public long VatTuHangHoaId { get; set; }
    public VatTuHangHoa? VatTuHangHoa { get; set; }

    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }

    public decimal TiLeChietKhau { get; set; }
    public decimal TienChietKhau { get; set; }

    public decimal ThueSuatVat { get; set; }
    public decimal TienThueVat { get; set; }
    public decimal TongTien => ThanhTien - TienChietKhau + TienThueVat;

    // Tài khoản định khoản Doanh thu
    public long TaiKhoanNoId { get; set; }       // 131, 111, 112
    public long TaiKhoanDoanhThuId { get; set; } // 5111, 5112
    public long TaiKhoanThueId { get; set; }     // 33311

    // Tài khoản định khoản Giá vốn (nếu kiêm xuất kho)
    public decimal DonGiaVon { get; set; }
    public decimal TienGiaVon => SoLuong * DonGiaVon;
    public long? TaiKhoanGiaVonId { get; set; }  // 632
    public long? TaiKhoanKhoId { get; set; }     // 1561
}
```

---

## 5. QUY TRÌNH NGHIỆP VỤ (WORKFLOWS) & USE CASES

### 5.1 Use Case UC-PUR-01: Lập Hóa Đơn Mua Hàng & Tự Động Định Khoản
- **Actor**: Kế toán Mua hàng / Kế toán kho
- **Mục tiêu**: Nhập liệu HĐĐT đầu vào từ nhà cung cấp, kiểm tra thuế suất, ghi nhận hàng hóa vào kho và sinh bút toán công nợ `Nợ 156 / Nợ 1331 / Có 331`.
- **Pre-conditions**: Nhà cung cấp đã có trong danh mục `DoiTuong`, vật tư đã có trong `VatTuHangHoa`.
- **Happy Path**:
  1. Kế toán chọn NCC, nhập Số HĐ (8 số), Ký hiệu (C26T), Ngày HĐ.
  2. Chọn các dòng hàng hóa, nhập số lượng, đơn giá, kiểm tra thuế suất VAT (8% hoặc 10%).
  3. Hệ thống tính tự động: `ThanhTien = SoLuong * DonGia`, `TienThue = ThanhTien * ThueSuat / 100`, `TongThanhToan`.
  4. Người dùng bấm **Lưu & Ghi Sổ**:
     - Service `MuaHangService.GhiSoAsync()` tự động khởi tạo thực thể `ButToan` Header:
       - `SoChungTuGoc` = `KyHieu + "-" + SoHoaDon`
       - `NgayChungTuGoc` = `NgayHoaDon`
       - `TongNo` = `TongCo` = `TongThanhToan`
     - Sinh các dòng `ChiTietButToan`:
       - Dòng 1: Nợ TK 1561 / Có TK 331 (Tiền hàng)
       - Dòng 2: Nợ TK 1331 / Có TK 331 (Tiền thuế VAT)
     - Cập nhật số dư công nợ của Nhà cung cấp (Tăng dư Có TK 331).
- **Alternative Path (Mua hàng thanh toán ngay bằng Tiền gửi/Tiền mặt)**:
  - Nếu chọn phương thức "Tiền gửi ngân hàng": Tài khoản Có tự động đổi thành TK 1121. Tự động sinh `GiayBaoNo` trong phân hệ Ngân hàng.
- **Exception Path (Vi phạm TT99 hoặc Sai lệch tiền)**:
  - Nếu `TongNo != TongCo` dù chỉ 1 đồng: Rollback transaction, báo lỗi chi tiết dòng sai lệch.
  - Nếu người dùng cố tình cấu hình tài khoản trung gian 911: Service throw `AccountingDomainException` từ chối ghi sổ.

### 5.2 Use Case UC-SAL-01: Bán Hàng Kiêm Xuất Kho & Phát Hành Hóa Đơn Điện Tử
- **Actor**: Kế toán Bán hàng
- **Mục tiêu**: Bán hàng cho khách, xuất hóa đơn điện tử có mã CQT, xuất kho hàng hóa và tự động hạch toán cả Doanh thu và Giá vốn.
- **Happy Path**:
  1. Kế toán chọn Khách hàng, nhập điều khoản thanh toán (hạn nợ 15 ngày).
  2. Thêm các mặt hàng xuất bán: Nhập số lượng, đơn giá bán, tỷ lệ chiết khấu (nếu có).
  3. Tích chọn "Kiêm xuất kho": Hệ thống tự động lấy Đơn giá vốn bình quân của hàng hóa từ kho.
  4. Bấm **Phát hành Hóa đơn & Ghi Sổ**:
     - Hệ thống đóng gói dữ liệu XML HĐĐT chuẩn QĐ 1450.
     - Gọi dịch vụ ký số (HSM/USB Token).
     - Gửi lên Cổng Thuế điện tử (`gdt.gov.vn`) nhận Mã CQT (`MaCoQuanThue`).
     - Tự động sinh 02 Bút toán độc lập nhưng liên kết chặt chẽ:
       - **Bút toán 1 (Doanh thu)**: Nợ 131 (Tổng tiền) / Có 5111 (Tiền hàng) / Có 33311 (Thuế GTGT).
       - **Bút toán 2 (Giá vốn)**: Nợ 632 / Có 1561 (Giá trị xuất kho).
     - Ghi nhận `SoChungTuGoc` là số HĐĐT đã được CQT phê duyệt.

---

## 6. QUẢN TRỊ CÔNG NỢ PHẢI THU / PHẢI TRẢ (AR/AP SUBLEDGER & AGING)

### 6.1 Cơ chế Đối trừ Chứng từ (Invoice Settlement Engine):
Mỗi chứng từ thanh toán (Phiếu chi, Giấy báo nợ, Ủy nhiệm chi, Phiếu thu) sẽ được bù trừ linh hoạt với Hóa đơn:
1. **Đối trừ tự động theo FIFO**: Ưu tiên thanh toán cho hóa đơn phát sinh sớm nhất còn nợ.
2. **Đối trừ chỉ định theo Hóa đơn**: Kế toán tick chọn chính xác Hóa đơn số `00000123` để tất toán.

```
+-----------------------------------------------------------------------------------------+
| SƠ ĐỒ ĐỐI TRỪ CÔNG NỢ CHI TIẾT (AR/AP SETTLEMENT)                                      |
+-----------------------------------------------------------------------------------------+
  [Hóa Đơn Bán Hàng HD001] -> 50.000.000 đ (Chưa thu)
  [Hóa Đơn Bán Hàng HD002] -> 30.000.000 đ (Chưa thu)
                                     |
               [Khách hàng chuyển khoản: 60.000.000 đ]
                                     |
                           +---------v---------+
                           | Thuật Toán FIFO   |
                           +---------+---------+
                                     |
           +-------------------------+-------------------------+
           |                                                   |
           v                                                   v
   Tất toán HD001: 50.000.000 đ                         Thanh toán 1 phần HD002:
   (Trạng thái: ĐÃ THANH TOÁN)                         10.000.000 đ (Còn nợ: 20.000.000 đ)
```

### 6.2 Báo cáo Phân tích Tuổi nợ (Aging Report):
Phân loại công nợ theo các kỳ hạn nghiêm ngặt để trích lập dự phòng nợ phải thu khó đòi theo Thông tư 48/2019/TT-BTC & TT99:
- **Trong hạn**: Chưa đến ngày `HanThanhToan`.
- **Quá hạn 1 - 30 ngày**: Nhắc nợ lần 1.
- **Quá hạn 31 - 90 ngày**: Nhắc nợ lần 2, phong tỏa xuất hàng mới.
- **Quá hạn 91 - 180 ngày**: Trích lập dự phòng 30%.
- **Quá hạn 181 - 360 ngày**: Trích lập dự phòng 50%.
- **Quá hạn > 360 ngày**: Trích lập dự phòng 70% - 100%.

---

## 7. THIẾT KẾ GIAO DIỆN NGƯỜI DÙNG (UI/UX WIREFRAMES ASCII)

### 7.1 Màn hình Lập Hóa Đơn Bán Hàng Kiêm Xuất Kho & Phát Hành HĐĐT:

```
+---------------------------------------------------------------------------------------------------------+
| ninjaTax ERP | BÁN HÀNG > LẬP HÓA ĐƠN BÁN HÀNG KIÊM PHIẾU XUẤT KHO                                      |
+---------------------------------------------------------------------------------------------------------+
| [X] Kiêm phiếu xuất kho   [X] Lập hóa đơn điện tử      | Trạng thái: [BẢN NHÁP - CHƯA PHÁT HÀNH]        |
+---------------------------------------------------------------------------------------------------------+
| THÔNG TIN CHUNG                                        | THÔNG TIN HÓA ĐƠN ĐIỆN TỬ                      |
| Khách hàng: [KH001 - Cty Ánh Dương        ][v]         | Ký hiệu mẫu số : [1C26TBB  ]                   |
| Địa chỉ   : 123 Phố Huế, Hai Bà Trưng, Hà Nội          | Ký hiệu hóa đơn: [C26TAA   ]                   |
| Mã số thuế: 0109988776                                 | Số hóa đơn     : [00000045 ] (CQT Tự động cấp) |
| Người nhận: Anh Hùng - 0988.123.456                    | Ngày hóa đơn   : [26/09/2026]                  |
| Diễn giải : Xuất bán thiết bị mạng dự án Quý 3/2026    | Hạn thanh toán : [11/10/2026] (15 ngày)        |
+---------------------------------------------------------------------------------------------------------+
| CHI TIẾT HÀNG HÓA XUẤT BÁN                                                                              |
+----+-------------+--------------------+-----+--------+-----------+-----------+-----+----------+-----------+
|STT | Mã Hàng     | Tên Hàng Hóa       | ĐVT | Số Lượng| Đơn Giá Bán| Thành Tiền|%VAT | Tiền Thuế| Tổng Tiền |
+----+-------------+--------------------+-----+--------+-----------+-----------+-----+----------+-----------+
| 1  | ROUTER-CIS  | Router Cisco C9200 | Chiếc|     2  | 25.000.000| 50.000.000| 10% | 5.000.000| 55.000.000|
| 2  | CAB-CAT6    | Thùng Cáp Mạng Cat6| Thùng|     5  |  2.000.000| 10.000.000|  8% |   800.000| 10.800.000|
+----+-------------+--------------------+-----+--------+-----------+-----------+-----+----------+-----------+
| [+ Thêm dòng]  [- Xóa dòng]                                                                             |
+---------------------------------------------------------------------------------------------------------+
| ĐỊNH KHOẢN TỰ ĐỘNG PREVIEW (TT99):                                                                      |
| - Doanh thu: Nợ TK 131: 65.800.000 đ | Có TK 5111: 60.000.000 đ | Có TK 33311: 5.800.000 đ              |
| - Giá vốn  : Nợ TK 632: 42.000.000 đ | Có TK 1561: 42.000.000 đ (Kho Hà Nội)                            |
+---------------------------------------------------------------------------------------------------------+
| TỔNG TIỀN HÀNG: 60.000.000 đ | TIỀN THUẾ VAT: 5.800.000 đ | TỔNG CỘNG THANH TOÁN: 65.800.000 đ VNĐ     |
+---------------------------------------------------------------------------------------------------------+
| [HỦY BỎ]                          [LƯU TẠM]            [KÝ SỐ & GỬI CƠ QUAN THUẾ]     [LƯU & IN HÓA ĐƠN]|
+---------------------------------------------------------------------------------------------------------+
```

---

## 8. KẾ HOẠCH & LỘ TRÌNH THỰC THI CHI TIẾT (EXECUTION ROADMAP)

```
===================================================================================================
LỘ TRÌNH TRIỂN KHAI PHASE 2: PHÂN HỆ MUA HÀNG & BÁN HÀNG (WBS CHUẨN ERP)
===================================================================================================

[Milestone 2.1] Core Entities & Database Migration
  ├── Tạo Models/Entities/VatTuHangHoa.cs (Phân loại, đơn vị tính, TK ngầm định)
  ├── Tạo Models/Entities/HoaDonMuaHang.cs & ChiTietHoaDonMua.cs (HĐĐT đầu vào, 19,4 decimal)
  ├── Tạo Models/Entities/HoaDonBanHang.cs & ChiTietHoaDonBan.cs (HĐĐT đầu ra chuẩn QĐ 1450)
  ├── Cập nhật Data/AppDbContext.cs: Fluent API, FKs, Indexes, HasPrecision(19, 4), SQLite TEXT
  └── Sinh Migration EF Core: `AddPurchaseAndSalesModule` & update database schema

[Milestone 2.2] Double-Entry Auto Journal Engine (Động cơ hạch toán TT99)
  ├── Xây dựng `IHachToanMuaHangService` & `HachToanMuaHangService`:
  │     └── Auto-generate `ButToan` Header (Nợ 152/156, Nợ 1331, Có 331), gán `SoChungTuGoc`
  ├── Xây dựng `IHachToanBanHangService` & `HachToanBanHangService`:
  │     ├── Bút toán 1: Doanh thu (Nợ 131, Có 511, Có 33311)
  │     └── Bút toán 2: Giá vốn kiêm xuất kho (Nợ 632, Có 156)
  └── Đảm bảo invariant: Tuyệt đối không dùng TK 911, kiểm tra cân đối kép TongNo == TongCo

[Milestone 2.3] AR/AP Subledger & Công nợ Hóa đơn
  ├── Xây dựng `ICongNoService`:
  │     ├── Quản lý sổ chi tiết công nợ TK 131 (Phải thu KH) và TK 331 (Phải trả NCC)
  │     ├── Cơ chế Đối trừ chứng từ hóa đơn (Invoice Matching FIFO & Chỉ định)
  │     └── Báo cáo Tuổi nợ (Aging Report: 0-30, 31-60, 61-90, >90 ngày) theo TT48/TT99
  └── Kiểm soát hạn mức công nợ & cảnh báo khách hàng nợ quá hạn khi lập đơn bán hàng

[Milestone 2.4] Controllers & Thin MVC Web UI
  ├── `VatTuHangHoaController.cs` & Razor Views (CRUD danh mục hàng hóa vật tư)
  ├── `MuaHangController.cs` & Views (Danh sách hóa đơn mua, Form lập chứng từ mua hàng)
  └── `BanHangController.cs` & Views (Danh sách HĐĐT bán ra, Form lập hóa đơn kiêm xuất kho)

[Milestone 2.5] Unit Test & Production Hardening
  ├── Bộ test `PurchaseModuleTests.cs`: Kiểm tra tính toán thuế VAT 8%/10%, sinh bút toán mua hàng
  ├── Bộ test `SalesModuleTests.cs`: Kiểm tra sinh bút toán kép Doanh thu + Giá vốn, kiểm tra HĐĐT
  ├── Bộ test `ArApSettlementTests.cs`: Kiểm tra thuật toán đối trừ công nợ và tính tuổi nợ
  └── Chạy `dotnet test`, `dotnet build` đạt 0 Warnings, 0 Errors, đảm bảo tương thích 4 CSDL.
===================================================================================================
```
