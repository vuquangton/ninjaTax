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
                name: "IX_ThongTinDoanhNghiep_MaSoThue",
                table: "ThongTinDoanhNghiep",
                column: "MaSoThue",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauHinhHoaDonDienTu");

            migrationBuilder.DropTable(
                name: "ChiNhanh");

            migrationBuilder.DropTable(
                name: "CauHinhKeToan");

            migrationBuilder.DropTable(
                name: "ThongTinDoanhNghiep");
        }
    }
}
