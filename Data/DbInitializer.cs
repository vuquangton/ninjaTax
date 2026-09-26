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
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        await SeedDataAsync(context);
    }

    public static void Initialize(AppDbContext context)
    {
        SeedDataAsync(context).GetAwaiter().GetResult();
    }

    public static async Task SeedDataAsync(AppDbContext context)
    {
        if (await context.TaiKhoans.AnyAsync())
        {
            return; // Đã có dữ liệu, không nạp lại
        }

        // Danh mục tài khoản chuẩn TT99 (Nghiêm cấm TK 911)
        var taiKhoans = new List<TaiKhoan>
        {
            // Nhóm 1: Tài sản ngắn hạn
            new() { MaTaiKhoan = "111", TenTaiKhoan = "Tiền mặt", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "1111", TenTaiKhoan = "Tiền Việt Nam", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "112", TenTaiKhoan = "Tiền gửi ngân hàng", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "1121", TenTaiKhoan = "Tiền Việt Nam gửi ngân hàng", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "131", TenTaiKhoan = "Phải thu của khách hàng", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "133", TenTaiKhoan = "Thuế GTGT được khấu trừ", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "1331", TenTaiKhoan = "Thuế GTGT được khấu trừ của hàng hóa, dịch vụ", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "141", TenTaiKhoan = "Tạm ứng", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "152", TenTaiKhoan = "Nguyên liệu, vật liệu", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "156", TenTaiKhoan = "Hàng hóa", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "1561", TenTaiKhoan = "Giá mua hàng hóa", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },

            // Nhóm 2: Tài sản dài hạn
            new() { MaTaiKhoan = "211", TenTaiKhoan = "Tài sản cố định hữu hình", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "214", TenTaiKhoan = "Hao mòn tài sản cố định", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "2141", TenTaiKhoan = "Hao mòn tài sản cố định hữu hình", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "242", TenTaiKhoan = "Chi phí trả trước", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.TaiSan, TinhChat = TinhChatTaiKhoan.DuNo, LaTaiKhoanSoCai = false },

            // Nhóm 3: Nợ phải trả
            new() { MaTaiKhoan = "331", TenTaiKhoan = "Phải trả cho người bán", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "333", TenTaiKhoan = "Thuế và các khoản phải nộp Nhà nước", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "3331", TenTaiKhoan = "Thuế giá trị gia tăng phải nộp", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "33311", TenTaiKhoan = "Thuế giá trị gia tăng đầu ra", BacTaiKhoan = 3, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "3335", TenTaiKhoan = "Thuế thu nhập cá nhân", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "334", TenTaiKhoan = "Phải trả người lao động", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "338", TenTaiKhoan = "Phải trả, phải nộp khác", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "3382", TenTaiKhoan = "Kinh phí công đoàn", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "3383", TenTaiKhoan = "Bảo hiểm xã hội", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "3384", TenTaiKhoan = "Bảo hiểm y tế", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "3386", TenTaiKhoan = "Bảo hiểm thất nghiệp", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.NoPhaiTra, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },

            // Nhóm 4: Vốn chủ sở hữu (Đặc thù TT99: 421 nhận kết chuyển doanh thu, chi phí trực tiếp)
            new() { MaTaiKhoan = "411", TenTaiKhoan = "Vốn đầu tư của chủ sở hữu", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.VonChuSoHuu, TinhChat = TinhChatTaiKhoan.DuCo, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "421", TenTaiKhoan = "Lợi nhuận sau thuế chưa phân phối", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.VonChuSoHuu, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "4211", TenTaiKhoan = "Lợi nhuận sau thuế chưa phân phối năm trước", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.VonChuSoHuu, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "4212", TenTaiKhoan = "Lợi nhuận sau thuế chưa phân phối năm nay", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.VonChuSoHuu, TinhChat = TinhChatTaiKhoan.LuongTinh, LaTaiKhoanSoCai = false },

            // Nhóm 5: Doanh thu
            new() { MaTaiKhoan = "511", TenTaiKhoan = "Doanh thu bán hàng và cung cấp dịch vụ", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = true },
            new() { MaTaiKhoan = "5111", TenTaiKhoan = "Doanh thu bán hàng hóa", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "5112", TenTaiKhoan = "Doanh thu bán các thành phẩm", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "5113", TenTaiKhoan = "Doanh thu cung cấp dịch vụ", BacTaiKhoan = 2, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "515", TenTaiKhoan = "Doanh thu hoạt động tài chính", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.DoanhThu, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },

            // Nhóm 6: Chi phí sản xuất, kinh doanh
            new() { MaTaiKhoan = "632", TenTaiKhoan = "Giá vốn hàng bán", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "635", TenTaiKhoan = "Chi phí tài chính", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "641", TenTaiKhoan = "Chi phí bán hàng", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "642", TenTaiKhoan = "Chi phí quản lý doanh nghiệp", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhi, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },

            // Nhóm 7 & 8: Thu nhập khác và Chi phí khác
            new() { MaTaiKhoan = "711", TenTaiKhoan = "Thu nhập khác", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ThuNhapKhac, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false },
            new() { MaTaiKhoan = "811", TenTaiKhoan = "Chi phí khác", BacTaiKhoan = 1, LoaiTaiKhoan = LoaiTaiKhoan.ChiPhiKhac, TinhChat = TinhChatTaiKhoan.KhongCoSoDu, LaTaiKhoanSoCai = false }
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
    }
}
