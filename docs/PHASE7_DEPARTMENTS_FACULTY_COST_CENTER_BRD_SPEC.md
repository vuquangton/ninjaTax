# PHASE 7: PHÒNG BAN, KHOA CHUYÊN MÔN & TRUNG TÂM CHI PHÍ (DEPARTMENTS / FACULTY / COST CENTERS)
## BUSINESS REQUIREMENTS DOCUMENT (BRD) & TECHNICAL SPECIFICATION

- **Mã tài liệu**: `BRD-SPEC-PHASE-7-DEPARTMENTS-COST-CENTERS`
- **Phiên bản**: `1.0.0-PROD-CANDIDATE`
- **Tác giả**: BA Lead (20+ năm kinh nghiệm ERP) & Kế toán trưởng (20+ năm kinh nghiệm VAS/TT99)
- **Chuẩn mực đối chiếu**:
  - **TT 99/2025/TT-BTC** (Chế độ Kế toán Doanh nghiệp mới - **Nghiêm cấm tuyệt đối TK 911**)
  - **TT 200/2014/TT-BTC** & **TT 133/2016/TT-BTC** (Tập hợp chi phí & Giá thành)
  - **IFRS 8 / VAS 28** (Operating Segments - Báo cáo bộ phận & Trung tâm trách nhiệm)
  - **Luật Doanh nghiệp 2020 & Luật Thuế TNDN 2008 (sửa đổi)**
  - **Hệ thống ERP đối chuẩn**: MISA AMIS, Fast Business Online, BRAVO 8R3, SAP S/4HANA (Cost Center Accounting - CO-OM-CCA)

---

## 1. ĐÁNH GIÁ SẴN SÀNG VẬN HÀNH PRODUCTION (PROD READINESS)

### 🚨 KẾT LUẬN: **TUYỆT ĐỐI CHƯA THỂ VẬN HÀNH TRÊN MÔI TRƯỜNG PRODUCTION (ABSOLUTE NO)**

Nếu đưa hệ thống `ninjaTax` hiện tại vào môi trường Production cho các doanh nghiệp, tập đoàn đa chi nhánh, hoặc cơ sở giáo dục/viện đào tạo (Faculty/Khoa), hệ thống sẽ gặp các sự cố nghiêm trọng sau:

```
+-------------------------------------------------------------------------------------------------------+
|                                7 LỖ HỔNG TỬ THẦN NẾU THIẾU MODULE PHÒNG BAN/KHOA                      |
+----+----------------------------------+----------------------------------+----------------------------+
| STT| Lỗ Hổng Kỹ Thuật / Nghiệp Vụ    | Rủi Ro Thực Tế Trong Sản Xuất   | Chế Tài / Thiệt Hại        |
+----+----------------------------------+----------------------------------+----------------------------+
| 1  | NhanVien.PhongBan là chuỗi text  | Nhập tự do "Kế toán", "ke toan", | Không thể gom nhóm lương   |
|    | tự do, không có bảng khóa ngoại. | "Phòng KT", "P.KTT".             | và thuế TNCN theo bộ phận. |
+----+----------------------------------+----------------------------------+----------------------------+
| 2  | Bút toán (ChiTietButToan) không   | Mọi chi phí 642, 641, 154 bị     | Mù hoàn toàn kế toán quản  |
|    | có trường PhongBanId.            | san phẳng toàn công ty.          | trị, không biết phòng nào  |
|    |                                  |                                  | bội chi ngân sách.         |
+----+----------------------------------+----------------------------------+----------------------------+
| 3  | Bảng lương ép cứng Nợ 642/Có 334 | Đội ngũ R&D, Bán hàng, Phân      | Sai lệch Giá thành (COGS), |
|    | cho 100% nhân viên.              | xưởng đều tính vào Chi phí QLDN  | sai Báo cáo B02-DN, Cơ     |
|    |                                  | thay vì tách 6421, 6422, 154/622.| quan thuế bóc tách chi phí.|
+----+----------------------------------+----------------------------------+----------------------------+
| 4  | Không có cơ chế Cây phân cấp     | Không hỗ trợ Khối -> Ban ->      | Báo cáo tài chính quản trị |
|    | (Parent-Child Hierarchy).        | Phòng -> Nhóm / Viện -> Khoa.    | không thể rollup đa tầng.  |
+----+----------------------------------+----------------------------------+----------------------------+
| 5  | Thiếu Seam Trung tâm lợi nhuận   | Các đơn vị tự chủ (Khoa Đào tạo, | Không đo lường được P&L    |
|    | (Profit Center) vs Trung tâm CP  | Dự án phần mềm) không thể đối trừ| từng đơn vị độc lập.       |
|    | (Cost Center).                   | Doanh thu 511 vs Chi phí 6xx.    |                            |
+----+----------------------------------+----------------------------------+----------------------------+
| 6  | Tài sản cố định không gán đơn vị | TSCĐ, máy móc, laptop không biết | Thất thoát tài sản khi bàn |
|    | sử dụng / phòng ban chịu phí.    | thuộc phòng ban nào quản lý.     | giao hoặc luân chuyển NS.  |
+----+----------------------------------+----------------------------------+----------------------------+
| 7  | Vi phạm IFRS 8 / VAS 28 về       | Doanh nghiệp niêm yết hoặc FDI   | Kiểm toán Big 4 từ chối    |
|    | Báo cáo Bộ Phận (Segment Report) | bắt buộc thuyết minh doanh thu,  | đưa ý kiến chấp nhận toàn  |
|    | trong Thuyết minh BCTC.          | chi phí theo bộ phận kinh doanh. | phần (Qualified Opinion).  |
+----+----------------------------------+----------------------------------+----------------------------+
```

