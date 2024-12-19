using System.Text;
using FluentAssertions;

namespace FixedLengthHelper.Test;

public class FixedLengthDataReaderTest
{
#if NET8_0_OR_GREATER
    static FixedLengthDataReaderTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
#endif

    [Fact]
    public void GetValue()
    {
        // Arrange
        var stream = new MemoryStream(
            """
            00554Pedro Gomez          123423006022004
            01732中村 充志        004350011052002
            00112Ramiro Politti       000000001022000
            00924Pablo Ramirez        033213024112002
            """u8.ToArray());

        // Act
        using var reader = FixedLengthDataReader
            .CreateBuilder()
            .AddColumn(0, 5)
            .AddColumn(5, 21)
            .AddColumn(26, 15)
            .Build(stream, Encoding.UTF8);

        // Assert
        reader.Read().Should().BeTrue();
        reader.GetValue(0).Should().Be("00554");
        reader.GetValue(1).Should().Be("Pedro Gomez          ");
        reader.GetValue(2).Should().Be("123423006022004");

        reader.Read().Should().BeTrue();
        reader.GetValue(0).Should().Be("01732");
        reader.GetValue(1).Should().Be("中村 充志        ");
        reader.GetValue(2).Should().Be("004350011052002");

        reader.Read().Should().BeTrue();
        reader.Read().Should().BeTrue();
        reader.Read().Should().BeFalse();
    }

    [Fact]
    public void GetOrdinal()
    {
        // Arrange
        var stream = new MemoryStream(""u8.ToArray());

        // Act
        using var reader = FixedLengthDataReader
            .CreateBuilder()
            .AddColumn("CustomerId", 0, 5)
            .AddColumn("Name", 5, 21)
            .AddColumn("Balance", 26, 15)
            .Build(stream, Encoding.UTF8);

        // Assert
        reader.GetOrdinal("CustomerId").Should().Be(0);
        reader.GetOrdinal("Name").Should().Be(1);
        reader.GetOrdinal("Balance").Should().Be(2);
    }
}