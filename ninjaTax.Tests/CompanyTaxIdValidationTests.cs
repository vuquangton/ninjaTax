using ninjaTax.Models.Entities;
using Xunit;

namespace ninjaTax.Tests;

public class CompanyTaxIdValidationTests
{
    [Theory]
    [InlineData("0100109106")] // Viettel Group
    [InlineData("0101245486")] // Vingroup JSC
    [InlineData("0109998883")] // Worked example
    public void KiemTraMstHopLe_HopLeTheoModulo11_TraVeTrue(string mst)
    {
        var isValid = ThongTinDoanhNghiep.KiemTraMstHopLe(mst);
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("0100109107")] // Sai check digit
    [InlineData("0101245480")] // Sai check digit
    [InlineData("123456789")]  // Chỉ có 9 chữ số
    [InlineData("12345678901")] // 11 chữ số
    [InlineData("ABC1234567")] // Có ký tự chữ
    [InlineData("")]           // Rỗng
    [InlineData(null)]         // Null
    public void KiemTraMstHopLe_SaiDinhDangHoacSaiCheckDigit_TraVeFalse(string? mst)
    {
        var isValid = ThongTinDoanhNghiep.KiemTraMstHopLe(mst);
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("0100109106-001", "0100109106")]
    [InlineData("0101245486-099", "0101245486")]
    [InlineData("0109998883-123", "0109998883")]
    public void KiemTraMstChiNhanhHopLe_DungDinhDang13So_TraVeTrue(string mstChiNhanh, string mstCongTyMe)
    {
        var isValid = ChiNhanh.KiemTraMstChiNhanhHopLe(mstChiNhanh, mstCongTyMe);
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("0100109106-000", "0100109106")] // Suffix 000 không hợp lệ (phải từ 001-999)
    [InlineData("0100109106-1000", "0100109106")] // Suffix 4 số
    [InlineData("0100109106001", "0100109106")] // Thiếu dấu gạch ngang
    [InlineData("0101245486-001", "0100109106")] // Không khớp MST công ty mẹ
    [InlineData("9999999999-001", "9999999999")] // 10 số đầu sai Modulo 11
    public void KiemTraMstChiNhanhHopLe_SaiDinhDang_TraVeFalse(string mstChiNhanh, string mstCongTyMe)
    {
        var isValid = ChiNhanh.KiemTraMstChiNhanhHopLe(mstChiNhanh, mstCongTyMe);
        Assert.False(isValid);
    }
}

