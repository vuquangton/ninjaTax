using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninjaTax.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddPhase6CompanyAndBranchConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaoCaoTaiChinhNam",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NamTaiChinh = table.Column<int>(type: "INTEGER", nullable: false),
                    SoChungTu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NgayLap = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayKhoaSo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TongTaiSan = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongNguonVon = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DoanhThuThuan = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    LoiNhuanGop = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    LoiNhuanTruocThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueTndnHienHanh = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    LoiNhuanSauThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    LuuChuyenThuanTrongKy = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TienDauKy = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TienCuoiKy = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoTaiChinhNam", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuyetToanThueTncn",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NamQuyetToan = table.Column<int>(type: "INTEGER", nullable: false),
                    SoChungTu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NgayLap = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TongSoNhanVienQuyetToan = table.Column<int>(type: "INTEGER", nullable: false),
                    SoNhanVienUyQuyen = table.Column<int>(type: "INTEGER", nullable: false),
                    TongThuNhapChiuThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongThuNhapMienThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongGiamTruGiaCanh = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongBaoHiemBatBuoc = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongThuNhapTinhThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongThueDaKhauTru = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongThuePhaiNopSauQtt = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongThueNopThua = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongThueConPhaiNopThem = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetToanThueTncn", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuyetToanThueTndn",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NamQuyetToan = table.Column<int>(type: "INTEGER", nullable: false),
                    SoChungTu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NgayLap = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChiTieuA1_LoiNhuanKeToan = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ChiTieuB4_ChiPhiKhongDuocTru = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ChiTieuB7_ThuNhapMienThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ChiTieuB14_ThuNhapChiuThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ChiTieuC1_ThuNhapTinhThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ChiTieuC4_LoKetChuyen = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueSuatPhanTram = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ChiTieuC7_ThueTndnPhaiNop = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueTndnTamNopQ1 = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueTndnTamNopQ2 = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueTndnTamNopQ3 = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueTndnTamNopQ4 = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongTamNop4Quy = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TyLeTamNopPhanTram = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ViPhamQuyTac80PhanTram = table.Column<bool>(type: "INTEGER", nullable: false),
                    SoTienNopThieu80 = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoNgayChamNop = table.Column<int>(type: "INTEGER", nullable: false),
                    TienPhatChamNopDuKien = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetToanThueTndn", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxAuditRiskShieldReport",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NamTaiChinh = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayQuet = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TongSoPhatHien = table.Column<int>(type: "INTEGER", nullable: false),
                    SoCanhBaoDoNghiemTrong = table.Column<int>(type: "INTEGER", nullable: false),
                    SoCanhBaoVangChuY = table.Column<int>(type: "INTEGER", nullable: false),
                    TongTienChiPhiRuiRo = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongTienThueTruyThuUocTinh = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongTienPhatChamNopUocTinh = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxAuditRiskShieldReport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThongTinDoanhNghiep",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaDoanhNghiep = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenDoanhNghiep = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TenGiaoDich = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    TenTiengAnh = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    MaSoThue = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DiaChiTruSo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TinhThanhPho = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    QuanHuyen = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MaCoQuanThueQuanLy = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TenCoQuanThueQuanLy = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    NguoiDaiDienPhapLuat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ChucDanhNguoiDaiDien = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    GiamDoc = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    KeToanTruong = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    NguoiLapBieu = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ThuQuy = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SoDienThoai = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    VonDieuLe = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    LogoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NgayThanhLap = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongTinDoanhNghiep", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietChiTieuBctc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaoCaoTaiChinhNamId = table.Column<long>(type: "INTEGER", nullable: false),
                    LoaiBaoCao = table.Column<int>(type: "INTEGER", nullable: false),
                    MaChiTieu = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TenChiTieu = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ThuyetMinh = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    SoDauNam = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoCuoiNam = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    CongThucThietLap = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietChiTieuBctc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietChiTieuBctc_BaoCaoTaiChinhNam_BaoCaoTaiChinhNamId",
                        column: x => x.BaoCaoTaiChinhNamId,
                        principalTable: "BaoCaoTaiChinhNam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BangKeQttTncn051",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuyetToanThueTncnId = table.Column<long>(type: "INTEGER", nullable: false),
                    NhanVienId = table.Column<long>(type: "INTEGER", nullable: false),
                    HoTen = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    MaSoThue = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    SoCccd = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CaNhanUyQuyenQuyetToan = table.Column<bool>(type: "INTEGER", nullable: false),
                    TongThuNhapChiuThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThuNhapMienThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    GiamTruBanThan = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoNguoiPhuThuoc = table.Column<int>(type: "INTEGER", nullable: false),
                    GiamTruNguoiPhuThuoc = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    BaoHiemBatBuoc = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThuNhapTinhThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueDaKhauTruTrongNam = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThuePhaiNopSauQuyetToan = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueNopThua = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueConPhaiNop = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangKeQttTncn051", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BangKeQttTncn051_NhanVien_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BangKeQttTncn051_QuyetToanThueTncn_QuyetToanThueTncnId",
                        column: x => x.QuyetToanThueTncnId,
                        principalTable: "QuyetToanThueTncn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BangKeQttTncn052",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuyetToanThueTncnId = table.Column<long>(type: "INTEGER", nullable: false),
                    NhanVienId = table.Column<long>(type: "INTEGER", nullable: false),
                    HoTen = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    MaSoThue = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    SoCccd = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CoCamKet08 = table.Column<bool>(type: "INTEGER", nullable: false),
                    TongThuNhapChiuThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueTncnDaKhauTru10 = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangKeQttTncn052", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BangKeQttTncn052_NhanVien_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BangKeQttTncn052_QuyetToanThueTncn_QuyetToanThueTncnId",
                        column: x => x.QuyetToanThueTncnId,
                        principalTable: "QuyetToanThueTncn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiPhiKhongHopLyB4",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuyetToanThueTndnId = table.Column<long>(type: "INTEGER", nullable: false),
                    LoaiViPham = table.Column<int>(type: "INTEGER", nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SoTien = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoChungTuLienQuan = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    NgayChungTu = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CanCuPhapLy = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiPhiKhongHopLyB4", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiPhiKhongHopLyB4_QuyetToanThueTndn_QuyetToanThueTndnId",
                        column: x => x.QuyetToanThueTndnId,
                        principalTable: "QuyetToanThueTndn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaxRiskFinding",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaxAuditRiskShieldReportId = table.Column<long>(type: "INTEGER", nullable: false),
                    LoaiBay = table.Column<int>(type: "INTEGER", nullable: false),
                    MucDo = table.Column<int>(type: "INTEGER", nullable: false),
                    TieuDe = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    MoTaChiTiet = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    MaChungTuLienQuan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NgayPhatSinh = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SoTienViPham = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoTienThueRuiRo = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoTienPhatDuKien = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    CanCuPhapLy = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    BienPhapKhacPhuc = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRiskFinding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxRiskFinding_TaxAuditRiskShieldReport_TaxAuditRiskShieldReportId",
                        column: x => x.TaxAuditRiskShieldReportId,
                        principalTable: "TaxAuditRiskShieldReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CauHinhKeToan",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DoanhNghiepId = table.Column<long>(type: "INTEGER", nullable: false),
                    CheDoKeToan = table.Column<int>(type: "INTEGER", nullable: false),
                    DonViTienTe = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    NgayBatDauNienDo = table.Column<int>(type: "INTEGER", nullable: false),
                    ThangBatDauNienDo = table.Column<int>(type: "INTEGER", nullable: false),
                    PhuongPhapThueGtgt = table.Column<int>(type: "INTEGER", nullable: false),
                    PhuongPhapXuatKho = table.Column<int>(type: "INTEGER", nullable: false),
                    PhuongPhapKhauHaoTscd = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayKhoaSo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CanhBaoChiVuotQuy = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanhBaoXuatAmKho = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanhBaoHoaDonTren20TrTienMat = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhKeToan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauHinhKeToan_ThongTinDoanhNghiep_DoanhNghiepId",
                        column: x => x.DoanhNghiepId,
                        principalTable: "ThongTinDoanhNghiep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiNhanh",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DoanhNghiepId = table.Column<long>(type: "INTEGER", nullable: false),
                    MaChiNhanh = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenChiNhanh = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    MaSoThueChiNhanh = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    LoaiChiNhanh = table.Column<int>(type: "INTEGER", nullable: false),
                    KeKhaiThueGtgtRieng = table.Column<bool>(type: "INTEGER", nullable: false),
                    KeKhaiThueTncnRieng = table.Column<bool>(type: "INTEGER", nullable: false),
                    DiaChi = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TinhThanhPho = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MaCoQuanThueQuanLyRieng = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TenCoQuanThueQuanLyRieng = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    NguoiDungDau = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SoDienThoai = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DangHoatDong = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiNhanh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiNhanh_ThongTinDoanhNghiep_DoanhNghiepId",
                        column: x => x.DoanhNghiepId,
                        principalTable: "ThongTinDoanhNghiep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CauHinhHoaDonDienTu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChiNhanhId = table.Column<long>(type: "INTEGER", nullable: false),
                    NhaCungCap = table.Column<int>(type: "INTEGER", nullable: false),
                    DuongDanApi = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    TaiKhoanApi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MatKhauApi = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    MauSoHoaDon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    KyHieuHoaDon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LoaiChungThuSo = table.Column<int>(type: "INTEGER", nullable: false),
                    SeriChungThuSo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TuDongPhatHanh = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhHoaDonDienTu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauHinhHoaDonDienTu_ChiNhanh_ChiNhanhId",
                        column: x => x.ChiNhanhId,
                        principalTable: "ChiNhanh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BangKeQttTncn051_NhanVienId",
                table: "BangKeQttTncn051",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_BangKeQttTncn051_QuyetToanThueTncnId",
                table: "BangKeQttTncn051",
                column: "QuyetToanThueTncnId");

            migrationBuilder.CreateIndex(
                name: "IX_BangKeQttTncn052_NhanVienId",
                table: "BangKeQttTncn052",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_BangKeQttTncn052_QuyetToanThueTncnId",
                table: "BangKeQttTncn052",
                column: "QuyetToanThueTncnId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhHoaDonDienTu_ChiNhanhId",
                table: "CauHinhHoaDonDienTu",
                column: "ChiNhanhId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhKeToan_DoanhNghiepId",
                table: "CauHinhKeToan",
                column: "DoanhNghiepId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiNhanh_DoanhNghiepId_MaChiNhanh",
                table: "ChiNhanh",
                columns: new[] { "DoanhNghiepId", "MaChiNhanh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiPhiKhongHopLyB4_QuyetToanThueTndnId",
                table: "ChiPhiKhongHopLyB4",
                column: "QuyetToanThueTndnId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietChiTieuBctc_BaoCaoTaiChinhNamId",
                table: "ChiTietChiTieuBctc",
                column: "BaoCaoTaiChinhNamId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRiskFinding_TaxAuditRiskShieldReportId",
                table: "TaxRiskFinding",
                column: "TaxAuditRiskShieldReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ThongTinDoanhNghiep_MaSoThue",
                table: "ThongTinDoanhNghiep",
                column: "MaSoThue",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BangKeQttTncn051");

            migrationBuilder.DropTable(
                name: "BangKeQttTncn052");

            migrationBuilder.DropTable(
                name: "CauHinhHoaDonDienTu");

            migrationBuilder.DropTable(
                name: "CauHinhKeToan");

            migrationBuilder.DropTable(
                name: "ChiPhiKhongHopLyB4");

            migrationBuilder.DropTable(
                name: "ChiTietChiTieuBctc");

            migrationBuilder.DropTable(
                name: "TaxRiskFinding");

            migrationBuilder.DropTable(
                name: "QuyetToanThueTncn");

            migrationBuilder.DropTable(
                name: "ChiNhanh");

            migrationBuilder.DropTable(
                name: "QuyetToanThueTndn");
                name: "CauHinhKeToan");

            migrationBuilder.DropTable(
                name: "BaoCaoTaiChinhNam");

            migrationBuilder.DropTable(
                name: "TaxAuditRiskShieldReport");

            migrationBuilder.DropTable(
                name: "ThongTinDoanhNghiep");
        }
    }
}