---

## 2. CĂN CỨ PHÁP LÝ & CHUẨN MỰC NGHỀ NGHIỆP

1. **Thông tư 99/2025/TT-BTC (Bộ Tài chính)**:
   - Doanh nghiệp phải mở sổ kế toán chi tiết theo dõi doanh thu (TK 511) và chi phí kinh doanh (TK 642, chia tách chi tiết 6421 - Chi phí bán hàng, 6422 - Chi phí quản lý doanh nghiệp) theo từng bộ phận, lĩnh vực kinh doanh.
   - **Nghiêm cấm tuyệt đối TK 911**: Toàn bộ kết chuyển doanh thu, chi phí theo từng bộ phận cuối kỳ phải đi trực tiếp vào TK 4212 (hoặc đối ứng trực tiếp qua Seam kế toán quản trị) mà không được làm méo mó dòng ghi sổ kép.
2. **Thông tư 200/2014/TT-BTC & Chuẩn mực Kế toán Việt Nam số 28 (VAS 28)**:
   - Quy định về Báo cáo bộ phận: Doanh nghiệp phân chia thành Bộ phận theo lĩnh vực kinh doanh hoặc Bộ phận theo khu vực địa lý để phản ánh rủi ro và tỷ suất sinh lời của từng bộ phận.
3. **IFRS 8 (Operating Segments)**:
   - Bắt buộc hệ thống ERP ghi nhận thông tin phân đoạn tài chính dựa trên các báo cáo nội bộ được cung cấp thường xuyên cho Giám đốc điều hành ra quyết định phân bổ nguồn lực (CODM - Chief Operating Decision Maker).
4. **Đối chuẩn phần mềm kế toán dẫn đầu**:
   - **MISA AMIS**: Thiết lập Cơ cấu tổ chức đa tầng (Tổng công ty -> Chi nhánh -> Khối/Văn phòng -> Phòng ban -> Phân xưởng/Khoa -> Nhóm/Tổ). Cho phép gắn đối tượng phòng ban vào từng dòng hạch toán chi phí/doanh thu.
   - **FAST Business Online**: Danh mục Bộ phận (Department) quản lý theo mã số mẹ - con (01 -> 01.01 -> 01.01.01), hỗ trợ phân bổ tự động chi phí chung và kết chuyển P&L theo bộ phận.
   - **BRAVO 8R3**: Quản lý đa chiều (Cost Center / Profit Center) tích hợp chặt chẽ giữa Quản trị nhân sự, Chấm công, Tính lương và Bút toán Sổ cái.

---

## 3. MÔ HÌNH MIỀN DỮ LIỆU & KIẾN TRÚC THỰC THỂ (DOMAIN MODEL)

### 3.1. Sơ đồ Thực thể Quan hệ (ERD - ASCII Art)

