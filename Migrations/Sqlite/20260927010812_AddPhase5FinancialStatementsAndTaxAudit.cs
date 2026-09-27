using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninjaTax.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddPhase5FinancialStatementsAndTaxAudit : Migration
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
                        onDelete: ReferentialAction.Restrict);
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
                        onDelete: ReferentialAction.Restrict);
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
                name: "IX_BaoCaoTaiChinhNam_NamTaiChinh",
                table: "BaoCaoTaiChinhNam",
                column: "NamTaiChinh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiPhiKhongHopLyB4_QuyetToanThueTndnId",
                table: "ChiPhiKhongHopLyB4",
                column: "QuyetToanThueTndnId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietChiTieuBctc_BaoCaoTaiChinhNamId_LoaiBaoCao_MaChiTieu",
                table: "ChiTietChiTieuBctc",
                columns: new[] { "BaoCaoTaiChinhNamId", "LoaiBaoCao", "MaChiTieu" });

            migrationBuilder.CreateIndex(
                name: "IX_QuyetToanThueTncn_NamQuyetToan",
                table: "QuyetToanThueTncn",
                column: "NamQuyetToan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuyetToanThueTndn_NamQuyetToan",
                table: "QuyetToanThueTndn",
                column: "NamQuyetToan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxRiskFinding_TaxAuditRiskShieldReportId",
                table: "TaxRiskFinding",
                column: "TaxAuditRiskShieldReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BangKeQttTncn051");

            migrationBuilder.DropTable(
                name: "BangKeQttTncn052");

            migrationBuilder.DropTable(
                name: "ChiPhiKhongHopLyB4");

            migrationBuilder.DropTable(
                name: "ChiTietChiTieuBctc");

            migrationBuilder.DropTable(
                name: "TaxRiskFinding");

            migrationBuilder.DropTable(
                name: "QuyetToanThueTncn");

            migrationBuilder.DropTable(
                name: "QuyetToanThueTndn");

            migrationBuilder.DropTable(
                name: "BaoCaoTaiChinhNam");

            migrationBuilder.DropTable(
                name: "TaxAuditRiskShieldReport");
        }
    }
}
