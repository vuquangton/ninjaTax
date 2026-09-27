using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninjaTax.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddPhase8InventoryAndWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kho",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChiNhanhId = table.Column<long>(type: "INTEGER", nullable: false),
                    MaKho = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenKho = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DiaChi = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ThuKhoId = table.Column<long>(type: "INTEGER", nullable: true),
                    TaiKhoanKhoMacDinhId = table.Column<long>(type: "INTEGER", nullable: true),
                    DangHoatDong = table.Column<bool>(type: "INTEGER", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kho", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kho_ChiNhanh_ChiNhanhId",
                        column: x => x.ChiNhanhId,
                        principalTable: "ChiNhanh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Kho_NhanVien_ThuKhoId",
                        column: x => x.ThuKhoId,
                        principalTable: "NhanVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Kho_TaiKhoan_TaiKhoanKhoMacDinhId",
                        column: x => x.TaiKhoanKhoMacDinhId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PhieuNhapKho",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChiNhanhId = table.Column<long>(type: "INTEGER", nullable: false),
                    KhoId = table.Column<long>(type: "INTEGER", nullable: false),
                    SoPhieu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NgayNhap = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayHachToan = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LoaiNhapKho = table.Column<int>(type: "INTEGER", nullable: false),
                    HoaDonMuaHangId = table.Column<long>(type: "INTEGER", nullable: true),
                    NhaCungCapId = table.Column<long>(type: "INTEGER", nullable: true),
                    DienGiai = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TongSoLuong = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongTienHang = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    ButToanId = table.Column<long>(type: "INTEGER", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuNhapKho", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhieuNhapKho_ButToan_ButToanId",
                        column: x => x.ButToanId,
                        principalTable: "ButToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PhieuNhapKho_ChiNhanh_ChiNhanhId",
                        column: x => x.ChiNhanhId,
                        principalTable: "ChiNhanh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhieuNhapKho_DoiTuong_NhaCungCapId",
                        column: x => x.NhaCungCapId,
                        principalTable: "DoiTuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PhieuNhapKho_HoaDonMuaHang_HoaDonMuaHangId",
                        column: x => x.HoaDonMuaHangId,
                        principalTable: "HoaDonMuaHang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PhieuNhapKho_Kho_KhoId",
                        column: x => x.KhoId,
                        principalTable: "Kho",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhieuXuatKho",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChiNhanhId = table.Column<long>(type: "INTEGER", nullable: false),
                    KhoId = table.Column<long>(type: "INTEGER", nullable: false),
                    SoPhieu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NgayXuat = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayHachToan = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LoaiXuatKho = table.Column<int>(type: "INTEGER", nullable: false),
                    HoaDonBanHangId = table.Column<long>(type: "INTEGER", nullable: true),
                    KhachHangId = table.Column<long>(type: "INTEGER", nullable: true),
                    PhongBanId = table.Column<long>(type: "INTEGER", nullable: true),
                    DienGiai = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TongSoLuong = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongTienGiaVon = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    ButToanId = table.Column<long>(type: "INTEGER", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuXuatKho", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhieuXuatKho_ButToan_ButToanId",
                        column: x => x.ButToanId,
                        principalTable: "ButToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PhieuXuatKho_ChiNhanh_ChiNhanhId",
                        column: x => x.ChiNhanhId,
                        principalTable: "ChiNhanh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhieuXuatKho_DoiTuong_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "DoiTuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PhieuXuatKho_HoaDonBanHang_HoaDonBanHangId",
                        column: x => x.HoaDonBanHangId,
                        principalTable: "HoaDonBanHang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PhieuXuatKho_Kho_KhoId",
                        column: x => x.KhoId,
                        principalTable: "Kho",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhieuXuatKho_PhongBan_PhongBanId",
                        column: x => x.PhongBanId,
                        principalTable: "PhongBan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietNhapKho",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhieuNhapKhoId = table.Column<long>(type: "INTEGER", nullable: false),
                    VatTuHangHoaId = table.Column<long>(type: "INTEGER", nullable: false),
                    SoLuong = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DonGia = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThanhTien = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TaiKhoanNoId = table.Column<long>(type: "INTEGER", nullable: false),
                    TaiKhoanCoId = table.Column<long>(type: "INTEGER", nullable: false),
                    SoLo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HanSuDung = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GhiChu = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietNhapKho", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietNhapKho_PhieuNhapKho_PhieuNhapKhoId",
                        column: x => x.PhieuNhapKhoId,
                        principalTable: "PhieuNhapKho",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietNhapKho_TaiKhoan_TaiKhoanCoId",
                        column: x => x.TaiKhoanCoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietNhapKho_TaiKhoan_TaiKhoanNoId",
                        column: x => x.TaiKhoanNoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietNhapKho_VatTuHangHoa_VatTuHangHoaId",
                        column: x => x.VatTuHangHoaId,
                        principalTable: "VatTuHangHoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietXuatKho",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhieuXuatKhoId = table.Column<long>(type: "INTEGER", nullable: false),
                    VatTuHangHoaId = table.Column<long>(type: "INTEGER", nullable: false),
                    SoLuong = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DonGiaVon = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TienGiaVon = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TaiKhoanNoId = table.Column<long>(type: "INTEGER", nullable: false),
                    TaiKhoanCoId = table.Column<long>(type: "INTEGER", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietXuatKho", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietXuatKho_PhieuXuatKho_PhieuXuatKhoId",
                        column: x => x.PhieuXuatKhoId,
                        principalTable: "PhieuXuatKho",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietXuatKho_TaiKhoan_TaiKhoanCoId",
                        column: x => x.TaiKhoanCoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietXuatKho_TaiKhoan_TaiKhoanNoId",
                        column: x => x.TaiKhoanNoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietXuatKho_VatTuHangHoa_VatTuHangHoaId",
                        column: x => x.VatTuHangHoaId,
                        principalTable: "VatTuHangHoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietNhapKho_PhieuNhapKhoId",
                table: "ChiTietNhapKho",
                column: "PhieuNhapKhoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietNhapKho_TaiKhoanCoId",
                table: "ChiTietNhapKho",
                column: "TaiKhoanCoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietNhapKho_TaiKhoanNoId",
                table: "ChiTietNhapKho",
                column: "TaiKhoanNoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietNhapKho_VatTuHangHoaId",
                table: "ChiTietNhapKho",
                column: "VatTuHangHoaId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXuatKho_PhieuXuatKhoId",
                table: "ChiTietXuatKho",
                column: "PhieuXuatKhoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXuatKho_TaiKhoanCoId",
                table: "ChiTietXuatKho",
                column: "TaiKhoanCoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXuatKho_TaiKhoanNoId",
                table: "ChiTietXuatKho",
                column: "TaiKhoanNoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietXuatKho_VatTuHangHoaId",
                table: "ChiTietXuatKho",
                column: "VatTuHangHoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Kho_ChiNhanhId_MaKho",
                table: "Kho",
                columns: new[] { "ChiNhanhId", "MaKho" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kho_TaiKhoanKhoMacDinhId",
                table: "Kho",
                column: "TaiKhoanKhoMacDinhId");

            migrationBuilder.CreateIndex(
                name: "IX_Kho_ThuKhoId",
                table: "Kho",
                column: "ThuKhoId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuNhapKho_ButToanId",
                table: "PhieuNhapKho",
                column: "ButToanId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuNhapKho_ChiNhanhId_SoPhieu",
                table: "PhieuNhapKho",
                columns: new[] { "ChiNhanhId", "SoPhieu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhieuNhapKho_HoaDonMuaHangId",
                table: "PhieuNhapKho",
                column: "HoaDonMuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuNhapKho_KhoId",
                table: "PhieuNhapKho",
                column: "KhoId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuNhapKho_NhaCungCapId",
                table: "PhieuNhapKho",
                column: "NhaCungCapId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuatKho_ButToanId",
                table: "PhieuXuatKho",
                column: "ButToanId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuatKho_ChiNhanhId_SoPhieu",
                table: "PhieuXuatKho",
                columns: new[] { "ChiNhanhId", "SoPhieu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuatKho_HoaDonBanHangId",
                table: "PhieuXuatKho",
                column: "HoaDonBanHangId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuatKho_KhachHangId",
                table: "PhieuXuatKho",
                column: "KhachHangId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuatKho_KhoId",
                table: "PhieuXuatKho",
                column: "KhoId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuatKho_PhongBanId",
                table: "PhieuXuatKho",
                column: "PhongBanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietNhapKho");

            migrationBuilder.DropTable(
                name: "ChiTietXuatKho");

            migrationBuilder.DropTable(
                name: "PhieuNhapKho");

            migrationBuilder.DropTable(
                name: "PhieuXuatKho");

            migrationBuilder.DropTable(
                name: "Kho");
        }
    }
}