```
+----------------------------------------------------------------------------------------------------+
|                                    SƠ ĐỒ THỰC THỂ PHÒNG BAN & TRUNG TÂM CHI PHÍ                    |
+----------------------------------------------------------------------------------------------------+

     +-----------------------+
     | ThongTinDoanhNghiep   |
     | (Doanh nghiệp sở hữu) |
     +-----------+-----------+
                 | 1
                 |
                 | n
     +-----------v-----------+               +--------------------------------------+
     |        ChiNhanh       |<------------->|         PhongBan / Khoa              |
     | (Đơn vị cơ sở địa lý) | 1           n | (Cơ cấu phòng ban / Khoa chuyên môn) |
     +-----------------------+               +------------------+-------------------+
                                                                |
                                             +------------------+-------------------+
                                             | 1 (Cha)                              | 1
                                             |                                      |
                                             | n (Con - Đệ quy)                     | n
                                             v                                      v
                             +-------------------+                  +-------------------+
                             |  PhongBan (Con)   |                  |     NhanVien      |
                             | (Bộ phận / Nhóm)  |                  | (Nhân sự phòng)   |
                             +-------------------+                  +---------+---------+
                                                                              | 1
                                                                              |
                                                                              | n
                                                                    +---------v---------+
                                                                    | ChiTietLuongNhan  |
                                                                    |       Vien        |
                                                                    +-------------------+

                 +--------------------------------------+
                 |           ChiTietButToan             |
                 |    (Dòng hạch toán Sổ Nhật ký chung) |
                 +------------------+-------------------+
                                    | n
                                    | (Theo dõi Cost Center)
                                    | 0..1
                 +------------------v-------------------+
                 |           PhongBan / Khoa            |
                 | - Id (PK)                            |
                 | - ChiNhanhId (FK)                    |
                 | - PhongBanChaId (FK Đệ quy, Nullable)|
                 | - MaPhongBan (Unique)                |
                 | - TenPhongBan                        |
                 | - LoaiPhongBan (Enum)                |
                 | - TruongPhongId (FK NhanVien, Null)  |
                 | - TaiKhoanChiPhiMacDinhId (FK)       |
                 | - LaTrungTamLoiNhuan (bool)          |
                 | - DangHoatDong (bool)                |
                 +--------------------------------------+
```

---

## 4. CHI TIẾT THỰC THỂ LÕI (CORE ENTITIES & INVARIANTS)

### 4.1. Thực thể `PhongBan` (Department / Faculty / Cost Center)

```csharp
namespace ninjaTax.Models.Entities;

public enum LoaiPhongBan
{
    BanLanhDao = 1,          // HĐQT, Ban Giám Đốc (TK mặc định: 6422)
    KinhDoanh = 2,           // Phòng Kinh doanh, Marketing, Bán hàng (TK mặc định: 6421)
    KeToanTaiChinh = 3,      // Phòng Kế toán, Tài chính (TK mặc định: 6422)
    NhanSuHanhChinh = 4,     // Nhân sự, Hành chính, IT nội bộ (TK mặc định: 6422)
    NghienCuuPhatTrien = 5,  // Khối R&D, Kỹ thuật công nghệ (TK mặc định: 6422 hoặc 154)
    SanXuatVanHanh = 6,      // Phân xưởng, Kho vận, Sản xuất (TK mặc định: 154 / 622 / 627)
    KhoaChuyenMon = 7,       // Khoa đào tạo, Bộ môn, Trung tâm (Đơn vị giáo dục/y tế/viện)
    DuAnDacThu = 8           // Ban quản lý dự án độc lập (Project Cost/Profit Center)
}

public class PhongBan
{
    public long Id { get; set; }

    public long ChiNhanhId { get; set; }
    public virtual ChiNhanh? ChiNhanh { get; set; }

    public long? PhongBanChaId { get; set; }
    public virtual PhongBan? PhongBanCha { get; set; }
    public virtual ICollection<PhongBan> PhongBanCons { get; set; } = new List<PhongBan>();

    [Required]
    [StringLength(50)]
    public string MaPhongBan { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string TenPhongBan { get; set; } = string.Empty;

    [StringLength(255)]
    public string? TenTiengAnh { get; set; }

    public LoaiPhongBan LoaiPhongBan { get; set; } = LoaiPhongBan.KinhDoanh;

    public long? TruongPhongId { get; set; }
    public virtual NhanVien? TruongPhong { get; set; }

    /// <summary>
    /// Tài khoản chi phí mặc định khi hạch toán lương hoặc chi phí mua ngoài của phòng ban
    /// Ví dụ: 6421 (Bán hàng), 6422 (Quản lý), 154 (Sản xuất/Dịch vụ theo TT99)
    /// </summary>
    public long? TaiKhoanChiPhiMacDinhId { get; set; }
    public virtual TaiKhoan? TaiKhoanChiPhiMacDinh { get; set; }

    /// <summary>
    /// true: Trung tâm lợi nhuận (Profit Center - phát sinh cả Doanh thu 511 và Chi phí 6xx).
    /// false: Trung tâm chi phí (Cost Center thuần túy - chỉ phát sinh chi phí).
    /// </summary>
    public bool LaTrungTamLoiNhuan { get; set; } = false;

    public bool DangHoatDong { get; set; } = true;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
    public virtual ICollection<ChiTietButToan> ChiTietButToans { get; set; } = new List<ChiTietButToan>();
}
```

