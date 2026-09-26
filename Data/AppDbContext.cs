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
/// - Tất cả các trường tiền tệ decimal đều được cấu hình độ chính xác cao HasPrecision(19, 4).
/// - Tuyệt đối không khai báo tài khoản 911.
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
        });
    }
}
