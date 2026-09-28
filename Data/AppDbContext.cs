using Microsoft.EntityFrameworkCore;
using ninjaTax.Models.Entities;

namespace ninjaTax.Data;

/// <summary>
/// DbContext trung tâm của ninjaTax hỗ trợ kiến trúc Multi-Database:
/// - SQLite (mặc định môi trường dev)
/// - SQL Server / SQL Express
/// - PostgreSQL
/// - MariaDB / MySQL
/// Tuân thủ quy tắc kế toán Thông tư TT99:
/// - Tất cả khóa chính và khóa ngoại đều là long (bigint).
/// - Chuẩn hóa Tài khoản 911 theo Thông tư 99/2025/TT-BTC làm tài khoản trung gian kết chuyển cuối kỳ (số dư cuối kỳ = 0).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<DoiTuong> DoiTuongs => Set<DoiTuong>();
    public DbSet<ButToan> ButToans => Set<ButToan>();
    public DbSet<ChiTietButToan> ChiTietButToans => Set<ChiTietButToan>();
    public DbSet<BangGiaBan> BangGiaBans => Set<BangGiaBan>();
    public DbSet<ChiTietBangGia> ChiTietBangGias => Set<ChiTietBangGia>();
    public DbSet<ExchangeRateHistory> ExchangeRateHistories => Set<ExchangeRateHistory>();
    public DbSet<VatTuHangHoa> VatTuHangHoas => Set<VatTuHangHoa>();
    public DbSet<HoaDonMuaHang> HoaDonMuaHangs => Set<HoaDonMuaHang>();
    public DbSet<ChiTietHoaDonMua> ChiTietHoaDonMuas => Set<ChiTietHoaDonMua>();
    public DbSet<HoaDonBanHang> HoaDonBanHangs => Set<HoaDonBanHang>();
    public DbSet<ChiTietHoaDonBan> ChiTietHoaDonBans => Set<ChiTietHoaDonBan>();
    public DbSet<DoiTruCongNo> DoiTruCongNos => Set<DoiTruCongNo>();
    public DbSet<TaiKhoanNganHang> TaiKhoanNganHangs => Set<TaiKhoanNganHang>();
    public DbSet<ChungTuThuChi> ChungTuThuChis => Set<ChungTuThuChi>();
    public DbSet<ChiTietChungTuThuChi> ChiTietChungTuThuChis => Set<ChiTietChungTuThuChi>();
    public DbSet<TaiSanCoDinh> TaiSanCoDinhs => Set<TaiSanCoDinh>();
    public DbSet<BangTinhKhauHao> BangTinhKhauHaos => Set<BangTinhKhauHao>();
    public DbSet<NhanVien> NhanViens => Set<NhanVien>();
    public DbSet<BangChamCongThang> BangChamCongThangs => Set<BangChamCongThang>();
    public DbSet<ChiTietChamCong> ChiTietChamCongs => Set<ChiTietChamCong>();
    public DbSet<BangLuongThang> BangLuongThangs => Set<BangLuongThang>();
    public DbSet<ChiTietLuongNhanVien> ChiTietLuongNhanViens => Set<ChiTietLuongNhanVien>();

    // Phase 5: BCTC TT99, Quyết toán thuế & Lá chắn rủi ro thuế
    public DbSet<BaoCaoTaiChinhNam> BaoCaoTaiChinhNams => Set<BaoCaoTaiChinhNam>();
    public DbSet<ChiTietChiTieuBctc> ChiTietChiTieuBctcs => Set<ChiTietChiTieuBctc>();
    public DbSet<QuyetToanThueTndn> QuyetToanThueTndns => Set<QuyetToanThueTndn>();
    public DbSet<ChiPhiKhongHopLyB4> ChiPhiKhongHopLyB4s => Set<ChiPhiKhongHopLyB4>();
    public DbSet<QuyetToanThueTncn> QuyetToanThueTncns => Set<QuyetToanThueTncn>();
    public DbSet<BangKeQttTncn051> BangKeQttTncn051s => Set<BangKeQttTncn051>();
    public DbSet<BangKeQttTncn052> BangKeQttTncn052s => Set<BangKeQttTncn052>();
    public DbSet<TaxAuditRiskShieldReport> TaxAuditRiskShieldReports => Set<TaxAuditRiskShieldReport>();
    public DbSet<TaxRiskFinding> TaxRiskFindings => Set<TaxRiskFinding>();

    // Phase 6: Thông tin Doanh nghiệp, Đa chi nhánh & Cấu hình Kế toán
    public DbSet<ThongTinDoanhNghiep> ThongTinDoanhNghieps => Set<ThongTinDoanhNghiep>();
    public DbSet<ChiNhanh> ChiNhanhs => Set<ChiNhanh>();
    public DbSet<CauHinhKeToan> CauHinhKeToans => Set<CauHinhKeToan>();
    public DbSet<CauHinhHoaDonDienTu> CauHinhHoaDonDienTus => Set<CauHinhHoaDonDienTu>();

    // Phase 7: Phòng Ban & Trung tâm Chi phí
    public DbSet<PhongBan> PhongBans => Set<PhongBan>();

    // Phase 8: Quản lý Kho & Hàng Tồn Kho (VAS 02 / TT99)
    public DbSet<Kho> Khos => Set<Kho>();
    public DbSet<PhieuNhapKho> PhieuNhapKhos => Set<PhieuNhapKho>();
    public DbSet<ChiTietNhapKho> ChiTietNhapKhos => Set<ChiTietNhapKho>();
    public DbSet<PhieuXuatKho> PhieuXuatKhos => Set<PhieuXuatKho>();
    public DbSet<ChiTietXuatKho> ChiTietXuatKhos => Set<ChiTietXuatKho>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Quy ước toàn cục cho tất cả kiểu decimal trong hệ thống: 19 chữ số, 4 chữ số thập phân
        configurationBuilder.Properties<decimal>().HavePrecision(19, 4);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================================
        // 1. Cấu hình Danh mục Tài khoản (TaiKhoan - TT99)
        // ========================================================
        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.ToTable("TaiKhoan");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.MaTaiKhoan)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(e => e.MaTaiKhoan)
                .IsUnique();

            entity.Property(e => e.TenTaiKhoan)
                .IsRequired()
                .HasMaxLength(255);

            // Quan hệ phân cấp cây tài khoản mẹ - con
            entity.HasOne(e => e.TaiKhoanMe)
                .WithMany(m => m.TaiKhoanCons)
                .HasForeignKey(e => e.TaiKhoanMeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================================
        // 2. Cấu hình Danh mục Đối tượng (DoiTuong)
        // ========================================================
        modelBuilder.Entity<DoiTuong>(entity =>
        {
            entity.ToTable("DoiTuong");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.MaDoiTuong)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.MaDoiTuong)
                .IsUnique();

            entity.Property(e => e.TenDoiTuong)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.MaSoThue)
                .HasMaxLength(20);

            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(30);

            entity.Property(e => e.Email)
                .HasMaxLength(100);
        });

        // ========================================================
        // 3. Cấu hình Chứng từ Bút toán (ButToan - Sổ Nhật ký chung)
        // ========================================================
        modelBuilder.Entity<ButToan>(entity =>
        {
            entity.ToTable("ButToan");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.SoChungTu)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.SoChungTu)
                .IsUnique();

            entity.Property(e => e.SoChungTuGoc)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.DienGiai)
                .IsRequired()
                .HasMaxLength(500);

            // Chỉ định tường minh độ chính xác tiền tệ 19, 4 bảo vệ tính toán Multi-DB
            entity.Property(e => e.TongTien).HasPrecision(19, 4);
            entity.Property(e => e.TongNo).HasPrecision(19, 4);
            entity.Property(e => e.TongCo).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.TongTien).HasColumnType("TEXT");
                entity.Property(e => e.TongNo).HasColumnType("TEXT");
                entity.Property(e => e.TongCo).HasColumnType("TEXT");
            }

            entity.HasMany(e => e.ChiTietButToans)
                .WithOne(d => d.ButToan)
                .HasForeignKey(d => d.ButToanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================================
        // 4. Cấu hình Chi tiết Bút toán (ChiTietButToan - Định khoản kép)
        // ========================================================
        modelBuilder.Entity<ChiTietButToan>(entity =>
        {
            entity.ToTable("ChiTietButToan");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.SoTien).HasPrecision(19, 4);
            if (Database.IsSqlite())
            {
                entity.Property(e => e.SoTien).HasColumnType("TEXT");
            }

            entity.Property(e => e.DienGiai)
                .HasMaxLength(500);

            // Ràng buộc khóa ngoại tài khoản Nợ
            entity.HasOne(e => e.TaiKhoanNo)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanNoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ràng buộc khóa ngoại tài khoản Có
            entity.HasOne(e => e.TaiKhoanCo)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanCoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ràng buộc khóa ngoại đối tượng công nợ
            entity.HasOne(e => e.DoiTuong)
                .WithMany()
                .HasForeignKey(e => e.DoiTuongId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ràng buộc khóa ngoại phòng ban / cost center (Phase 7)
            entity.HasOne(e => e.PhongBan)
                .WithMany(p => p.ChiTietButToans)
                .HasForeignKey(e => e.PhongBanId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ========================================================
        // 5. Cấu hình Danh mục Vật tư hàng hóa (VatTuHangHoa)
        // ========================================================
        modelBuilder.Entity<VatTuHangHoa>(entity =>
        {
            entity.ToTable("VatTuHangHoa");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.MaVatTu).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.MaVatTu).IsUnique();

            entity.Property(e => e.TenVatTu).IsRequired().HasMaxLength(255);
            entity.Property(e => e.DonViTinh).IsRequired().HasMaxLength(50);

            entity.Property(e => e.ThueSuatVatMacDinh).HasPrecision(19, 4);
            entity.Property(e => e.DonGiaMuaGanNhat).HasPrecision(19, 4);
            entity.Property(e => e.DonGiaBanTieuChuan).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.ThueSuatVatMacDinh).HasColumnType("TEXT");
                entity.Property(e => e.DonGiaMuaGanNhat).HasColumnType("TEXT");
                entity.Property(e => e.DonGiaBanTieuChuan).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.TaiKhoanKho)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanKhoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanDoanhThu)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanDoanhThuId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanGiaVon)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanGiaVonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================================
        // 6. Cấu hình Hóa đơn mua hàng (HoaDonMuaHang)
        // ========================================================
        modelBuilder.Entity<HoaDonMuaHang>(entity =>
        {
            entity.ToTable("HoaDonMuaHang");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.SoChungTu).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.SoChungTu).IsUnique();

            entity.Property(e => e.KHMauSoHoaDon).HasMaxLength(20);
            entity.Property(e => e.KyHieuHoaDon).HasMaxLength(20);
            entity.Property(e => e.SoHoaDon).HasMaxLength(50);
            entity.Property(e => e.MaTraCuuHdt).HasMaxLength(100);

            entity.Property(e => e.TongTienHang).HasPrecision(19, 4);
            entity.Property(e => e.TongTienChietKhau).HasPrecision(19, 4);
            entity.Property(e => e.TongTienThueVat).HasPrecision(19, 4);
            entity.Property(e => e.TongThanhToan).HasPrecision(19, 4);
            entity.Property(e => e.DaThanhToan).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.TongTienHang).HasColumnType("TEXT");
                entity.Property(e => e.TongTienChietKhau).HasColumnType("TEXT");
                entity.Property(e => e.TongTienThueVat).HasColumnType("TEXT");
                entity.Property(e => e.TongThanhToan).HasColumnType("TEXT");
                entity.Property(e => e.DaThanhToan).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.NhaCungCap)
                .WithMany()
                .HasForeignKey(e => e.NhaCungCapId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ButToan)
                .WithMany()
                .HasForeignKey(e => e.ButToanId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.ChiTietHangs)
                .WithOne(d => d.HoaDonMuaHang)
                .HasForeignKey(d => d.HoaDonMuaHangId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================================
        // 7. Cấu hình Chi tiết Hóa đơn mua (ChiTietHoaDonMua)
        // ========================================================
        modelBuilder.Entity<ChiTietHoaDonMua>(entity =>
        {
            entity.ToTable("ChiTietHoaDonMua");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.SoLuong).HasPrecision(19, 4);
            entity.Property(e => e.DonGia).HasPrecision(19, 4);
            entity.Property(e => e.ThanhTien).HasPrecision(19, 4);
            entity.Property(e => e.TiLeChietKhau).HasPrecision(19, 4);
            entity.Property(e => e.TienChietKhau).HasPrecision(19, 4);
            entity.Property(e => e.ThueSuatVat).HasPrecision(19, 4);
            entity.Property(e => e.TienThueVat).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.SoLuong).HasColumnType("TEXT");
                entity.Property(e => e.DonGia).HasColumnType("TEXT");
                entity.Property(e => e.ThanhTien).HasColumnType("TEXT");
                entity.Property(e => e.TiLeChietKhau).HasColumnType("TEXT");
                entity.Property(e => e.TienChietKhau).HasColumnType("TEXT");
                entity.Property(e => e.ThueSuatVat).HasColumnType("TEXT");
                entity.Property(e => e.TienThueVat).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.VatTuHangHoa)
                .WithMany()
                .HasForeignKey(e => e.VatTuHangHoaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanNo)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanNoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanThue)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanThueId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanCo)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanCoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================================
        // 8. Cấu hình Hóa đơn bán hàng (HoaDonBanHang)
        // ========================================================
        modelBuilder.Entity<HoaDonBanHang>(entity =>
        {
            entity.ToTable("HoaDonBanHang");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.SoChungTu).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.SoChungTu).IsUnique();

            entity.Property(e => e.KHMauSo).HasMaxLength(20);
            entity.Property(e => e.KyHieu).HasMaxLength(20);
            entity.Property(e => e.SoHoaDon).HasMaxLength(50);
            entity.Property(e => e.MaCoQuanThue).HasMaxLength(100);

            entity.Property(e => e.TongTienHang).HasPrecision(19, 4);
            entity.Property(e => e.TongTienChietKhau).HasPrecision(19, 4);
            entity.Property(e => e.TongTienThueVat).HasPrecision(19, 4);
            entity.Property(e => e.TongThanhToan).HasPrecision(19, 4);
            entity.Property(e => e.DaThuTien).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.TongTienHang).HasColumnType("TEXT");
                entity.Property(e => e.TongTienChietKhau).HasColumnType("TEXT");
                entity.Property(e => e.TongTienThueVat).HasColumnType("TEXT");
                entity.Property(e => e.TongThanhToan).HasColumnType("TEXT");
                entity.Property(e => e.DaThuTien).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.KhachHang)
                .WithMany()
                .HasForeignKey(e => e.KhachHangId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ButToanDoanhThu)
                .WithMany()
                .HasForeignKey(e => e.ButToanDoanhThuId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ButToanGiaVon)
                .WithMany()
                .HasForeignKey(e => e.ButToanGiaVonId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.ChiTietBans)
                .WithOne(d => d.HoaDonBanHang)
                .HasForeignKey(d => d.HoaDonBanHangId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================================
        // 9. Cấu hình Chi tiết Hóa đơn bán (ChiTietHoaDonBan)
        // ========================================================
        modelBuilder.Entity<ChiTietHoaDonBan>(entity =>
        {
            entity.ToTable("ChiTietHoaDonBan");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.SoLuong).HasPrecision(19, 4);
            entity.Property(e => e.DonGia).HasPrecision(19, 4);
            entity.Property(e => e.ThanhTien).HasPrecision(19, 4);
            entity.Property(e => e.TiLeChietKhau).HasPrecision(19, 4);
            entity.Property(e => e.TienChietKhau).HasPrecision(19, 4);
            entity.Property(e => e.ThueSuatVat).HasPrecision(19, 4);
            entity.Property(e => e.TienThueVat).HasPrecision(19, 4);
            entity.Property(e => e.DonGiaVon).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.SoLuong).HasColumnType("TEXT");
                entity.Property(e => e.DonGia).HasColumnType("TEXT");
                entity.Property(e => e.ThanhTien).HasColumnType("TEXT");
                entity.Property(e => e.TiLeChietKhau).HasColumnType("TEXT");
                entity.Property(e => e.TienChietKhau).HasColumnType("TEXT");
                entity.Property(e => e.ThueSuatVat).HasColumnType("TEXT");
                entity.Property(e => e.TienThueVat).HasColumnType("TEXT");
                entity.Property(e => e.DonGiaVon).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.VatTuHangHoa)
                .WithMany()
                .HasForeignKey(e => e.VatTuHangHoaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanNo)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanNoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanDoanhThu)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanDoanhThuId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanThue)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanThueId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanGiaVon)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanGiaVonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanKho)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanKhoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================================
        // 10. Cấu hình Đối trừ công nợ (DoiTruCongNo)
        // ========================================================
        modelBuilder.Entity<DoiTruCongNo>(entity =>
        {
            entity.ToTable("DoiTruCongNo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.SoTienDoiTru).HasPrecision(19, 4);
            if (Database.IsSqlite())
            {
                entity.Property(e => e.SoTienDoiTru).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.DoiTuong)
                .WithMany()
                .HasForeignKey(e => e.DoiTuongId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.HoaDonBanHang)
                .WithMany()
                .HasForeignKey(e => e.HoaDonBanHangId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.HoaDonMuaHang)
                .WithMany()
                .HasForeignKey(e => e.HoaDonMuaHangId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ButToan)
                .WithMany()
                .HasForeignKey(e => e.ButToanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================================
        // 11. Cấu hình Tài khoản Ngân hàng (TaiKhoanNganHang)
        // ========================================================
        modelBuilder.Entity<TaiKhoanNganHang>(entity =>
        {
            entity.ToTable("TaiKhoanNganHang");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.SoTaiKhoan)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.SoTaiKhoan)
                .IsUnique();

            entity.Property(e => e.TenNganHang)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.SoDuBanDau).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.SoDuBanDau).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.TaiKhoanKeToan)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanKeToanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================================
        // 12. Cấu hình Chứng từ Thu - Chi (ChungTuThuChi)
        // ========================================================
        modelBuilder.Entity<ChungTuThuChi>(entity =>
        {
            entity.ToTable("ChungTuThuChi");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.SoChungTu)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.SoChungTu)
                .IsUnique();

            entity.Property(e => e.TongTien).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.TongTien).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.DoiTuong)
                .WithMany()
                .HasForeignKey(e => e.DoiTuongId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanNganHang)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanNganHangId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ButToan)
                .WithMany()
                .HasForeignKey(e => e.ButToanId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.HoaDonBanHang)
                .WithMany()
                .HasForeignKey(e => e.HoaDonBanHangId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.HoaDonMuaHang)
                .WithMany()
                .HasForeignKey(e => e.HoaDonMuaHangId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.ChiTietThuChis)
                .WithOne(d => d.ChungTuThuChi)
                .HasForeignKey(d => d.ChungTuThuChiId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================================
        // 13. Cấu hình Chi tiết Thu - Chi (ChiTietChungTuThuChi)
        // ========================================================
        modelBuilder.Entity<ChiTietChungTuThuChi>(entity =>
        {
            entity.ToTable("ChiTietChungTuThuChi");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.SoTien).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.SoTien).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.TaiKhoanNo)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanNoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanCo)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanCoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DoiTuong)
                .WithMany()
                .HasForeignKey(e => e.DoiTuongId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================================
        // 14. Cấu hình Tài sản cố định (TaiSanCoDinh)
        // ========================================================
        modelBuilder.Entity<TaiSanCoDinh>(entity =>
        {
            entity.ToTable("TaiSanCoDinh");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.MaTaiSan)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.MaTaiSan)
                .IsUnique();

            entity.Property(e => e.NguyenGia).HasPrecision(19, 4);
            entity.Property(e => e.GiaTriDaKhauHao).HasPrecision(19, 4);
            entity.Property(e => e.GiaTriConLai).HasPrecision(19, 4);
            entity.Property(e => e.MucKhauHaoThang).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.NguyenGia).HasColumnType("TEXT");
                entity.Property(e => e.GiaTriDaKhauHao).HasColumnType("TEXT");
                entity.Property(e => e.GiaTriConLai).HasColumnType("TEXT");
                entity.Property(e => e.MucKhauHaoThang).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.TaiKhoanNguyenGia)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanNguyenGiaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanKhauHao)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanKhauHaoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanChiPhi)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanChiPhiId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.BangTinhKhauHaos)
                .WithOne(d => d.TaiSanCoDinh)
                .HasForeignKey(d => d.TaiSanCoDinhId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================================
        // 15. Cấu hình Bảng tính khấu hao (BangTinhKhauHao)
        // ========================================================
        modelBuilder.Entity<BangTinhKhauHao>(entity =>
        {
            entity.ToTable("BangTinhKhauHao");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.KyKeToan)
                .IsRequired()
                .HasMaxLength(10);

            entity.HasIndex(e => new { e.TaiSanCoDinhId, e.KyKeToan })
                .IsUnique();

            entity.Property(e => e.NguyenGia).HasPrecision(19, 4);
            entity.Property(e => e.SoTienKhauHao).HasPrecision(19, 4);
            entity.Property(e => e.LuyKeKhauHao).HasPrecision(19, 4);
            entity.Property(e => e.GiaTriConLai).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.NguyenGia).HasColumnType("TEXT");
                entity.Property(e => e.SoTienKhauHao).HasColumnType("TEXT");
                entity.Property(e => e.LuyKeKhauHao).HasColumnType("TEXT");
                entity.Property(e => e.GiaTriConLai).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.ButToan)
                .WithMany()
                .HasForeignKey(e => e.ButToanId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ========================================================
        // 16. Cấu hình Nhân sự & Tiền lương (NhanVien)
        // ========================================================
        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.ToTable("NhanVien");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.MaNhanVien)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.MaNhanVien)
                .IsUnique();

            entity.Property(e => e.HoTen)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.LuongCoBan).HasPrecision(19, 4);
            entity.Property(e => e.LuongDongBaoHiem).HasPrecision(19, 4);
            entity.Property(e => e.PhuCapAnTrua).HasPrecision(19, 4);
            entity.Property(e => e.PhuCapTrachNhiem).HasPrecision(19, 4);
            entity.Property(e => e.PhuCapDienThoai).HasPrecision(19, 4);
            entity.Property(e => e.PhuCapTrangPhuc).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.LuongCoBan).HasColumnType("TEXT");
                entity.Property(e => e.LuongDongBaoHiem).HasColumnType("TEXT");
                entity.Property(e => e.PhuCapAnTrua).HasColumnType("TEXT");
                entity.Property(e => e.PhuCapTrachNhiem).HasColumnType("TEXT");
                entity.Property(e => e.PhuCapDienThoai).HasColumnType("TEXT");
                entity.Property(e => e.PhuCapTrangPhuc).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.PhongBanEntity)
                .WithMany(p => p.NhanViens)
                .HasForeignKey(e => e.PhongBanId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ========================================================
        // 17. Cấu hình Bảng Chấm Công (BangChamCongThang & ChiTietChamCong)
        // ========================================================
        modelBuilder.Entity<BangChamCongThang>(entity =>
        {
            entity.ToTable("BangChamCongThang");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.KyKeToan)
                .IsRequired()
                .HasMaxLength(10);

            entity.HasIndex(e => e.KyKeToan)
                .IsUnique();

            entity.HasMany(e => e.ChiTiets)
                .WithOne(d => d.BangChamCongThang)
                .HasForeignKey(d => d.BangChamCongThangId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChiTietChamCong>(entity =>
        {
            entity.ToTable("ChiTietChamCong");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasIndex(e => new { e.BangChamCongThangId, e.NhanVienId })
                .IsUnique();

            entity.Property(e => e.SoNgayDiLam).HasPrecision(19, 4);
            entity.Property(e => e.SoNgayNghiPhep).HasPrecision(19, 4);
            entity.Property(e => e.SoNgayNghiLe).HasPrecision(19, 4);
            entity.Property(e => e.SoNgayNghiKhongLuong).HasPrecision(19, 4);
            entity.Property(e => e.SoNgayNghiOmBhxh).HasPrecision(19, 4);
            entity.Property(e => e.SoNgayNghiThaiSan).HasPrecision(19, 4);
            entity.Property(e => e.GioLamThemNgayThuong).HasPrecision(19, 4);
            entity.Property(e => e.GioLamThemNgayNghi).HasPrecision(19, 4);
            entity.Property(e => e.GioLamThemNgayLe).HasPrecision(19, 4);
            entity.Property(e => e.TongCongTinhLuong).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.SoNgayDiLam).HasColumnType("TEXT");
                entity.Property(e => e.SoNgayNghiPhep).HasColumnType("TEXT");
                entity.Property(e => e.SoNgayNghiLe).HasColumnType("TEXT");
                entity.Property(e => e.SoNgayNghiKhongLuong).HasColumnType("TEXT");
                entity.Property(e => e.SoNgayNghiOmBhxh).HasColumnType("TEXT");
                entity.Property(e => e.SoNgayNghiThaiSan).HasColumnType("TEXT");
                entity.Property(e => e.GioLamThemNgayThuong).HasColumnType("TEXT");
                entity.Property(e => e.GioLamThemNgayNghi).HasColumnType("TEXT");
                entity.Property(e => e.GioLamThemNgayLe).HasColumnType("TEXT");
                entity.Property(e => e.TongCongTinhLuong).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.NhanVien)
                .WithMany()
                .HasForeignKey(e => e.NhanVienId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================================
        // 18. Cấu hình Bảng Lương (BangLuongThang & ChiTietLuongNhanVien)
        // ========================================================
        modelBuilder.Entity<BangLuongThang>(entity =>
        {
            entity.ToTable("BangLuongThang");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.SoChungTu)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.SoChungTu)
                .IsUnique();

            entity.Property(e => e.KyKeToan)
                .IsRequired()
                .HasMaxLength(10);

            entity.HasIndex(e => e.KyKeToan)
                .IsUnique();

            entity.Property(e => e.TongQuyLuong).HasPrecision(19, 4);
            entity.Property(e => e.TongBaoHiemDnGanh).HasPrecision(19, 4);
            entity.Property(e => e.TongBaoHiemNldGanh).HasPrecision(19, 4);
            entity.Property(e => e.TongThueTncn).HasPrecision(19, 4);
            entity.Property(e => e.TongThucLinh).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.TongQuyLuong).HasColumnType("TEXT");
                entity.Property(e => e.TongBaoHiemDnGanh).HasColumnType("TEXT");
                entity.Property(e => e.TongBaoHiemNldGanh).HasColumnType("TEXT");
                entity.Property(e => e.TongThueTncn).HasColumnType("TEXT");
                entity.Property(e => e.TongThucLinh).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.ButToanChiPhiLuong)
                .WithMany()
                .HasForeignKey(e => e.ButToanChiPhiLuongId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ButToanBaoHiemDn)
                .WithMany()
                .HasForeignKey(e => e.ButToanBaoHiemDnId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ButToanKhauTruLuong)
                .WithMany()
                .HasForeignKey(e => e.ButToanKhauTruLuongId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.ChiTiets)
                .WithOne(d => d.BangLuongThang)
                .HasForeignKey(d => d.BangLuongThangId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChiTietLuongNhanVien>(entity =>
        {
            entity.ToTable("ChiTietLuongNhanVien");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasIndex(e => new { e.BangLuongThangId, e.NhanVienId })
                .IsUnique();

            entity.Property(e => e.LuongThoiGian).HasPrecision(19, 4);
            entity.Property(e => e.LuongLamThemGio).HasPrecision(19, 4);
            entity.Property(e => e.LuongOtMienThue).HasPrecision(19, 4);
            entity.Property(e => e.PhuCapChiuThue).HasPrecision(19, 4);
            entity.Property(e => e.PhuCapMienThue).HasPrecision(19, 4);
            entity.Property(e => e.TienThuong).HasPrecision(19, 4);
            entity.Property(e => e.TongThuNhap).HasPrecision(19, 4);
            entity.Property(e => e.LuongDongBaoHiem).HasPrecision(19, 4);
            entity.Property(e => e.BhxhNld).HasPrecision(19, 4);
            entity.Property(e => e.BhytNld).HasPrecision(19, 4);
            entity.Property(e => e.BhtnNld).HasPrecision(19, 4);
            entity.Property(e => e.TongBaoHiemNld).HasPrecision(19, 4);
            entity.Property(e => e.BhxhDn).HasPrecision(19, 4);
            entity.Property(e => e.BhytDn).HasPrecision(19, 4);
            entity.Property(e => e.BhtnDn).HasPrecision(19, 4);
            entity.Property(e => e.KpcdDn).HasPrecision(19, 4);
            entity.Property(e => e.TongBaoHiemDn).HasPrecision(19, 4);
            entity.Property(e => e.GiamTruBanThan).HasPrecision(19, 4);
            entity.Property(e => e.GiamTruNguoiPhuThuoc).HasPrecision(19, 4);
            entity.Property(e => e.ThuNhapTinhThue).HasPrecision(19, 4);
            entity.Property(e => e.ThueTncnKhauTru).HasPrecision(19, 4);
            entity.Property(e => e.TamUng).HasPrecision(19, 4);
            entity.Property(e => e.ThucLinh).HasPrecision(19, 4);

            if (Database.IsSqlite())
            {
                entity.Property(e => e.LuongThoiGian).HasColumnType("TEXT");
                entity.Property(e => e.LuongLamThemGio).HasColumnType("TEXT");
                entity.Property(e => e.LuongOtMienThue).HasColumnType("TEXT");
                entity.Property(e => e.PhuCapChiuThue).HasColumnType("TEXT");
                entity.Property(e => e.PhuCapMienThue).HasColumnType("TEXT");
                entity.Property(e => e.TienThuong).HasColumnType("TEXT");
                entity.Property(e => e.TongThuNhap).HasColumnType("TEXT");
                entity.Property(e => e.LuongDongBaoHiem).HasColumnType("TEXT");
                entity.Property(e => e.BhxhNld).HasColumnType("TEXT");
                entity.Property(e => e.BhytNld).HasColumnType("TEXT");
                entity.Property(e => e.BhtnNld).HasColumnType("TEXT");
                entity.Property(e => e.TongBaoHiemNld).HasColumnType("TEXT");
                entity.Property(e => e.BhxhDn).HasColumnType("TEXT");
                entity.Property(e => e.BhytDn).HasColumnType("TEXT");
                entity.Property(e => e.BhtnDn).HasColumnType("TEXT");
                entity.Property(e => e.KpcdDn).HasColumnType("TEXT");
                entity.Property(e => e.TongBaoHiemDn).HasColumnType("TEXT");
                entity.Property(e => e.GiamTruBanThan).HasColumnType("TEXT");
                entity.Property(e => e.GiamTruNguoiPhuThuoc).HasColumnType("TEXT");
                entity.Property(e => e.ThuNhapTinhThue).HasColumnType("TEXT");
                entity.Property(e => e.ThueTncnKhauTru).HasColumnType("TEXT");
                entity.Property(e => e.TamUng).HasColumnType("TEXT");
                entity.Property(e => e.ThucLinh).HasColumnType("TEXT");
            }

            entity.HasOne(e => e.NhanVien)
                .WithMany()
                .HasForeignKey(e => e.NhanVienId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================================
        // 20. BCTC TT99, Quyết toán thuế & Tax Audit Risk Shield
        // ========================================================
        modelBuilder.Entity<BaoCaoTaiChinhNam>(entity => entity.ToTable("BaoCaoTaiChinhNam"));
        modelBuilder.Entity<ChiTietChiTieuBctc>(entity => entity.ToTable("ChiTietChiTieuBctc"));
        modelBuilder.Entity<QuyetToanThueTndn>(entity => entity.ToTable("QuyetToanThueTndn"));
        modelBuilder.Entity<ChiPhiKhongHopLyB4>(entity => entity.ToTable("ChiPhiKhongHopLyB4"));
        modelBuilder.Entity<QuyetToanThueTncn>(entity => entity.ToTable("QuyetToanThueTncn"));
        modelBuilder.Entity<BangKeQttTncn051>(entity => entity.ToTable("BangKeQttTncn051"));
        modelBuilder.Entity<BangKeQttTncn052>(entity => entity.ToTable("BangKeQttTncn052"));
        modelBuilder.Entity<TaxAuditRiskShieldReport>(entity => entity.ToTable("TaxAuditRiskShieldReport"));
        modelBuilder.Entity<TaxRiskFinding>(entity => entity.ToTable("TaxRiskFinding"));

        // ========================================================
        // 21. Thông tin Doanh nghiệp & Đa chi nhánh (Phase 6)
        // ========================================================
        modelBuilder.Entity<ThongTinDoanhNghiep>(entity =>
        {
            entity.ToTable("ThongTinDoanhNghiep");
            entity.HasIndex(e => e.MaSoThue).IsUnique();
        });

        modelBuilder.Entity<ChiNhanh>(entity =>
        {
            entity.ToTable("ChiNhanh");
            entity.HasIndex(e => new { e.DoanhNghiepId, e.MaChiNhanh }).IsUnique();
            entity.HasOne(e => e.DoanhNghiep)
                .WithMany(d => d.ChiNhanhs)
                .HasForeignKey(e => e.DoanhNghiepId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CauHinhKeToan>(entity =>
        {
            entity.ToTable("CauHinhKeToan");
            entity.HasOne(e => e.DoanhNghiep)
                .WithOne(d => d.CauHinhKeToan)
                .HasForeignKey<CauHinhKeToan>(e => e.DoanhNghiepId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CauHinhHoaDonDienTu>(entity =>
        {
            entity.ToTable("CauHinhHoaDonDienTu");
            entity.HasOne(e => e.ChiNhanh)
                .WithOne(c => c.CauHinhHddt)
                .HasForeignKey<CauHinhHoaDonDienTu>(e => e.ChiNhanhId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================================
        // 22. Cấu hình Phòng Ban & Trung tâm Chi phí (Phase 7)
        // ========================================================
        modelBuilder.Entity<PhongBan>(entity =>
        {
            entity.ToTable("PhongBan");
            entity.HasIndex(e => new { e.ChiNhanhId, e.MaPhongBan }).IsUnique();

            entity.HasOne(e => e.ChiNhanh)
                .WithMany()
                .HasForeignKey(e => e.ChiNhanhId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PhongBanCha)
                .WithMany(p => p.PhongBanCons)
                .HasForeignKey(e => e.PhongBanChaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TruongPhong)
                .WithMany()
                .HasForeignKey(e => e.TruongPhongId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ========================================================
        // 23. Cấu hình Kho & Hàng Tồn Kho (Phase 8 - VAS 02 / TT99)
        // ========================================================
        modelBuilder.Entity<Kho>(entity =>
        {
            entity.ToTable("Kho");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.ChiNhanhId, e.MaKho }).IsUnique();

            entity.HasOne(e => e.ChiNhanh)
                .WithMany()
                .HasForeignKey(e => e.ChiNhanhId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ThuKho)
                .WithMany()
                .HasForeignKey(e => e.ThuKhoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TaiKhoanKhoMacDinh)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanKhoMacDinhId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PhieuNhapKho>(entity =>
        {
            entity.ToTable("PhieuNhapKho");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.ChiNhanhId, e.SoPhieu }).IsUnique();

            entity.HasOne(e => e.ChiNhanh)
                .WithMany()
                .HasForeignKey(e => e.ChiNhanhId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Kho)
                .WithMany(k => k.PhieuNhapKhos)
                .HasForeignKey(e => e.KhoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.NhaCungCap)
                .WithMany()
                .HasForeignKey(e => e.NhaCungCapId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.HoaDonMuaHang)
                .WithMany()
                .HasForeignKey(e => e.HoaDonMuaHangId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ButToan)
                .WithMany()
                .HasForeignKey(e => e.ButToanId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ChiTietNhapKho>(entity =>
        {
            entity.ToTable("ChiTietNhapKho");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(e => e.PhieuNhapKho)
                .WithMany(p => p.ChiTietNhapKhos)
                .HasForeignKey(e => e.PhieuNhapKhoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.VatTuHangHoa)
                .WithMany()
                .HasForeignKey(e => e.VatTuHangHoaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanNo)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanNoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanCo)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanCoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PhieuXuatKho>(entity =>
        {
            entity.ToTable("PhieuXuatKho");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.ChiNhanhId, e.SoPhieu }).IsUnique();

            entity.HasOne(e => e.ChiNhanh)
                .WithMany()
                .HasForeignKey(e => e.ChiNhanhId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Kho)
                .WithMany(k => k.PhieuXuatKhos)
                .HasForeignKey(e => e.KhoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.KhachHang)
                .WithMany()
                .HasForeignKey(e => e.KhachHangId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PhongBan)
                .WithMany()
                .HasForeignKey(e => e.PhongBanId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.HoaDonBanHang)
                .WithMany()
                .HasForeignKey(e => e.HoaDonBanHangId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ButToan)
                .WithMany()
                .HasForeignKey(e => e.ButToanId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ChiTietXuatKho>(entity =>
        {
            entity.ToTable("ChiTietXuatKho");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(e => e.PhieuXuatKho)
                .WithMany(p => p.ChiTietXuatKhos)
                .HasForeignKey(e => e.PhieuXuatKhoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.VatTuHangHoa)
                .WithMany()
                .HasForeignKey(e => e.VatTuHangHoaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanNo)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanNoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaiKhoanCo)
                .WithMany()
                .HasForeignKey(e => e.TaiKhoanCoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