### 4.2. Bất biến Nghiệp vụ (Business Invariants)
1. **Chống Vòng Lặp Phân Cấp (No Cyclic Tree Hierarchy)**: Một phòng ban không được phép chọn chính nó hoặc bất kỳ phòng ban con cháu nào của nó làm `PhongBanChaId`.
2. **Tính Duy Nhất Mã Phòng Ban (Unique Department Code)**: Trong cùng một Chi nhánh, `MaPhongBan` là duy nhất (Unique Index trên `[ChiNhanhId, MaPhongBan]`).
3. **Ràng Buộc Xóa (Restricted Deletion)**: Không được xóa phòng ban nếu:
   - Đang có phòng ban con trực thuộc.
   - Đang có nhân viên hoạt động thuộc phòng ban đó.
   - Đã phát sinh ít nhất một dòng chứng từ bút toán (`ChiTietButToan`) gắn với phòng ban đó. (Chỉ được phép chuyển `DangHoatDong = false`).
4. **Phù Hợp Tài Khoản Mặc Định Theo TT99**:
   - `TaiKhoanChiPhiMacDinh` bắt buộc phải là tài khoản chi phí hợp lệ (Đầu 6: 6421, 6422; hoặc Đầu 1: 154).
   - **Nghiêm cấm tuyệt đối gán TK 911** làm tài khoản chi phí mặc định.

---

## 5. LUỒNG DỮ LIỆU & QUY TRÌNH HẠCH TOÁN CHI PHÍ THEO PHÒNG BAN

### 5.1. Luồng Hạch Toán Chi Phí Lương Tự Động Phân Bổ (Payroll Departmental Split Flow)

```
+------------------------------------------------------------------------------------------------------+
|                           QUY TRÌNH PHÂN BỔ BẢNG LƯƠNG THEO PHÒNG BAN (TT99)                        |
+------------------------------------------------------------------------------------------------------+

      [ Danh sách Nhân Viên ]
                 |
                 +--> Nhân viên A (Phòng Kinh Doanh -> TK 6421) -> Lương 20.000.000
                 +--> Nhân viên B (Phòng Kế Toán    -> TK 6422) -> Lương 25.000.000
                 +--> Nhân viên C (Khoa Đào tạo CNTT-> TK 154 ) -> Lương 30.000.000
                 |
                 v
      [ Chạy Bảng Tính Lương Tháng ]
                 |
                 v
      [ Tự Động Gom Nhóm Chi Phí Theo Phòng Ban ]
                 |
                 +---> Nhóm 1: Nợ 6421 (Phòng Kinh Doanh)        : 20.000.000
                 |             Có 334 (Phải trả người lao động)  : 20.000.000
                 |
                 +---> Nhóm 2: Nợ 6422 (Phòng Kế Toán)           : 25.000.000
                 |             Có 334 (Phải trả người lao động)  : 25.000.000
                 |
                 +---> Nhóm 3: Nợ 154  (Khoa Đào tạo CNTT)       : 30.000.000
                               Có 334 (Phải trả người lao động)  : 30.000.000
                 |
                 v
      [ Sinh Bút Toán Nhật Ký Chung (ChiTietButToan) ]
                 |
                 +--> Dòng 1: Nợ 6421 / Có 334 | SoTien: 20tr | PhongBanId = 1
                 +--> Dòng 2: Nợ 6422 / Có 334 | SoTien: 25tr | PhongBanId = 2
                 +--> Dòng 3: Nợ 154  / Có 334 | SoTien: 30tr | PhongBanId = 3
                 |
                 v
      [ Báo Cáo Kết Quả Kinh Doanh & Chi Phí Bộ Phận (Segment P&L) ]
```

