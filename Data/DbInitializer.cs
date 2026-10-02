using Microsoft.EntityFrameworkCore;
using ninjaTax.Models.Entities;

namespace ninjaTax.Data;

/// <summary>
/// Khởi tạo và nạp dữ liệu ban đầu cho hệ thống kế toán:
/// - Danh mục Hệ thống tài khoản kế toán chuẩn Thông tư TT99 (loại trừ hoàn toàn TK 911).
/// - Đối tượng mẫu (Khách hàng, Nhà cung cấp).
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        if (context.Database.IsSqlite())
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = '20260927051037_AddDepartmentAndCostCenter' AND NOT EXISTS (SELECT 1 FROM \"sqlite_master\" WHERE \"type\"='table' AND \"name\"='PhongBan');");
            }
            catch
            {
                // Bỏ qua nếu bảng quản lý migration chưa tồn tại
            }

            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();

            // Tự động đồng bộ các bảng mới (Phase 13, 14, 15) nếu CSDL đã tồn tại trước đó
            await EnsureSchemaSyncedAsync(context);
        }

        await SeedDataAsync(context);
    }

    private static async Task EnsureSchemaSyncedAsync(AppDbContext context)
    {
        var tables = new[]
        {
            @"CREATE TABLE IF NOT EXISTS `BangGiaBans` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `MaBangGia` longtext NOT NULL,
                `TenBangGia` longtext NOT NULL,
                `EffectiveFrom` datetime(6) NOT NULL,
                `EffectiveTo` datetime(6) NULL,
                `IsActive` tinyint(1) NOT NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `ExchangeRateHistories` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `FromCurrency` longtext NOT NULL,
                `ToCurrency` longtext NOT NULL,
                `Rate` decimal(19,4) NOT NULL,
                `EffectiveFrom` datetime(6) NOT NULL,
                `EffectiveTo` datetime(6) NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `ChungTuChiPhiMuaHang` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `ChiNhanhId` bigint NOT NULL,
                `SoChungTu` varchar(50) NOT NULL,
                `NgayChungTu` datetime(6) NOT NULL,
                `NgayHachToan` datetime(6) NOT NULL,
                `NhaCungCapDichVuId` bigint NULL,
                `DienGiai` varchar(500) NOT NULL,
                `TongChiPhi` decimal(19,4) NOT NULL,
                `ThueSuatVat` decimal(19,4) NOT NULL,
                `TienThueVat` decimal(19,4) NOT NULL,
                `TongThanhToan` decimal(19,4) NOT NULL,
                `PhuongThucPhanBo` int NOT NULL,
                `DaPhanBo` tinyint(1) NOT NULL,
                `ButToanId` bigint NULL,
                `NgayTao` datetime(6) NOT NULL,
                `NgayCapNhat` datetime(6) NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `ChiPhiMuaHangPhanBo` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `ChungTuChiPhiMuaHangId` bigint NOT NULL,
                `ChiTietNhapKhoId` bigint NOT NULL,
                `SoTienPhanBo` decimal(19,4) NOT NULL,
                `GhiChu` varchar(255) NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `DonViTinhQuyDoi` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `VatTuHangHoaId` bigint NOT NULL,
                `TenDonViTinh` longtext NOT NULL,
                `TyLeQuyDoi` decimal(19,4) NOT NULL,
                `PhepTinh` int NOT NULL,
                `DonGiaBanQuyDoi` decimal(19,4) NOT NULL,
                `LaDonViBanMacDinh` tinyint(1) NOT NULL,
                `LaDonViMuaMacDinh` tinyint(1) NOT NULL,
                `DangHoatDong` tinyint(1) NOT NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `PhieuDieuChuyenKho` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `ChiNhanhId` bigint NOT NULL,
                `SoPhieu` longtext NOT NULL,
                `NgayDieuChuyen` datetime(6) NOT NULL,
                `NgayHachToan` datetime(6) NOT NULL,
                `KhoXuatId` bigint NOT NULL,
                `KhoNhapId` bigint NOT NULL,
                `NguoiVanChuyen` longtext NULL,
                `PhuongTienVanChuyen` longtext NULL,
                `LenhDieuDongSo` longtext NULL,
                `LyDoDieuChuyen` longtext NULL,
                `TongSoLuong` decimal(19,4) NOT NULL,
                `TongGiaTri` decimal(19,4) NOT NULL,
                `TrangThai` int NOT NULL,
                `ButToanId` bigint NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `ChiTietDieuChuyenKho` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `PhieuDieuChuyenKhoId` bigint NOT NULL,
                `VatTuHangHoaId` bigint NOT NULL,
                `DonViTinh` longtext NOT NULL,
                `SoLuong` decimal(19,4) NOT NULL,
                `DonGiaVon` decimal(19,4) NOT NULL,
                `ThanhTien` decimal(19,4) NOT NULL,
                `TaiKhoanXuatId` bigint NULL,
                `TaiKhoanNhapId` bigint NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `ChungTuDieuChinhThuongMai` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `ChiNhanhId` bigint NOT NULL,
                `LoaiDieuChinh` int NOT NULL,
                `SoChungTu` longtext NOT NULL,
                `NgayChungTu` datetime(6) NOT NULL,
                `NgayHachToan` datetime(6) NOT NULL,
                `DoiTuongId` bigint NOT NULL,
                `HoaDonBanHangGocId` bigint NULL,
                `HoaDonMuaHangGocId` bigint NULL,
                `KhoId` bigint NULL,
                `LyDo` longtext NULL,
                `HinhThucXuLy` int NOT NULL,
                `TrangThai` int NOT NULL,
                `TongTienHang` decimal(19,4) NOT NULL,
                `TongTienThueVat` decimal(19,4) NOT NULL,
                `TongThanhToan` decimal(19,4) NOT NULL,
                `TongGiaTriNhapLaiKho` decimal(19,4) NOT NULL,
                `ButToanDoanhThuCongNoId` bigint NULL,
                `ButToanGiaVonKhoId` bigint NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `ChiTietDieuChinhThuongMai` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `ChungTuDieuChinhThuongMaiId` bigint NOT NULL,
                `DongSo` int NOT NULL,
                `VatTuHangHoaId` bigint NULL,
                `DonViTinh` longtext NOT NULL,
                `SoLuong` decimal(19,4) NOT NULL,
                `DonGia` decimal(19,4) NOT NULL,
                `ThanhTien` decimal(19,4) NOT NULL,
                `ThueSuatVat` decimal(19,4) NOT NULL,
                `TienThueVat` decimal(19,4) NOT NULL,
                `DonGiaVonNhapLai` decimal(19,4) NOT NULL,
                `TienGiaVonNhapLai` decimal(19,4) NOT NULL,
                `TaiKhoanNoId` bigint NULL,
                `TaiKhoanCoId` bigint NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `BangTrichLapDuPhongNoPhaiThu` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `ChiNhanhId` bigint NOT NULL,
                `SoChungTu` longtext NOT NULL,
                `NgayLap` datetime(6) NOT NULL,
                `NgayHachToan` datetime(6) NOT NULL,
                `TongNoQuaHan` decimal(19,4) NOT NULL,
                `TongSoDuPhongPhaiTrich` decimal(19,4) NOT NULL,
                `SoDuDuPhongHienTai2293` decimal(19,4) NOT NULL,
                `SoTienTrichThem` decimal(19,4) NOT NULL,
                `SoTienHoanNhap` decimal(19,4) NOT NULL,
                `TrangThai` int NOT NULL,
                `ButToanId` bigint NULL,
                `GhiChu` longtext NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `ChiTietTrichLapDuPhong` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `BangTrichLapDuPhongNoPhaiThuId` bigint NOT NULL,
                `KhachHangId` bigint NOT NULL,
                `HoaDonBanHangId` bigint NULL,
                `SoHoaDon` longtext NOT NULL,
                `NgayHoaDon` datetime(6) NOT NULL,
                `HanThanhToan` datetime(6) NOT NULL,
                `SoTienConNo` decimal(19,4) NOT NULL,
                `SoNgayQuaHan` int NOT NULL,
                `TyLeTrichLap` decimal(19,4) NOT NULL,
                `SoTienDuPhong` decimal(19,4) NOT NULL,
                `LyDoDacBiet` longtext NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `DanhGiaLaiNgoaiTe` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `ChiNhanhId` bigint NOT NULL,
                `SoChungTu` longtext NOT NULL,
                `NgayChungTu` datetime(6) NOT NULL,
                `NgayHachToan` datetime(6) NOT NULL,
                `LoaiTien` longtext NOT NULL,
                `TyGiaMua` decimal(19,4) NOT NULL,
                `TyGiaBan` decimal(19,4) NOT NULL,
                `TongLaiTyGia` decimal(19,4) NOT NULL,
                `TongLoTyGia` decimal(19,4) NOT NULL,
                `ButToanDanhGiaLaiId` bigint NULL,
                `ButToanKetChuyen413Id` bigint NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `ChiTietDanhGiaLaiNgoaiTe` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `DanhGiaLaiNgoaiTeId` bigint NOT NULL,
                `TaiKhoanId` bigint NOT NULL,
                `DoiTuongId` bigint NULL,
                `SoDuNgoaiTe` decimal(19,4) NOT NULL,
                `TyGiaGhiSo` decimal(19,4) NOT NULL,
                `GiaTriGhiSoVnd` decimal(19,4) NOT NULL,
                `TyGiaDanhGiaLai` decimal(19,4) NOT NULL,
                `GiaTriDanhGiaLaiVnd` decimal(19,4) NOT NULL,
                `ChenhLechVnd` decimal(19,4) NOT NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",

            @"CREATE TABLE IF NOT EXISTS `ChiTietBangGias` (
                `Id` bigint NOT NULL AUTO_INCREMENT,
                `BangGiaBanId` bigint NOT NULL,
                `VatTuHangHoaId` bigint NOT NULL,
                `DonGia` decimal(19,4) NOT NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"
        };

        foreach (var sql in tables)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EnsureSchemaSyncedAsync Table Error]: {ex.Message}");
            }
        }

        var columns = new[]
        {
            "ALTER TABLE `ChungTuChiPhiMuaHang` ADD COLUMN `DaPhanBo` tinyint(1) NOT NULL DEFAULT 0;",
            "ALTER TABLE `ChungTuChiPhiMuaHang` ADD COLUMN `TongChiPhi` decimal(19,4) NOT NULL DEFAULT 0.0000;",
            "ALTER TABLE `ChungTuChiPhiMuaHang` ADD COLUMN `ThueSuatVat` decimal(19,4) NOT NULL DEFAULT 10.0000;",
            "ALTER TABLE `ChungTuChiPhiMuaHang` ADD COLUMN `TienThueVat` decimal(19,4) NOT NULL DEFAULT 0.0000;",
            "ALTER TABLE `ChiPhiMuaHangPhanBo` ADD COLUMN `SoTienPhanBo` decimal(19,4) NOT NULL DEFAULT 0.0000;",
            "ALTER TABLE `PhieuNhapKho` ADD COLUMN `ChiPhiMuaHang` decimal(19,4) NOT NULL DEFAULT 0.0000;",
            "ALTER TABLE `ChiTietNhapKho` ADD COLUMN `ChiPhiMuaHangPhanBo` decimal(19,4) NOT NULL DEFAULT 0.0000;",
            "ALTER TABLE `PhieuXuatKho` ADD COLUMN `LaDieuChinhGiaVonCuoiKy` tinyint(1) NOT NULL DEFAULT 0;",
            "ALTER TABLE `ChiTietXuatKho` ADD COLUMN `ChenhLechGiaVon` decimal(19,4) NULL;",
            "ALTER TABLE `ChiTietXuatKho` ADD COLUMN `DonGiaVonCuoiKy` decimal(19,4) NULL;"
        };

        foreach (var sql in columns)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(sql);
            }
            catch
            {
                // Bỏ qua nếu cột đã tồn tại
            }
        }
    }

    public static void Initialize(AppDbContext context)
    {
        SeedDataAsync(context).GetAwaiter().GetResult();
    }

    public static async Task SeedDataAsync(AppDbContext context)
    {
        if (!await context.TaiKhoans.AnyAsync())
        {
            // Danh mục tài khoản chuẩn TT99/2025/TT-BTC
            var taiKhoans = new List<TaiKhoan>
        {
            // Nhóm 1: Tài sản ngắn hạn
            new() { MaTaiKhoan = "111", TenTaiKhoan = "Tiền mặt", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "1111", TenTaiKhoan = "Tiền Việt Nam", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "1112", TenTaiKhoan = "Ngoại tệ", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "112", TenTaiKhoan = "Tiền gửi ngân hàng", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "1121", TenTaiKhoan = "Tiền Việt Nam gửi ngân hàng", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "1122", TenTaiKhoan = "Ngoại tệ gửi ngân hàng", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "131", TenTaiKhoan = "Phải thu của khách hàng", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "133", TenTaiKhoan = "Thuế GTGT được khấu trừ", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "1331", TenTaiKhoan = "Thuế GTGT được khấu trừ của hàng hóa, dịch vụ", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "141", TenTaiKhoan = "Tạm ứng", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "152", TenTaiKhoan = "Nguyên liệu, vật liệu", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "156", TenTaiKhoan = "Hàng hóa", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "1561", TenTaiKhoan = "Giá mua hàng hóa", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "154", TenTaiKhoan = "Chi phí sản xuất, kinh doanh dở dang", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },

            // Nhóm 2: Tài sản dài hạn & Dự phòng tổn thất tài sản
            new() { MaTaiKhoan = "211", TenTaiKhoan = "Tài sản cố định hữu hình", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "214", TenTaiKhoan = "Hao mòn tài sản cố định", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "2141", TenTaiKhoan = "Hao mòn tài sản cố định hữu hình", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "229", TenTaiKhoan = "Dự phòng tổn thất tài sản", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "2293", TenTaiKhoan = "Dự phòng phải thu khó đòi", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "242", TenTaiKhoan = "Chi phí trả trước", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },

            // Nhóm 3: Nợ phải trả
            new() { MaTaiKhoan = "331", TenTaiKhoan = "Phải trả cho người bán", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "333", TenTaiKhoan = "Thuế và các khoản phải nộp Nhà nước", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "3331", TenTaiKhoan = "Thuế giá trị gia tăng phải nộp", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "33311", TenTaiKhoan = "Thuế giá trị gia tăng đầu ra", BacTaiKhoan = 3, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "3334", TenTaiKhoan = "Thuế thu nhập doanh nghiệp", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "3335", TenTaiKhoan = "Thuế thu nhập cá nhân", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "334", TenTaiKhoan = "Phải trả người lao động", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "338", TenTaiKhoan = "Phải trả, phải nộp khác", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "3382", TenTaiKhoan = "Kinh phí công đoàn", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "3383", TenTaiKhoan = "Bảo hiểm xã hội", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "3384", TenTaiKhoan = "Bảo hiểm y tế", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "3386", TenTaiKhoan = "Bảo hiểm thất nghiệp", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "341", TenTaiKhoan = "Vay và nợ thuê tài chính", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },

            // Nhóm 4: Vốn chủ sở hữu & Chênh lệch tỷ giá (TK 413)
            new() { MaTaiKhoan = "411", TenTaiKhoan = "Vốn đầu tư của chủ sở hữu", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.VonChuSoHuu, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "4111", TenTaiKhoan = "Vốn góp của chủ sở hữu", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.VonChuSoHuu, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "413", TenTaiKhoan = "Chênh lệch tỷ giá hối đoái", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.VonChuSoHuu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "4131", TenTaiKhoan = "Chênh lệch tỷ giá đánh giá lại cuối kỳ", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.VonChuSoHuu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "421", TenTaiKhoan = "Lợi nhuận sau thuế chưa phân phối", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.VonChuSoHuu, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "4211", TenTaiKhoan = "Lợi nhuận sau thuế chưa phân phối năm trước", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.VonChuSoHuu, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "4212", TenTaiKhoan = "Lợi nhuận sau thuế chưa phân phối năm nay", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.VonChuSoHuu, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = false },

            // Nhóm 5: Doanh thu
            new() { MaTaiKhoan = "511", TenTaiKhoan = "Doanh thu bán hàng và cung cấp dịch vụ", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "5111", TenTaiKhoan = "Doanh thu bán hàng hóa", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "5112", TenTaiKhoan = "Doanh thu bán các thành phẩm", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "5113", TenTaiKhoan = "Doanh thu cung cấp dịch vụ", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "515", TenTaiKhoan = "Doanh thu hoạt động tài chính", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "521", TenTaiKhoan = "Các khoản giảm trừ doanh thu", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "5211", TenTaiKhoan = "Chiết khấu thương mại", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "5212", TenTaiKhoan = "Hàng bán bị trả lại", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },

            // Nhóm 6: Chi phí sản xuất, kinh doanh
            new() { MaTaiKhoan = "632", TenTaiKhoan = "Giá vốn hàng bán", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "635", TenTaiKhoan = "Chi phí tài chính", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "641", TenTaiKhoan = "Chi phí bán hàng", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "642", TenTaiKhoan = "Chi phí quản lý doanh nghiệp", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "6421", TenTaiKhoan = "Chi phí bán hàng", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "6422", TenTaiKhoan = "Chi phí quản lý doanh nghiệp", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "6426", TenTaiKhoan = "Chi phí dự phòng nợ phải thu khó đòi", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },

            // Nhóm 7 & 8: Thu nhập khác và Chi phí khác
            new() { MaTaiKhoan = "711", TenTaiKhoan = "Thu nhập khác", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ThuNhapKhac, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "811", TenTaiKhoan = "Chi phí khác", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhiKhac, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "821", TenTaiKhoan = "Chi phí thuế thu nhập doanh nghiệp", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "8211", TenTaiKhoan = "Chi phí thuế TNDN hiện hành", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },

            // Nhóm 9: Xác định kết quả kinh doanh (Chuẩn Thông tư 99/2025/TT-BTC)
            new() { MaTaiKhoan = "911", TenTaiKhoan = "Xác định kết quả kinh doanh", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.XacDinhKetQuaKinhDoanh, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false }
        };

        await context.TaiKhoans.AddRangeAsync(taiKhoans);
        await context.SaveChangesAsync();

        // Nạp đối tượng mẫu
        var doiTuongs = new List<DoiTuong>
        {
            new()
            {
                MaDoiTuong = "KH001",
                TenDoiTuong = "Công ty TNHH Giải Pháp Công Nghệ Ánh Dương",
                Loai = LoaiDoiTuong.KhachHang,
                MaSoThue = "0109988776",
                DiaChi = "Số 123 Phố Huế, Hai Bà Trưng, Hà Nội",
                SoDienThoai = "02439887766",
                Email = "contact@anhduongtech.vn"
            },
            new()
            {
                MaDoiTuong = "NCC001",
                TenDoiTuong = "Công ty Cổ Phần Thiết Bị Văn Phòng Hòa Phát",
                Loai = LoaiDoiTuong.NhaCungCap,
                MaSoThue = "0301122334",
                DiaChi = "Tòa nhà PaxSky, Quận 3, TP. Hồ Chí Minh",
                SoDienThoai = "02838112233",
                Email = "kinhdoanh@hoaphat-office.vn"
            }
        };

        await context.DoiTuongs.AddRangeAsync(doiTuongs);
        await context.SaveChangesAsync();
        }
        else
        {
            // Bổ sung các tài khoản TT99 nếu database đã được khởi tạo trước đó
            var existingCodes = await context.TaiKhoans.Select(t => t.MaTaiKhoan).ToListAsync();
            var missingAccounts = new List<TaiKhoan>();

            if (!existingCodes.Contains("821"))
            {
                missingAccounts.Add(new TaiKhoan { MaTaiKhoan = "821", TenTaiKhoan = "Chi phí thuế thu nhập doanh nghiệp", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = true });
            }
            if (!existingCodes.Contains("8211"))
            {
                missingAccounts.Add(new TaiKhoan { MaTaiKhoan = "8211", TenTaiKhoan = "Chi phí thuế TNDN hiện hành", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false });
            }
            if (!existingCodes.Contains("911"))
            {
                missingAccounts.Add(new TaiKhoan { MaTaiKhoan = "911", TenTaiKhoan = "Xác định kết quả kinh doanh", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.XacDinhKetQuaKinhDoanh, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false });
            }

            if (missingAccounts.Count > 0)
            {
                await context.TaiKhoans.AddRangeAsync(missingAccounts);
                await context.SaveChangesAsync();
            }
        }

        // Nạp danh mục Vật tư hàng hóa mẫu nếu chưa có
        if (!await context.VatTuHangHoas.AnyAsync())
        {
            var tk156 = await context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "156");
            var tk152 = await context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "152");
            var tk511 = await context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "511");
            var tk632 = await context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "632");

            var items = new List<VatTuHangHoa>
            {
                new()
                {
                    MaVatTu = "HH001",
                    TenVatTu = "Máy chủ Dell PowerEdge R750xs",
                    DonViTinh = "Bộ",
                    LoaiVatTu = LoaiVatTuHangHoa.HangHoa,
                    TaiKhoanKhoId = tk156?.Id,
                    TaiKhoanDoanhThuId = tk511?.Id,
                    TaiKhoanGiaVonId = tk632?.Id,
                    ThueSuatVatMacDinh = 10m,
                    DonGiaMuaGanNhat = 65000000m,
                    DonGiaBanTieuChuan = 78000000m,
                    DangTheoDoiTonKho = true,
                    DangHoatDong = true
                },
                new()
                {
                    MaVatTu = "HH002",
                    TenVatTu = "Thiết bị định tuyến Router Cisco Catalyst C9200",
                    DonViTinh = "Chiếc",
                    LoaiVatTu = LoaiVatTuHangHoa.HangHoa,
                    TaiKhoanKhoId = tk156?.Id,
                    TaiKhoanDoanhThuId = tk511?.Id,
                    TaiKhoanGiaVonId = tk632?.Id,
                    ThueSuatVatMacDinh = 10m,
                    DonGiaMuaGanNhat = 21000000m,
                    DonGiaBanTieuChuan = 26500000m,
                    DangTheoDoiTonKho = true,
                    DangHoatDong = true
                },
                new()
                {
                    MaVatTu = "DV001",
                    TenVatTu = "Dịch vụ triển khai bảo trì hạ tầng mạng (Gói 12 tháng)",
                    DonViTinh = "Gói",
                    LoaiVatTu = LoaiVatTuHangHoa.DichVu,
                    TaiKhoanKhoId = null,
                    TaiKhoanDoanhThuId = tk511?.Id,
                    TaiKhoanGiaVonId = tk632?.Id,
                    ThueSuatVatMacDinh = 8m,
                    DonGiaMuaGanNhat = 0m,
                    DonGiaBanTieuChuan = 15000000m,
                    DangTheoDoiTonKho = false,
                    DangHoatDong = true
                }
            };

            await context.VatTuHangHoas.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }

        // Khởi tạo thông tin Doanh nghiệp & Chi nhánh Trụ sở chính ban đầu (Phase 6)
        if (!await context.ThongTinDoanhNghieps.AnyAsync())
        {
            var company = new ThongTinDoanhNghiep
            {
                MaDoanhNghiep = "DN01",
                TenDoanhNghiep = "CÔNG TY CỔ PHẦN CÔNG NGHỆ NINJATAX VIỆT NAM",
                TenGiaoDich = "NINJATAX JSC",
                TenTiengAnh = "NINJATAX VIETNAM TECHNOLOGY JOINT STOCK COMPANY",
                MaSoThue = "0109998883",
                DiaChiTruSo = "Tầng 10, Tòa nhà Keangnam Landmark 72, Đường Phạm Hùng, Q. Nam Từ Liêm, TP. Hà Nội",
                TinhThanhPho = "TP. Hà Nội",
                QuanHuyen = "Quận Nam Từ Liêm",
                MaCoQuanThueQuanLy = "101",
                TenCoQuanThueQuanLy = "Cục Thuế Thành phố Hà Nội",
                NguoiDaiDienPhapLuat = "Nguyễn Văn Doanh",
                ChucDanhNguoiDaiDien = "Tổng Giám Đốc",
                GiamDoc = "Nguyễn Văn Doanh",
                KeToanTruong = "Trần Thị Kế Toán",
                NguoiLapBieu = "Lê Văn Lập Biểu",
                ThuQuy = "Phạm Thị Thủ Quỹ",
                SoDienThoai = "024.3999.8888",
                Email = "ketoan@ninjatax.vn",
                VonDieuLe = 20_000_000_000m,
                NgayThanhLap = new DateTime(2020, 1, 1)
            };

            var hoBranch = new ChiNhanh
            {
                MaChiNhanh = "HO-01",
                TenChiNhanh = "Trụ sở chính Hà Nội",
                MaSoThueChiNhanh = company.MaSoThue,
                LoaiChiNhanh = LoaiChiNhanh.TruSoChinh,
                DiaChi = company.DiaChiTruSo,
                TinhThanhPho = company.TinhThanhPho,
                DangHoatDong = true
            };
            company.ChiNhanhs.Add(hoBranch);

            var accountingConfig = new CauHinhKeToan
            {
                CheDoKeToan = CheDoKeToanDoanhNghiep.TT99_2025,
                DonViTienTe = "VND",
                NgayBatDauNienDo = 1,
                ThangBatDauNienDo = 1,
                PhuongPhapThueGtgt = PhuongPhapTinhThueGtgt.KhauTru,
                PhuongPhapXuatKho = PhuongPhapGiaXuatKho.BinhQuanCuoiKy,
                PhuongPhapKhauHaoTscd = PhuongPhapKhauHao.DuongThang,
                CanhBaoChiVuotQuy = true,
                CanhBaoXuatAmKho = true,
                CanhBaoHoaDonTren20TrTienMat = true
            };
            company.CauHinhKeToan = accountingConfig;

            await context.ThongTinDoanhNghieps.AddAsync(company);
            await context.SaveChangesAsync();
        }

        // Khởi tạo Cơ cấu Phòng Ban / Khoa chuyên môn mặc định (Phase 7)
        if (!await context.PhongBans.AnyAsync())
        {
            var hoBranch = await context.ChiNhanhs.FirstOrDefaultAsync(b => b.LoaiChiNhanh == LoaiChiNhanh.TruSoChinh)
                           ?? await context.ChiNhanhs.FirstOrDefaultAsync();

            if (hoBranch != null)
            {
                var bod = new PhongBan
                {
                    ChiNhanhId = hoBranch.Id,
                    MaPhongBan = "BOD",
                    TenPhongBan = "Ban Giám Đốc",
                    LoaiPhongBan = LoaiPhongBan.QuanLy,
                    MaTaiKhoanChiPhi = "6422",
                    LaTrungTamLoiNhuan = false,
                    DangHoatDong = true,
                    GhiChu = "Cơ quan điều hành cao nhất toàn công ty"
                };
                await context.PhongBans.AddAsync(bod);
                await context.SaveChangesAsync();

                var departments = new List<PhongBan>
                {
                    new()
                    {
                        ChiNhanhId = hoBranch.Id,
                        PhongBanChaId = bod.Id,
                        MaPhongBan = "PB-KTTC",
                        TenPhongBan = "Phòng Kế Toán & Tài Chính",
                        LoaiPhongBan = LoaiPhongBan.QuanLy,
                        MaTaiKhoanChiPhi = "6422",
                        DangHoatDong = true
                    },
                    new()
                    {
                        ChiNhanhId = hoBranch.Id,
                        PhongBanChaId = bod.Id,
                        MaPhongBan = "PB-KD",
                        TenPhongBan = "Phòng Kinh Doanh & Tiếp Thị",
                        LoaiPhongBan = LoaiPhongBan.BanHang,
                        MaTaiKhoanChiPhi = "6421",
                        LaTrungTamLoiNhuan = true,
                        DangHoatDong = true
                    },
                    new()
                    {
                        ChiNhanhId = hoBranch.Id,
                        PhongBanChaId = bod.Id,
                        MaPhongBan = "PX-SX",
                        TenPhongBan = "Phân Xưởng Sản Xuất & Kỹ Thuật",
                        LoaiPhongBan = LoaiPhongBan.SanXuat,
                        MaTaiKhoanChiPhi = "154",
                        DangHoatDong = true
                    },
                    new()
                    {
                        ChiNhanhId = hoBranch.Id,
                        PhongBanChaId = bod.Id,
                        MaPhongBan = "KHOA-CNTT",
                        TenPhongBan = "Khoa Công Nghệ Thông Tin & Đào Tạo",
                        LoaiPhongBan = LoaiPhongBan.KhoaChuyenMon,
                        MaTaiKhoanChiPhi = "154",
                        LaTrungTamLoiNhuan = true,
                        DangHoatDong = true
                    }
                };

                await context.PhongBans.AddRangeAsync(departments);
                await context.SaveChangesAsync();

                // Cập nhật gán phòng ban cho nhân viên mẫu nếu có
                var nhanViens = await context.NhanViens.ToListAsync();
                if (nhanViens.Any())
                {
                    var pbKt = departments.First(d => d.MaPhongBan == "PB-KTTC");
                    var pbKd = departments.First(d => d.MaPhongBan == "PB-KD");

                    for (int i = 0; i < nhanViens.Count; i++)
                    {
                        var nv = nhanViens[i];
                        if (i % 2 == 0)
                        {
                            nv.PhongBanId = pbKt.Id;
                            nv.PhongBan = pbKt.TenPhongBan;
                        }
                        else
                        {
                            nv.PhongBanId = pbKd.Id;
                            nv.PhongBan = pbKd.TenPhongBan;
                        }
                    }
                    await context.SaveChangesAsync();
                }
            }
        }

        // Khởi tạo Danh mục Kho hàng mặc định (Phase 8 - VAS 02 / TT99)
        if (!await context.Khos.AnyAsync())
        {
            var hoBranch = await context.ChiNhanhs.FirstOrDefaultAsync(b => b.LoaiChiNhanh == LoaiChiNhanh.TruSoChinh)
                           ?? await context.ChiNhanhs.FirstOrDefaultAsync();

            if (hoBranch != null)
            {
                var tk1561 = await context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "1561")
                             ?? await context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "156");
                var tk152 = await context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == "152");

                var khos = new List<Kho>
                {
                    new()
                    {
                        ChiNhanhId = hoBranch.Id,
                        MaKho = "KHO-TONG",
                        TenKho = "Kho Tổng Hàng Hóa & Thiết Bị",
                        DiaChi = "Tầng 1, Tòa nhà Trụ sở chính",
                        TaiKhoanKhoMacDinhId = tk1561?.Id,
                        DangHoatDong = true,
                        GhiChu = "Kho trung tâm lưu trữ hàng hóa kinh doanh"
                    },
                    new()
                    {
                        ChiNhanhId = hoBranch.Id,
                        MaKho = "KHO-NVL",
                        TenKho = "Kho Nguyên Vật Liệu & Phụ Kiện",
                        DiaChi = "Khu sản xuất, Phân xưởng kỹ thuật",
                        TaiKhoanKhoMacDinhId = tk152?.Id,
                        DangHoatDong = true,
                        GhiChu = "Kho nguyên vật liệu phục vụ sản xuất gia công"
                    }
                };

                await context.Khos.AddRangeAsync(khos);
                await context.SaveChangesAsync();
            }
        }
    }
}
