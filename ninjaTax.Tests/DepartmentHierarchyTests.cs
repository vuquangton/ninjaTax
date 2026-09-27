using ninjaTax.Models.Entities;
using Xunit;

namespace ninjaTax.Tests;

public class DepartmentHierarchyTests
{
    [Fact]
    public void KiemTraChuTrinh_ProposedParentNull_ReturnsFalse()
    {
        // Gán thành phòng ban cấp cao nhất (không có cha)
        var isCycle = PhongBan.KiemTraChuTrinhDeQuy(1, null, id => null);
        Assert.False(isCycle);
    }

    [Fact]
    public void KiemTraChuTrinh_SelfReference_ReturnsTrue()
    {
        // Phòng ban tự chọn chính nó làm phòng ban cha (A -> A)
        var isCycle = PhongBan.KiemTraChuTrinhDeQuy(5, 5, id => null);
        Assert.True(isCycle);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            PhongBan.XacThucPhongBanCha(5, 5, id => null));
        Assert.Contains("chu trình", ex.Message);
    }

    [Fact]
    public void KiemTraChuTrinh_ValidParentChild_ReturnsFalse()
    {
        // 1 (Khối Kinh Doanh) là cha của 2 (Phòng Bán Hàng 1) -> Hợp lệ
        var parentMap = new Dictionary<long, long?>
        {
            { 1, null }, // 1 không có cha
            { 2, 1 }     // 2 có cha là 1
        };

        // Gán 3 (Nhóm Sale 1) có cha là 2 -> Hợp lệ
        var isCycle = PhongBan.KiemTraChuTrinhDeQuy(3, 2, id => parentMap.TryGetValue(id, out var p) ? p : null);
        Assert.False(isCycle);

        // Không ném exception
        PhongBan.XacThucPhongBanCha(3, 2, id => parentMap.TryGetValue(id, out var p) ? p : null);
    }

    [Fact]
    public void KiemTraChuTrinh_DirectTwoLevelCycle_ReturnsTrue()
    {
        // Ban đầu: 1 là cha của 2 (1 -> 2)
        var parentMap = new Dictionary<long, long?>
        {
            { 1, null },
            { 2, 1 }
        };

        // Thử đổi cha của 1 thành 2 (Tạo chu trình 1 -> 2 -> 1)
        var isCycle = PhongBan.KiemTraChuTrinhDeQuy(1, 2, id => parentMap.TryGetValue(id, out var p) ? p : null);
        Assert.True(isCycle);

        Assert.Throws<InvalidOperationException>(() =>
            PhongBan.XacThucPhongBanCha(1, 2, id => parentMap.TryGetValue(id, out var p) ? p : null));
    }

    [Fact]
    public void KiemTraChuTrinh_MultiLevelCycle_ReturnsTrue()
    {
        // Cấu trúc cây: 1 (Khối) -> 2 (Ban) -> 3 (Phòng) -> 4 (Tổ)
        var parentMap = new Dictionary<long, long?>
        {
            { 1, null },
            { 2, 1 },
            { 3, 2 },
            { 4, 3 }
        };

        // Thử gán cha của 1 là 4 (Tạo chu trình 1 -> 2 -> 3 -> 4 -> 1)
        var isCycle1 = PhongBan.KiemTraChuTrinhDeQuy(1, 4, id => parentMap.TryGetValue(id, out var p) ? p : null);
        Assert.True(isCycle1);

        // Thử gán cha của 2 là 4 (Tạo chu trình 2 -> 3 -> 4 -> 2)
        var isCycle2 = PhongBan.KiemTraChuTrinhDeQuy(2, 4, id => parentMap.TryGetValue(id, out var p) ? p : null);
        Assert.True(isCycle2);

        // Thử gán cha của 2 là 3 (Tạo chu trình 2 -> 3 -> 2)
        var isCycle3 = PhongBan.KiemTraChuTrinhDeQuy(2, 3, id => parentMap.TryGetValue(id, out var p) ? p : null);
        Assert.True(isCycle3);
    }

    [Fact]
    public void KiemTraChuTrinh_DeepHierarchyFiveLevels_DetectsCycle()
    {
        // 10 -> 20 -> 30 -> 40 -> 50
        var parentMap = new Dictionary<long, long?>
        {
            { 10, null },
            { 20, 10 },
            { 30, 20 },
            { 40, 30 },
            { 50, 40 }
        };

        // Gán cha của 10 là 50 -> Chu trình
        Assert.True(PhongBan.KiemTraChuTrinhDeQuy(10, 50, id => parentMap.TryGetValue(id, out var p) ? p : null));

        // Gán cha của 30 là 50 -> Chu trình
        Assert.True(PhongBan.KiemTraChuTrinhDeQuy(30, 50, id => parentMap.TryGetValue(id, out var p) ? p : null));
    }

    [Fact]
    public void KiemTraChuTrinh_IndependentSubtrees_ReturnsFalse()
    {
        // Nhánh A: 1 -> 2
        // Nhánh B: 3 -> 4
        var parentMap = new Dictionary<long, long?>
        {
            { 1, null },
            { 2, 1 },
            { 3, null },
            { 4, 3 }
        };

        // Di chuyển node 2 từ Nhánh A sang làm con của node 4 thuộc Nhánh B
        // Kết quả mới: 3 -> 4 -> 2 (Không có chu trình)
        var isCycle = PhongBan.KiemTraChuTrinhDeQuy(2, 4, id => parentMap.TryGetValue(id, out var p) ? p : null);
        Assert.False(isCycle);
    }

    [Theory]
    [InlineData(LoaiPhongBan.BanHang, "6421")]
    [InlineData(LoaiPhongBan.QuanLy, "6422")]
    [InlineData(LoaiPhongBan.SanXuat, "154")]
    [InlineData(LoaiPhongBan.KhoaChuyenMon, "154")]
    [InlineData(LoaiPhongBan.Khac, "6422")]
    public void LoaiPhongBan_LayMaTaiKhoanChiPhiMacDinh_ReturnsExpectedTT99Accounts(LoaiPhongBan loai, string expectedAccount)
    {
        var actualAccount = loai.LayMaTaiKhoanChiPhiMacDinh();
        Assert.Equal(expectedAccount, actualAccount);
    }
}