### 5.2. Luồng Báo Cáo Lãi/Lỗ Theo Phòng Ban / Khoa (Departmental P&L Flow)

```
           +-------------------------------------------------------------+
           |               DOANH THU & CHI PHÍ BỘ PHẬN                   |
           +------------------------------+------------------------------+
                                          |
                    +---------------------+---------------------+
                    |                                           |
                    v                                           v
       [ DOANH THU BỘ PHẬN ]                        [ CHI PHÍ BỘ PHẬN ]
       (TK 511 gắn PhongBanId)                      (TK 642, 635, 154 gắn PhongBanId)
                    |                                           |
                    |                                           |
                    +---------------------+---------------------+
                                          |
                                          v
                       [ LỢI NHUẬN GỘP / THUẦN THEO BỘ PHẬN ]
                       = Doanh Thu Bộ Phận - Chi Phí Bộ Phận
                                          |
                                          v
                       [ ĐỐI CHIẾU VỚI BÁO CÁO TOÀN CÔNG TY ]
                       Tổng Lợi Nhuận Các Bộ Phận == Lợi Nhuận B02-DN
```

---

## 6. DANH SÁCH USE CASES CHI TIẾT

```
+--------------------------------------------------------------------------------------------------------+
|                                    MA TRẬN USE CASES CHO PHASE 7                                       |
+------------+--------------------------------------------+---------------+------------------------------+
| Mã UC      | Tên Use Case                               | Tác Nhân      | Mục Tiêu / Kỳ Vọng           |
+------------+--------------------------------------------+---------------+------------------------------+
| UC-DEP-01  | Xem cây cơ cấu tổ chức phòng ban           | Kế toán / HR  | Hiển thị cấu trúc đa tầng    |
| UC-DEP-02  | Thêm mới phòng ban / khoa chuyên môn       | Kế toán trưởng| Kiểm tra mã trùng, cấm cycle |
| UC-DEP-03  | Sửa thông tin & đổi phòng ban cha          | Kế toán trưởng| Kiểm tra tính hợp lệ cây     |
| UC-DEP-04  | Ngừng hoạt động / Xóa phòng ban            | Admin hệ thống| Chặn xóa nếu có chứng từ/NV  |
| UC-DEP-05  | Phân bổ nhân viên vào phòng ban            | HR / Kế toán  | Gán PhongBanId cho NhanVien  |
| UC-DEP-06  | Hạch toán chi phí chi tiết theo phòng ban  | Kế toán viên  | Chọn phòng ban trên bút toán |
| UC-DEP-07  | Bảng lương tự động tách tài khoản bộ phận  | Kế toán lương | Sinh bút toán Nợ 6421/6422   |
| UC-DEP-08  | Báo cáo kết quả hoạt động theo bộ phận/khoa| Kế toán trưởng| Báo cáo P&L phân đoạn IFRS 8 |
+------------+--------------------------------------------+---------------+------------------------------+
```

### Chi Tiết UC-DEP-02: Thêm Mới Phòng Ban / Khoa Chuyên Môn
- **Tác nhân**: Kế toán trưởng / Admin.
- **Tiền điều kiện**: Đã có Chi nhánh được chọn trong hệ thống.
- **Luồng chính (Happy Path)**:
  1. Người dùng vào menu `Hệ Thống` &rarr; `Phòng Ban & Trung Tâm Chi Phí` &rarr; nhấn `Thêm phòng ban`.
  2. Nhập Mã phòng ban (vd: `PB-KD-01`), Tên phòng ban (`Phòng Kinh Doanh 1`), chọn Chi nhánh, loại hình (`KinhDoanh`), tài khoản chi phí mặc định (`6421`).
  3. Chọn phòng ban cha (nếu là phòng con) hoặc để trống (nếu là phòng ban cấp cao nhất).
  4. Hệ thống kiểm tra:
     - `MaPhongBan` chưa tồn tại trong chi nhánh.
     - `TaiKhoanChiPhiMacDinh` không phải TK 911 và tồn tại trong hệ thống tài khoản TT99.
  5. Hệ thống lưu thực thể `PhongBan` và ghi nhận lịch sử.
  6. Chuyển hướng về Cây danh mục với thông báo thành công.
