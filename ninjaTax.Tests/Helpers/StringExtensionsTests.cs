using ninjaTax.Helpers;
using Xunit;

namespace ninjaTax.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void RemoveDiacritics_VietnameseText_ReturnsUnaccented()
    {
        Assert.Equal("Nguyen Van A", "Nguyễn Văn A".RemoveDiacritics());
        Assert.Equal("cong ty ABC", "công ty ABC".RemoveDiacritics());
    }

    [Fact]
    public void RemoveDiacritics_DCharacter_ReturnsD()
    {
        // đ/Đ are not decomposed by Unicode normalization, must be handled explicitly
        Assert.Equal("duong", "đường".RemoveDiacritics());
        Assert.Equal("Da Nang", "Đà Nẵng".RemoveDiacritics());
    }

    [Fact]
    public void RemoveDiacritics_EmptyString_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, string.Empty.RemoveDiacritics());
    }

    [Fact]
    public void RemoveDiacritics_AsciiText_Unchanged()
    {
        Assert.Equal("Hello World 123", "Hello World 123".RemoveDiacritics());
    }

    [Fact]
    public void RemoveDiacritics_MixedText_StripsOnlyDiacritics()
    {
        Assert.Equal("Nguyen Thi Bich Hang", "Nguyễn Thị Bích Hằng".RemoveDiacritics());
    }
}
