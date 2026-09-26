using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninjaTax.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ButToan",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoChungTu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NgayHachToan = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayChungTu = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SoChungTuGoc = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NgayChungTuGoc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DienGiai = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TongTien = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongNo = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TongCo = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    TrangThai = table.Column<int>(type: "INTEGER", nullable: false),
                    NguoiTao = table.Column<string>(type: "TEXT", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ButToan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DoiTuong",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaDoiTuong = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenDoiTuong = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Loai = table.Column<int>(type: "INTEGER", nullable: false),
                    MaSoThue = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    DiaChi = table.Column<string>(type: "TEXT", nullable: true),
                    SoDienThoai = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NguoiLienHe = table.Column<string>(type: "TEXT", nullable: true),
                    SoTaiKhoanNganHang = table.Column<string>(type: "TEXT", nullable: true),
                    TenNganHang = table.Column<string>(type: "TEXT", nullable: true),
                    DangHoatDong = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoiTuong", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaTaiKhoan = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TenTaiKhoan = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    BacTaiKhoan = table.Column<int>(type: "INTEGER", nullable: false),
                    TaiKhoanMeId = table.Column<long>(type: "INTEGER", nullable: true),
                    LoaiTaiKhoan = table.Column<int>(type: "INTEGER", nullable: false),
                    TinhChat = table.Column<int>(type: "INTEGER", nullable: false),
                    LaTaiKhoanSoCai = table.Column<bool>(type: "INTEGER", nullable: false),
                    DangHoatDong = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiKhoan_TaiKhoan_TaiKhoanMeId",
                        column: x => x.TaiKhoanMeId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietButToan",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ButToanId = table.Column<long>(type: "INTEGER", nullable: false),
                    DongSo = table.Column<int>(type: "INTEGER", nullable: false),
                    TaiKhoanNoId = table.Column<long>(type: "INTEGER", nullable: false),
                    TaiKhoanCoId = table.Column<long>(type: "INTEGER", nullable: false),
                    SoTien = table.Column<decimal>(type: "TEXT", precision: 19, scale: 4, nullable: false),
                    DienGiai = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DoiTuongId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietButToan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietButToan_ButToan_ButToanId",
                        column: x => x.ButToanId,
                        principalTable: "ButToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietButToan_DoiTuong_DoiTuongId",
                        column: x => x.DoiTuongId,
                        principalTable: "DoiTuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietButToan_TaiKhoan_TaiKhoanCoId",
                        column: x => x.TaiKhoanCoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietButToan_TaiKhoan_TaiKhoanNoId",
                        column: x => x.TaiKhoanNoId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ButToan_SoChungTu",
                table: "ButToan",
                column: "SoChungTu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietButToan_ButToanId",
                table: "ChiTietButToan",
                column: "ButToanId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietButToan_DoiTuongId",
                table: "ChiTietButToan",
                column: "DoiTuongId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietButToan_TaiKhoanCoId",
                table: "ChiTietButToan",
                column: "TaiKhoanCoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietButToan_TaiKhoanNoId",
                table: "ChiTietButToan",
                column: "TaiKhoanNoId");

            migrationBuilder.CreateIndex(
                name: "IX_DoiTuong_MaDoiTuong",
                table: "DoiTuong",
                column: "MaDoiTuong",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_MaTaiKhoan",
                table: "TaiKhoan",
                column: "MaTaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_TaiKhoanMeId",
                table: "TaiKhoan",
                column: "TaiKhoanMeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietButToan");

            migrationBuilder.DropTable(
                name: "ButToan");

            migrationBuilder.DropTable(
                name: "DoiTuong");

            migrationBuilder.DropTable(
                name: "TaiKhoan");
        }
    }
}
