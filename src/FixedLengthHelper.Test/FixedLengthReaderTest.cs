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
        using IFixedLengthReader reader = new FixedLengthReader(line.ToStream(encoding), encoding);

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
        using IFixedLengthReader reader = new FixedLengthReader(line.ToStream(encoding), encoding);

        // Act & Assert
        (await reader.ReadAsync()).Should().BeTrue();
        reader.GetField(encoding.GetBytes(offset).Length, encoding.GetBytes(expected).Length).Should().Be(expected);
    }

    [Theory]
    [InlineData("Shift_JIS", "01  456  9", null, null, "  456  ")]
    [InlineData("Shift_JIS", "01  456  9", TrimMode.None, null, "  456  ")]
    [InlineData("Shift_JIS", "01  456  9", TrimMode.Trim, null, "456")]
    [InlineData("Shift_JIS", "01  456  9", TrimMode.TrimStart, null, "456  ")]
    [InlineData("Shift_JIS", "01  456  9", TrimMode.TrimEnd, null, "  456")]
    [InlineData("Shift_JIS", "01xy456z09", TrimMode.Trim, new[]{'x', 'y', 'z', '0'}, "456")]
    public void GetField_WithTrim(string encodingName, string line, TrimMode? trimMode, char[]? trimChars, string expected)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        using IFixedLengthReader reader = new FixedLengthReader(line.ToStream(encoding), encoding);

        // Act & Assert
        reader.Read().Should().BeTrue();
        if (trimMode is null && trimChars is null)
        {
            reader.GetField(2, 7).Should().Be(expected);
        }
        else if (trimChars is null)
        {
            reader.GetField(2, 7, trimMode!.Value).Should().Be(expected);
        }
        else
        {
            reader.GetField(2, 7, trimMode!.Value, trimChars).Should().Be(expected);
        }
    }


    [Theory]
    [InlineData("Sample-UTF8.txt", false)]
    [InlineData("Sample-UTF8-with-BOM.txt", true)]
    public void Constructor_FromFile(string fileName, bool withBom)
    {
        Encoding encoding = withBom 
            ? new UTF8Encoding(true) 
            : new UTF8Encoding(false);
        // Arrange
        IFixedLengthReader reader = new FixedLengthReader(fileName, encoding);

        // Act & Assert
        reader.Read().Should().BeTrue();
        reader.GetField(0, 5, TrimMode.TrimStart, '0').Should().Be("554");
    }

    [Fact]
    public async Task GetField_WithInvalidTrim()
    {
        // Arrange
#if NET48_OR_GREATER
        using IFixedLengthReader reader = new FixedLengthReader("hello".ToStream(Encoding.UTF8), Encoding.UTF8);
#else
        await using IFixedLengthReader reader = new FixedLengthReader("hello".ToStream(Encoding.UTF8), Encoding.UTF8);
#endif

        // Act
        (await reader.ReadAsync()).Should().BeTrue();
        // ReSharper disable once AccessToDisposedClosure
        var act = () => reader.GetField(1, 2, (TrimMode)9);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Shift_JIS", "0123456789", -1, 2)]
    [InlineData("Shift_JIS", "0123456789", 11, 2)]
    [InlineData("Shift_JIS", "0123456789", 0, 11)]
    public void GetField_OutOfRange(string encodingName, string line, int offset, int length)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        using IFixedLengthReader reader = new FixedLengthReader(line.ToStream(encoding), encoding);

        // Act & Assert
        reader.Read().Should().BeTrue();
        // ReSharper disable once AccessToDisposedClosure
        var act = () => reader.GetField(offset, length);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

}