- **Luồng ngoại lệ (Exception Path)**:
  - Nếu `MaPhongBan` bị trùng: Báo lỗi `"Mã phòng ban đã tồn tại trong chi nhánh này"`.
  - Nếu chọn phòng ban cha tạo thành chu trình khép kín: Báo lỗi `"Không thể chọn phòng ban con cháu làm phòng ban cha (Cyclic dependency)"`.

---

## 7. ĐẶC TẢ GIAO DIỆN NGƯỜI DÙNG (UI/UX WIREFRAMES)

### 7.1. Wireframe Danh Mục Cơ Cấu Phòng Ban Dạng Cây (ASCII Art)

```
+--------------------------------------------------------------------------------------------------------+
| [ninjaTax]  Mua Hàng | Bán Hàng | Thu Tiền | Chi Tiền | Lương & Thuế | BCTC | Sổ Cái | HỆ THỐNG [v]   |
+--------------------------------------------------------------------------------------------------------+
| Trang chủ > Hệ thống > Cơ Cấu Phòng Ban & Trung Tâm Chi Phí                                            |
|                                                                                                        |
| CƠ CẤU PHÒNG BAN & TRUNG TÂM CHI PHÍ (COST CENTERS)                         [+ Thêm Phòng Ban Mới]     |
| Quản lý cấu trúc tổ chức, khoa chuyên môn, đơn vị đào tạo và thiết lập tài khoản chi phí ngầm định    |
|                                                                                                        |
| Chi Nhánh: [ Trụ sở chính Hà Nội [v] ]           Lọc loại: [ Tất cả loại hình [v] ]                    |
+--------------------------------------------------------------------------------------------------------+
|                                                                                                        |
| +----------------------------------------------------------------------------------------------------+ |
| | CƠ CẤU TỔ CHỨC DẠNG CÂY (ORGANIZATION HIERARCHY TREE)                                              | |
| +----------------------------------------------------------------------------------------------------+ |
| | [-] CÔNG TY CỔ PHẦN CÔNG NGHỆ NINJATAX VIỆT NAM                                                   | |
| |   |-- [Ban Giám Đốc] (Mã: BOD) | Trưởng ban: Nguyễn Văn Doanh | TK CP: 6422 | [Sửa] [Thêm con]        | |
| |   |-- [+] [Khối Kinh Doanh & Tiếp Thị] (Mã: BLOCK-BIZ) | Trưởng ban: Lê Bán Hàng                     | |
| |   |   |-- [Phòng Kinh Doanh Miền Bắc] (Mã: PB-KDB) | TK CP: 6421 | NS: 12 người | [Sửa] [Thêm con] | |
| |   |   \-- [Phòng Marketing & Digital] (Mã: PB-MKT) | TK CP: 6421 | NS: 6 người  | [Sửa] [Thêm con] | |
| |   |-- [+] [Khối Công Nghệ & Sản Phẩm] (Mã: BLOCK-TECH) | TK CP: 6422                              | |
| |   |   |-- [Khoa / Viện Nghiên Cứu AI] (Mã: FAC-AI) | TK CP: 154  | Profit Center: CÓ | [Sửa]        | |
| |   |   \-- [Phòng Phát Triển Phần Mềm] (Mã: PB-DEV) | TK CP: 154  | NS: 25 người | [Sửa]           | |
| |   \-- [Phòng Kế Toán & Tài Chính] (Mã: PB-KTTC) | Trưởng phòng: Trần Thị Kế Toán | TK: 6422 [Sửa]  | |
| +----------------------------------------------------------------------------------------------------+ |
|                                                                                                        |
| TỔNG HỢP: 7 Phòng ban/Khoa | 58 Nhân sự đã phân bổ | 0 Nhân sự chưa phân bổ phòng ban                  |
+--------------------------------------------------------------------------------------------------------+
```

### 7.2. Wireframe Báo Cáo Lãi/Lỗ Theo Phòng Ban / Khoa (Segment P&L Report)

