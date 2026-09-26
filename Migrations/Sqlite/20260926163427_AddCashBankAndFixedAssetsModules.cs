using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninjaTax.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddCashBankAndFixedAssetsModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaiKhoanNganHang",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoTaiKhoan = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenNganHang = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ChiNhanh = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ChuTaiKhoan = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SoDuBanDau = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DangHoatDong = table.Column<bool>(type: "INTEGER", nullable: false),
                    TaiKhoanKeToanId = table.Column<long>(type: "INTEGER", nullable: true),
                    GhiChu = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoanNganHang", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiKhoanNganHang_TaiKhoan_TaiKhoanKeToanId",
                        column: x => x.TaiKhoanKeToanId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiSanCoDinh",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaTaiSan = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenTaiSan = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    LoaiTaiSan = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayGhiTang = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayBatDauKhauHao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NguyenGia = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ThoiGianSuDungThang = table.Column<int>(type: "INTEGER", nullable: false),
                    GiaTriDaKhauHao = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    GiaTriConLai = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    MucKhauHaoThang = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TaiKhoanNguyenGiaId = table.Column<long>(type: "INTEGER", nullable: false),
                    TaiKhoanKhauHaoId = table.Column<long>(type: "INTEGER", nullable: true),
                    TaiKhoanChiPhiId = table.Column<long>(type: "INTEGER", nullable: false),
                    BoPhanSuDung = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiSanCoDinh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiSanCoDinh_TaiKhoan_TaiKhoanChiPhiId",
                        column: x => x.TaiKhoanChiPhiId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiSanCoDinh_TaiKhoan_TaiKhoanKhauHaoId",
                        column: x => x.TaiKhoanKhauHaoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaiSanCoDinh_TaiKhoan_TaiKhoanNguyenGiaId",
                        column: x => x.TaiKhoanNguyenGiaId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChungTuThuChi",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoChungTu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LoaiChungTu = table.Column<int>(type: "INTEGER", nullable: false),
                    NgayChungTu = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayHachToan = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SoChungTuGoc = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DoiTuongId = table.Column<long>(type: "INTEGER", nullable: true),
                    NguoiGiaoNopNhan = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DiaChi = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LyDo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TaiKhoanNganHangId = table.Column<long>(type: "INTEGER", nullable: true),
                    TongTien = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ViPhamQuyTac20Tr = table.Column<bool>(type: "INTEGER", nullable: false),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    ButToanId = table.Column<long>(type: "INTEGER", nullable: true),
                    HoaDonBanHangId = table.Column<long>(type: "INTEGER", nullable: true),
                    HoaDonMuaHangId = table.Column<long>(type: "INTEGER", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChungTuThuChi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChungTuThuChi_ButToan_ButToanId",
                        column: x => x.ButToanId,
                        principalTable: "ButToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChungTuThuChi_DoiTuong_DoiTuongId",
                        column: x => x.DoiTuongId,
                        principalTable: "DoiTuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChungTuThuChi_HoaDonBanHang_HoaDonBanHangId",
                        column: x => x.HoaDonBanHangId,
                        principalTable: "HoaDonBanHang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChungTuThuChi_HoaDonMuaHang_HoaDonMuaHangId",
                        column: x => x.HoaDonMuaHangId,
                        principalTable: "HoaDonMuaHang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChungTuThuChi_TaiKhoanNganHang_TaiKhoanNganHangId",
                        column: x => x.TaiKhoanNganHangId,
                        principalTable: "TaiKhoanNganHang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BangTinhKhauHao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaiSanCoDinhId = table.Column<long>(type: "INTEGER", nullable: false),
                    KyKeToan = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TuNgay = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DenNgay = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NguyenGia = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    SoTienKhauHao = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    LuyKeKhauHao = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    GiaTriConLai = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    ButToanId = table.Column<long>(type: "INTEGER", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangTinhKhauHao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BangTinhKhauHao_ButToan_ButToanId",
                        column: x => x.ButToanId,
                        principalTable: "ButToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BangTinhKhauHao_TaiSanCoDinh_TaiSanCoDinhId",
                        column: x => x.TaiSanCoDinhId,
                        principalTable: "TaiSanCoDinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietChungTuThuChi",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChungTuThuChiId = table.Column<long>(type: "INTEGER", nullable: false),
                    DienGiai = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TaiKhoanNoId = table.Column<long>(type: "INTEGER", nullable: false),
                    TaiKhoanCoId = table.Column<long>(type: "INTEGER", nullable: false),
                    SoTien = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DoiTuongId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietChungTuThuChi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietChungTuThuChi_ChungTuThuChi_ChungTuThuChiId",
                        column: x => x.ChungTuThuChiId,
                        principalTable: "ChungTuThuChi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietChungTuThuChi_DoiTuong_DoiTuongId",
                        column: x => x.DoiTuongId,
                        principalTable: "DoiTuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietChungTuThuChi_TaiKhoan_TaiKhoanCoId",
                        column: x => x.TaiKhoanCoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietChungTuThuChi_TaiKhoan_TaiKhoanNoId",
                        column: x => x.TaiKhoanNoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BangTinhKhauHao_ButToanId",
                table: "BangTinhKhauHao",
                column: "ButToanId");

            migrationBuilder.CreateIndex(
                name: "IX_BangTinhKhauHao_TaiSanCoDinhId_KyKeToan",
                table: "BangTinhKhauHao",
                columns: new[] { "TaiSanCoDinhId", "KyKeToan" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietChungTuThuChi_ChungTuThuChiId",
                table: "ChiTietChungTuThuChi",
                column: "ChungTuThuChiId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietChungTuThuChi_DoiTuongId",
                table: "ChiTietChungTuThuChi",
                column: "DoiTuongId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietChungTuThuChi_TaiKhoanCoId",
                table: "ChiTietChungTuThuChi",
                column: "TaiKhoanCoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietChungTuThuChi_TaiKhoanNoId",
                table: "ChiTietChungTuThuChi",
                column: "TaiKhoanNoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChungTuThuChi_ButToanId",
                table: "ChungTuThuChi",
                column: "ButToanId");

            migrationBuilder.CreateIndex(
                name: "IX_ChungTuThuChi_DoiTuongId",
                table: "ChungTuThuChi",
                column: "DoiTuongId");

            migrationBuilder.CreateIndex(
                name: "IX_ChungTuThuChi_HoaDonBanHangId",
                table: "ChungTuThuChi",
                column: "HoaDonBanHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ChungTuThuChi_HoaDonMuaHangId",
                table: "ChungTuThuChi",
                column: "HoaDonMuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ChungTuThuChi_SoChungTu",
                table: "ChungTuThuChi",
                column: "SoChungTu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChungTuThuChi_TaiKhoanNganHangId",
                table: "ChungTuThuChi",
                column: "TaiKhoanNganHangId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoanNganHang_SoTaiKhoan",
                table: "TaiKhoanNganHang",
                column: "SoTaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoanNganHang_TaiKhoanKeToanId",
                table: "TaiKhoanNganHang",
                column: "TaiKhoanKeToanId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiSanCoDinh_MaTaiSan",
                table: "TaiSanCoDinh",
                column: "MaTaiSan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiSanCoDinh_TaiKhoanChiPhiId",
                table: "TaiSanCoDinh",
                column: "TaiKhoanChiPhiId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiSanCoDinh_TaiKhoanKhauHaoId",
                table: "TaiSanCoDinh",
                column: "TaiKhoanKhauHaoId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiSanCoDinh_TaiKhoanNguyenGiaId",
                table: "TaiSanCoDinh",
                column: "TaiKhoanNguyenGiaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BangTinhKhauHao");

            migrationBuilder.DropTable(
                name: "ChiTietChungTuThuChi");

            migrationBuilder.DropTable(
                name: "TaiSanCoDinh");

            migrationBuilder.DropTable(
                name: "ChungTuThuChi");

            migrationBuilder.DropTable(
                name: "TaiKhoanNganHang");
        }
    }
}
