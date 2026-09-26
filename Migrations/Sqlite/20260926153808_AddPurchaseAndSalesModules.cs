using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninjaTax.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddPurchaseAndSalesModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HoaDonBanHang",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoChungTu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NgayChungTu = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayHachToan = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KHMauSo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    KyHieu = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SoHoaDon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NgayHoaDon = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MaCoQuanThue = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ChuKySo = table.Column<string>(type: "TEXT", nullable: true),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    KhachHangId = table.Column<long>(type: "INTEGER", nullable: false),
                    MaSoThueKH = table.Column<string>(type: "TEXT", nullable: true),
                    TenKhachHang = table.Column<string>(type: "TEXT", nullable: true),
                    DiaChiKH = table.Column<string>(type: "TEXT", nullable: true),
                    DienGiai = table.Column<string>(type: "TEXT", nullable: false),
                    HanThanhToan = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TongTienHang = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongTienChietKhau = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongTienThueVat = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongThanhToan = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DaThuTien = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    BanHangKiemXuatKho = table.Column<bool>(type: "INTEGER", nullable: false),
                    ButToanDoanhThuId = table.Column<long>(type: "INTEGER", nullable: true),
                    ButToanGiaVonId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonBanHang", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoaDonBanHang_ButToan_ButToanDoanhThuId",
                        column: x => x.ButToanDoanhThuId,
                        principalTable: "ButToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HoaDonBanHang_ButToan_ButToanGiaVonId",
                        column: x => x.ButToanGiaVonId,
                        principalTable: "ButToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HoaDonBanHang_DoiTuong_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "DoiTuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoaDonMuaHang",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoChungTu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NgayChungTu = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayHachToan = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KHMauSoHoaDon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    KyHieuHoaDon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SoHoaDon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NgayHoaDon = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MaTraCuuHdt = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NhaCungCapId = table.Column<long>(type: "INTEGER", nullable: false),
                    MaSoThueNCC = table.Column<string>(type: "TEXT", nullable: true),
                    TenNCC = table.Column<string>(type: "TEXT", nullable: true),
                    DiaChiNCC = table.Column<string>(type: "TEXT", nullable: true),
                    DienGiai = table.Column<string>(type: "TEXT", nullable: false),
                    HanThanhToan = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TongTienHang = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongTienChietKhau = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongTienThueVat = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongThanhToan = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DaThanhToan = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    MuaHangKiemKho = table.Column<bool>(type: "INTEGER", nullable: false),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    ButToanId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonMuaHang", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoaDonMuaHang_ButToan_ButToanId",
                        column: x => x.ButToanId,
                        principalTable: "ButToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HoaDonMuaHang_DoiTuong_NhaCungCapId",
                        column: x => x.NhaCungCapId,
                        principalTable: "DoiTuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VatTuHangHoa",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaVatTu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenVatTu = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DonViTinh = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LoaiVatTu = table.Column<int>(type: "INTEGER", nullable: false),
                    TaiKhoanKhoId = table.Column<long>(type: "INTEGER", nullable: true),
                    TaiKhoanDoanhThuId = table.Column<long>(type: "INTEGER", nullable: true),
                    TaiKhoanGiaVonId = table.Column<long>(type: "INTEGER", nullable: true),
                    ThueSuatVatMacDinh = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DonGiaMuaGanNhat = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DonGiaBanTieuChuan = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DangTheoDoiTonKho = table.Column<bool>(type: "INTEGER", nullable: false),
                    DangHoatDong = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VatTuHangHoa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VatTuHangHoa_TaiKhoan_TaiKhoanDoanhThuId",
                        column: x => x.TaiKhoanDoanhThuId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VatTuHangHoa_TaiKhoan_TaiKhoanGiaVonId",
                        column: x => x.TaiKhoanGiaVonId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VatTuHangHoa_TaiKhoan_TaiKhoanKhoId",
                        column: x => x.TaiKhoanKhoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoiTruCongNo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Loai = table.Column<int>(type: "INTEGER", nullable: false),
                    DoiTuongId = table.Column<long>(type: "INTEGER", nullable: false),
                    NgayDoiTru = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoaDonBanHangId = table.Column<long>(type: "INTEGER", nullable: true),
                    HoaDonMuaHangId = table.Column<long>(type: "INTEGER", nullable: true),
                    ButToanId = table.Column<long>(type: "INTEGER", nullable: true),
                    SoTienDoiTru = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoiTruCongNo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoiTruCongNo_ButToan_ButToanId",
                        column: x => x.ButToanId,
                        principalTable: "ButToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoiTruCongNo_DoiTuong_DoiTuongId",
                        column: x => x.DoiTuongId,
                        principalTable: "DoiTuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoiTruCongNo_HoaDonBanHang_HoaDonBanHangId",
                        column: x => x.HoaDonBanHangId,
                        principalTable: "HoaDonBanHang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoiTruCongNo_HoaDonMuaHang_HoaDonMuaHangId",
                        column: x => x.HoaDonMuaHangId,
                        principalTable: "HoaDonMuaHang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietHoaDonBan",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HoaDonBanHangId = table.Column<long>(type: "INTEGER", nullable: false),
                    DongSo = table.Column<int>(type: "INTEGER", nullable: false),
                    VatTuHangHoaId = table.Column<long>(type: "INTEGER", nullable: false),
                    SoLuong = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DonGia = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThanhTien = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TiLeChietKhau = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TienChietKhau = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueSuatVat = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TienThueVat = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TaiKhoanNoId = table.Column<long>(type: "INTEGER", nullable: false),
                    TaiKhoanDoanhThuId = table.Column<long>(type: "INTEGER", nullable: false),
                    TaiKhoanThueId = table.Column<long>(type: "INTEGER", nullable: false),
                    DonGiaVon = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TaiKhoanGiaVonId = table.Column<long>(type: "INTEGER", nullable: true),
                    TaiKhoanKhoId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietHoaDonBan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonBan_HoaDonBanHang_HoaDonBanHangId",
                        column: x => x.HoaDonBanHangId,
                        principalTable: "HoaDonBanHang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonBan_TaiKhoan_TaiKhoanDoanhThuId",
                        column: x => x.TaiKhoanDoanhThuId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonBan_TaiKhoan_TaiKhoanGiaVonId",
                        column: x => x.TaiKhoanGiaVonId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonBan_TaiKhoan_TaiKhoanKhoId",
                        column: x => x.TaiKhoanKhoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonBan_TaiKhoan_TaiKhoanNoId",
                        column: x => x.TaiKhoanNoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonBan_TaiKhoan_TaiKhoanThueId",
                        column: x => x.TaiKhoanThueId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonBan_VatTuHangHoa_VatTuHangHoaId",
                        column: x => x.VatTuHangHoaId,
                        principalTable: "VatTuHangHoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietHoaDonMua",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HoaDonMuaHangId = table.Column<long>(type: "INTEGER", nullable: false),
                    DongSo = table.Column<int>(type: "INTEGER", nullable: false),
                    VatTuHangHoaId = table.Column<long>(type: "INTEGER", nullable: false),
                    SoLuong = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DonGia = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThanhTien = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TiLeChietKhau = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TienChietKhau = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThueSuatVat = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TienThueVat = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TaiKhoanNoId = table.Column<long>(type: "INTEGER", nullable: false),
                    TaiKhoanThueId = table.Column<long>(type: "INTEGER", nullable: false),
                    TaiKhoanCoId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietHoaDonMua", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonMua_HoaDonMuaHang_HoaDonMuaHangId",
                        column: x => x.HoaDonMuaHangId,
                        principalTable: "HoaDonMuaHang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonMua_TaiKhoan_TaiKhoanCoId",
                        column: x => x.TaiKhoanCoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonMua_TaiKhoan_TaiKhoanNoId",
                        column: x => x.TaiKhoanNoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonMua_TaiKhoan_TaiKhoanThueId",
                        column: x => x.TaiKhoanThueId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonMua_VatTuHangHoa_VatTuHangHoaId",
                        column: x => x.VatTuHangHoaId,
                        principalTable: "VatTuHangHoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonBan_HoaDonBanHangId",
                table: "ChiTietHoaDonBan",
                column: "HoaDonBanHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonBan_TaiKhoanDoanhThuId",
                table: "ChiTietHoaDonBan",
                column: "TaiKhoanDoanhThuId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonBan_TaiKhoanGiaVonId",
                table: "ChiTietHoaDonBan",
                column: "TaiKhoanGiaVonId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonBan_TaiKhoanKhoId",
                table: "ChiTietHoaDonBan",
                column: "TaiKhoanKhoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonBan_TaiKhoanNoId",
                table: "ChiTietHoaDonBan",
                column: "TaiKhoanNoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonBan_TaiKhoanThueId",
                table: "ChiTietHoaDonBan",
                column: "TaiKhoanThueId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonBan_VatTuHangHoaId",
                table: "ChiTietHoaDonBan",
                column: "VatTuHangHoaId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonMua_HoaDonMuaHangId",
                table: "ChiTietHoaDonMua",
                column: "HoaDonMuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonMua_TaiKhoanCoId",
                table: "ChiTietHoaDonMua",
                column: "TaiKhoanCoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonMua_TaiKhoanNoId",
                table: "ChiTietHoaDonMua",
                column: "TaiKhoanNoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonMua_TaiKhoanThueId",
                table: "ChiTietHoaDonMua",
                column: "TaiKhoanThueId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonMua_VatTuHangHoaId",
                table: "ChiTietHoaDonMua",
                column: "VatTuHangHoaId");

            migrationBuilder.CreateIndex(
                name: "IX_DoiTruCongNo_ButToanId",
                table: "DoiTruCongNo",
                column: "ButToanId");

            migrationBuilder.CreateIndex(
                name: "IX_DoiTruCongNo_DoiTuongId",
                table: "DoiTruCongNo",
                column: "DoiTuongId");

            migrationBuilder.CreateIndex(
                name: "IX_DoiTruCongNo_HoaDonBanHangId",
                table: "DoiTruCongNo",
                column: "HoaDonBanHangId");

            migrationBuilder.CreateIndex(
                name: "IX_DoiTruCongNo_HoaDonMuaHangId",
                table: "DoiTruCongNo",
                column: "HoaDonMuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonBanHang_ButToanDoanhThuId",
                table: "HoaDonBanHang",
                column: "ButToanDoanhThuId");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonBanHang_ButToanGiaVonId",
                table: "HoaDonBanHang",
                column: "ButToanGiaVonId");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonBanHang_KhachHangId",
                table: "HoaDonBanHang",
                column: "KhachHangId");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonBanHang_SoChungTu",
                table: "HoaDonBanHang",
                column: "SoChungTu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonMuaHang_ButToanId",
                table: "HoaDonMuaHang",
                column: "ButToanId");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonMuaHang_NhaCungCapId",
                table: "HoaDonMuaHang",
                column: "NhaCungCapId");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonMuaHang_SoChungTu",
                table: "HoaDonMuaHang",
                column: "SoChungTu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VatTuHangHoa_MaVatTu",
                table: "VatTuHangHoa",
                column: "MaVatTu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VatTuHangHoa_TaiKhoanDoanhThuId",
                table: "VatTuHangHoa",
                column: "TaiKhoanDoanhThuId");

            migrationBuilder.CreateIndex(
                name: "IX_VatTuHangHoa_TaiKhoanGiaVonId",
                table: "VatTuHangHoa",
                column: "TaiKhoanGiaVonId");

            migrationBuilder.CreateIndex(
                name: "IX_VatTuHangHoa_TaiKhoanKhoId",
                table: "VatTuHangHoa",
                column: "TaiKhoanKhoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietHoaDonBan");

            migrationBuilder.DropTable(
                name: "ChiTietHoaDonMua");

            migrationBuilder.DropTable(
                name: "DoiTruCongNo");

            migrationBuilder.DropTable(
                name: "VatTuHangHoa");

            migrationBuilder.DropTable(
                name: "HoaDonBanHang");

            migrationBuilder.DropTable(
                name: "HoaDonMuaHang");
        }
    }
}
