using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninjaTax.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddPayrollAndTaxModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BangChamCongThang",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KyKeToan = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Nam = table.Column<int>(type: "INTEGER", nullable: false),
                    Thang = table.Column<int>(type: "INTEGER", nullable: false),
                    SoNgayCongChuan = table.Column<int>(type: "INTEGER", nullable: false),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangChamCongThang", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BangLuongThang",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoChungTu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    KyKeToan = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    NgayLap = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayGhiSo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SoNgayCongChuan = table.Column<int>(type: "INTEGER", nullable: false),
                    TongQuyLuong = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongBaoHiemDnGanh = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongBaoHiemNldGanh = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongThueTncn = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongThucLinh = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ButToanChiPhiLuongId = table.Column<long>(type: "INTEGER", nullable: true),
                    ButToanBaoHiemDnId = table.Column<long>(type: "INTEGER", nullable: true),
                    ButToanKhauTruLuongId = table.Column<long>(type: "INTEGER", nullable: true),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangLuongThang", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BangLuongThang_ButToan_ButToanBaoHiemDnId",
                        column: x => x.ButToanBaoHiemDnId,
                        principalTable: "ButToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BangLuongThang_ButToan_ButToanChiPhiLuongId",
                        column: x => x.ButToanChiPhiLuongId,
                        principalTable: "ButToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BangLuongThang_ButToan_ButToanKhauTruLuongId",
                        column: x => x.ButToanKhauTruLuongId,
                        principalTable: "ButToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaNhanVien = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    HoTen = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SoCccd = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    MaSoThue = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    SoSoBhxh = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PhongBan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ChucVu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LoaiHopDong = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayVaoLam = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayKetThucHd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LuongCoBan = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    LuongDongBaoHiem = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    PhuCapAnTrua = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    PhuCapTrachNhiem = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    PhuCapDienThoai = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    PhuCapTrangPhuc = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoNguoiPhuThuoc = table.Column<int>(type: "INTEGER", nullable: false),
                    CoCamKet08 = table.Column<bool>(type: "INTEGER", nullable: false),
                    DongBaoHiem = table.Column<bool>(type: "INTEGER", nullable: false),
                    LaDoanVienCongDoan = table.Column<bool>(type: "INTEGER", nullable: false),
                    SoTaiKhoanNganHang = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TenNganHang = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DangLamViec = table.Column<bool>(type: "INTEGER", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietChamCong",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BangChamCongThangId = table.Column<long>(type: "INTEGER", nullable: false),
                    NhanVienId = table.Column<long>(type: "INTEGER", nullable: false),
                    SoNgayDiLam = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoNgayNghiPhep = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoNgayNghiLe = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoNgayNghiKhongLuong = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoNgayNghiOmBhxh = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoNgayNghiThaiSan = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    GioLamThemNgayThuong = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    GioLamThemNgayNghi = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    GioLamThemNgayLe = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongCongTinhLuong = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietChamCong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietChamCong_BangChamCongThang_BangChamCongThangId",
                        column: x => x.BangChamCongThangId,
                        principalTable: "BangChamCongThang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietChamCong_NhanVien_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietLuongNhanVien",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BangLuongThangId = table.Column<long>(type: "INTEGER", nullable: false),
                    NhanVienId = table.Column<long>(type: "INTEGER", nullable: false),
                    LuongThoiGian = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    LuongLamThemGio = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    LuongOtMienThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    PhuCapChiuThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    PhuCapMienThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TienThuong = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongThuNhap = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    LuongDongBaoHiem = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    BhxhNld = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    BhytNld = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    BhtnNld = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongBaoHiemNld = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    BhxhDn = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    BhytDn = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    BhtnDn = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    KpcdDn = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongBaoHiemDn = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    GiamTruBanThan = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    GiamTruNguoiPhuThuoc = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThuNhapTinhThue = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueTncnKhauTru = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TamUng = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThucLinh = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietLuongNhanVien", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietLuongNhanVien_BangLuongThang_BangLuongThangId",
                        column: x => x.BangLuongThangId,
                        principalTable: "BangLuongThang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietLuongNhanVien_NhanVien_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BangChamCongThang_KyKeToan",
                table: "BangChamCongThang",
                column: "KyKeToan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BangLuongThang_ButToanBaoHiemDnId",
                table: "BangLuongThang",
                column: "ButToanBaoHiemDnId");

            migrationBuilder.CreateIndex(
                name: "IX_BangLuongThang_ButToanChiPhiLuongId",
                table: "BangLuongThang",
                column: "ButToanChiPhiLuongId");

            migrationBuilder.CreateIndex(
                name: "IX_BangLuongThang_ButToanKhauTruLuongId",
                table: "BangLuongThang",
                column: "ButToanKhauTruLuongId");

            migrationBuilder.CreateIndex(
                name: "IX_BangLuongThang_KyKeToan",
                table: "BangLuongThang",
                column: "KyKeToan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BangLuongThang_SoChungTu",
                table: "BangLuongThang",
                column: "SoChungTu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietChamCong_BangChamCongThangId_NhanVienId",
                table: "ChiTietChamCong",
                columns: new[] { "BangChamCongThangId", "NhanVienId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietChamCong_NhanVienId",
                table: "ChiTietChamCong",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietLuongNhanVien_BangLuongThangId_NhanVienId",
                table: "ChiTietLuongNhanVien",
                columns: new[] { "BangLuongThangId", "NhanVienId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietLuongNhanVien_NhanVienId",
                table: "ChiTietLuongNhanVien",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_MaNhanVien",
                table: "NhanVien",
                column: "MaNhanVien",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietChamCong");

            migrationBuilder.DropTable(
                name: "ChiTietLuongNhanVien");

            migrationBuilder.DropTable(
                name: "BangChamCongThang");

            migrationBuilder.DropTable(
                name: "BangLuongThang");

            migrationBuilder.DropTable(
                name: "NhanVien");
        }
    }
}
