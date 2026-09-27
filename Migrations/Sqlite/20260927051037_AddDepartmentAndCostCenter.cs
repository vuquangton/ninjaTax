using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninjaTax.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddDepartmentAndCostCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhongBan",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChiNhanhId = table.Column<long>(type: "INTEGER", nullable: false),
                    PhongBanChaId = table.Column<long>(type: "INTEGER", nullable: true),
                    MaPhongBan = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenPhongBan = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TenTiengAnh = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    LoaiPhongBan = table.Column<int>(type: "INTEGER", nullable: false),
                    MaTaiKhoanChiPhi = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TruongPhongId = table.Column<long>(type: "INTEGER", nullable: true),
                    LaTrungTamLoiNhuan = table.Column<bool>(type: "INTEGER", nullable: false),
                    DangHoatDong = table.Column<bool>(type: "INTEGER", nullable: false),
                    GhiChu = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhongBan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhongBan_ChiNhanh_ChiNhanhId",
                        column: x => x.ChiNhanhId,
                        principalTable: "ChiNhanh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhongBan_PhongBan_PhongBanChaId",
                        column: x => x.PhongBanChaId,
                        principalTable: "PhongBan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhongBan_NhanVien_TruongPhongId",
                        column: x => x.TruongPhongId,
                        principalTable: "NhanVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.AddColumn<long>(
                name: "PhongBanId",
                table: "NhanVien",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PhongBanId",
                table: "ChiTietButToan",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhongBan_ChiNhanhId_MaPhongBan",
                table: "PhongBan",
                columns: new[] { "ChiNhanhId", "MaPhongBan" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhongBan_PhongBanChaId",
                table: "PhongBan",
                column: "PhongBanChaId");

            migrationBuilder.CreateIndex(
                name: "IX_PhongBan_TruongPhongId",
                table: "PhongBan",
                column: "TruongPhongId");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_PhongBanId",
                table: "NhanVien",
                column: "PhongBanId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietButToan_PhongBanId",
                table: "ChiTietButToan",
                column: "PhongBanId");

            migrationBuilder.AddForeignKey(
                name: "FK_NhanVien_PhongBan_PhongBanId",
                table: "NhanVien",
                column: "PhongBanId",
                principalTable: "PhongBan",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietButToan_PhongBan_PhongBanId",
                table: "ChiTietButToan",
                column: "PhongBanId",
                principalTable: "PhongBan",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietButToan_PhongBan_PhongBanId",
                table: "ChiTietButToan");

            migrationBuilder.DropForeignKey(
                name: "FK_NhanVien_PhongBan_PhongBanId",
                table: "NhanVien");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietButToan_PhongBanId",
                table: "ChiTietButToan");

            migrationBuilder.DropIndex(
                name: "IX_NhanVien_PhongBanId",
                table: "NhanVien");

            migrationBuilder.DropColumn(
                name: "PhongBanId",
                table: "ChiTietButToan");

            migrationBuilder.DropColumn(
                name: "PhongBanId",
                table: "NhanVien");

            migrationBuilder.DropTable(
                name: "PhongBan");
        }
    }
}
