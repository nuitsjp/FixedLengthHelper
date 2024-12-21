using System.Diagnostics.CodeAnalysis;
using System.Text;
using FluentAssertions;
using Moq;

namespace FixedLengthHelper.Test;

[SuppressMessage("Performance", "CA1859:可能な場合は具象型を使用してパフォーマンスを向上させる")]
public class FixedLengthReaderTest
{
#if NET8_0_OR_GREATER
    static FixedLengthReaderTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
#endif

    [Theory]
    // ASCII Characters
    [InlineData("Shift_JIS", "0123456789", "0", "12")]
    [InlineData("UTF-8", "0123456789", "0", "12")]
    // Multibyte Characters
    [InlineData("Shift_JIS", "0あ3456789", "0", "あ")]
    [InlineData("UTF-8", "0あ3456789", "0", "あ")]
    // Surrogate Pairs
    [InlineData("UTF-8", "👨👩👧👦家族", "👨👩", "👧👦家族")]
    public void GetField(string encodingName, string line, string offset, string expected)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        Mock<IByteStreamReader> byteStreamReader = new();
        byteStreamReader
            .Setup(x => x.ReadLine())
            .Returns(encoding.GetBytes(line));
        using IFixedLengthReader reader = new FixedLengthReader(byteStreamReader.Object, encoding);

        // Act & Assert
        reader.Read().Should().BeTrue();
        reader.GetField(encoding.GetBytes(offset).Length, encoding.GetBytes(expected).Length).Should().Be(expected);
    }

    [Theory]
    // ASCII Characters
    [InlineData("Shift_JIS", "0123456789", "0", "12")]
    [InlineData("UTF-8", "0123456789", "0", "12")]
    // Multibyte Characters
    [InlineData("Shift_JIS", "0あ3456789", "0", "あ")]
    [InlineData("UTF-8", "0あ3456789", "0", "あ")]
    // Surrogate Pairs
    [InlineData("UTF-8", "👨👩👧👦家族", "👨👩", "👧👦家族")]
    public async Task GetField_WithReadAsync(string encodingName, string line, string offset, string expected)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        Mock<IByteStreamReader> byteStreamReader = new();
        byteStreamReader
            .Setup(x => x.ReadLineAsync())
            .ReturnsAsync(encoding.GetBytes(line));
#if NET48_OR_GREATER
        using IFixedLengthReader reader = new FixedLengthReader(byteStreamReader.Object, encoding);
#else
        await using IFixedLengthReader reader = new FixedLengthReader(byteStreamReader.Object, encoding);
#endif

        // Act & Assert
        (await reader.ReadAsync()).Should().BeTrue();
        reader.GetField(encoding.GetBytes(offset).Length, encoding.GetBytes(expected).Length).Should().Be(expected);
    }

    [Fact]
    public void Constructor_FromFile()
    {
        // Arrange
        IFixedLengthReader reader = new FixedLengthReader("Sample.txt", Encoding.UTF8);

        // Act & Assert
        reader.Read().Should().BeTrue();
        reader.GetField(0, 5).Should().Be("00554");
    }

    [Theory]
    [InlineData("Shift_JIS", "0123456789", -1, 2)]
    [InlineData("Shift_JIS", "0123456789", 11, 2)]
    [InlineData("Shift_JIS", "0123456789", 0, 11)]
    public void GetField_OutOfRange(string encodingName, string line, int offset, int length)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        Mock<IByteStreamReader> byteStreamReader = new();
        byteStreamReader
            .Setup(x => x.ReadLine())
            .Returns(encoding.GetBytes(line));
        using IFixedLengthReader reader = new FixedLengthReader(byteStreamReader.Object, encoding);

        // Act & Assert
        reader.Read().Should().BeTrue();
        // ReSharper disable once AccessToDisposedClosure
        var act = () => reader.GetField(offset, length);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

}