```
+--------------------------------------------------------------------------------------------------------+
| BÁO CÁO KẾT QUẢ HOẠT ĐỘNG KINH DOANH THEO BỘ PHẬN / KHOA (SEGMENT P&L REPORT)                          |
| Kỳ báo cáo: Năm 2026 | Chế độ kế toán: TT 99/2025/TT-BTC | Đơn vị: VND                                |
+----+--------------------------------+-----------------+-----------------+---------------+--------------+
| STT| Chỉ Tiêu Doanh Thu / Chi Phí   | Khối Kinh Doanh | Viện Đào tạo AI | Khối Văn Phòng| TOÀN DOANH   |
|    |                                | (Cost Ctr: BIZ) | (Profit Ctr: AI)| (Cost Ctr: ADM| NGHIỆP       |
+----+--------------------------------+-----------------+-----------------+---------------+--------------+
| 1  | 1. Doanh thu thuần (TK 511)    |   1.200.000.000 |     850.000.000 |             0 | 2.050.000.000|
| 2  | 2. Giá vốn & DV trực tiếp (154)|               0 |     320.000.000 |             0 |   320.000.000|
| 3  | 3. Lợi nhuận gộp (1 - 2)       |   1.200.000.000 |     530.000.000 |             0 | 1.730.000.000|
| 4  | 4. Chi phí bán hàng (TK 6421)  |     280.000.000 |      45.000.000 |             0 |   325.000.000|
| 5  | 5. Chi phí QLDN (TK 6422)      |      60.000.000 |      50.000.000 |    310.000.000|   420.000.000|
| 6  | 6. Lợi nhuận thuần bộ phận     |   + 860.000.000 |   + 435.000.000 | - 310.000.000 | + 985.000.000|
+----+--------------------------------+-----------------+-----------------+---------------+--------------+
| Ghi chú: Tổng Lợi nhuận thuần bộ phận (985.000.000 VND) khớp 100% với Báo cáo B02-DN toàn công ty.     |
+--------------------------------------------------------------------------------------------------------+
```

---

## 8. LỘ TRÌNH TRIỂN KHAI THEO LÁT CẮT DỌC (VERTICAL SLICES ROADMAP & TDD)

Triển khai theo nguyên tắc: **Simple to Complex &bull; Core to Edge &bull; Seam Test-First &bull; Không làm vỡ bất biến TT99**.

```
+--------------------------------------------------------------------------------------------------------+
|                                  LỘ TRÌNH 5 LÁT CẮT DỌC TRIỂN KHAI PHASE 7                             |
+--------------------------------------------------------------------------------------------------------+
  [Slice 1: Core Domain & Tree Invariants]
     |---> Thực thể PhongBan, LoaiPhongBan, Validation chống chu trình đệ quy (Anti-cyclic tree validation)
     |---> Kiểm thử TDD: DepartmentHierarchyTests (100% Invariant checking)
     |
  [Slice 2: Database Schema & Migration]
     |---> Quan hệ PhongBan <-> ChiNhanh, PhongBan <-> NhanVien, PhongBan <-> ChiTietButToan
     |---> Migration EF Core Sqlite & Nạp dữ liệu phòng ban mặc định (Seed Data)
     |
  [Slice 3: Service Layer & Payroll Auto-split Seam]
     |---> IDepartmentService & DepartmentService
     |---> Tự động phân loại Nợ 6421 / Nợ 6422 / Nợ 154 trong TienLuongService theo phòng ban
     |---> Kiểm thử TDD: DepartmentalPayrollCostAllocationTests
     |
  [Slice 4: UI & Controller]
     |---> DepartmentController: Index (Tree & Table View), Create, Edit, Delete
     |---> Views/Department/Index.cshtml, Create.cshtml, Edit.cshtml
     |---> Menu Hệ Thống: Phòng Ban & Bộ Phận
     |
  [Slice 5: Segment P&L Reporting & Verification]
     |---> Báo cáo Lợi nhuận Bộ phận / Khoa (Segment P&L Service & View)
     |---> Đối chiếu bất biến: Tổng chi phí bộ phận == Chi phí B02-DN
     |---> Code Review & Đồng bộ Git Master
```

---

## 9. CHECKLIST BẢO VỆ CHUẨN MỰC TT99/2025/TT-BTC
- [x] Tuyệt đối không sử dụng Tài khoản 911 trong bất kỳ bút toán phân bổ hoặc kết chuyển bộ phận nào.
- [x] Đảm bảo phân loại chính xác giữa Chi phí bán hàng (TK 6421) và Chi phí quản lý doanh nghiệp (TK 6422).
- [x] Thuật toán duyệt cây phân cấp phòng ban chặn đứng vòng lặp vô tận (Infinite recursion protection).
- [x] Kiểm thử tự động bao phủ toàn bộ các Seam trước khi báo cáo hoàn thành.
