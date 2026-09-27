using ninjaTax.Models.Services;
using Xunit;

namespace ninjaTax.Tests;

public class BusinessExceptionTests
{
    [Fact]
    public void BusinessException_CarriesUserMessageAndField()
    {
        var ex = new BusinessException("TK 1111 không đủ số dư (Thiếu 5.000.000đ)", "SoTien");
        Assert.Equal("TK 1111 không đủ số dư (Thiếu 5.000.000đ)", ex.UserMessage);
        Assert.Equal("SoTien", ex.FieldName);
    }

    [Fact]
    public void BusinessException_NullFieldName_IsAllowed()
    {
        var ex = new BusinessException("Lỗi nghiệp vụ");
        Assert.Equal("Lỗi nghiệp vụ", ex.UserMessage);
        Assert.Null(ex.FieldName);
    }

    [Fact]
    public void BusinessException_InheritsFromException()
    {
        var ex = new BusinessException("Test message");
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void BusinessException_MessagePropertyEqualsUserMessage()
    {
        const string msg = "Ngày hóa đơn không được lớn hơn ngày hiện tại";
        var ex = new BusinessException(msg, "NgayHoaDon");
        // Exception.Message should also return the user message for logging
        Assert.Equal(msg, ex.Message);
    }
}
