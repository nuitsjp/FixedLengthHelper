using System.Diagnostics.CodeAnalysis;
using System.Text;
using FluentAssertions;
using Moq;

namespace FixedLengthHelper.Test;

[SuppressMessage("Performance", "CA1859:可能な場合は具象型を使用してパフォーマンスを向上させる")]
[SuppressMessage("Performance", "CA1806:メソッドの結果を無視しない")]
public class ByteStreamReaderTest
{
#if NET8_0_OR_GREATER
    static ByteStreamReaderTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
#endif

    [Theory]
    [InlineData("Shift_JIS", "\r", null)]
    [InlineData("UTF-8", "\r", null)]
    [InlineData("Shift_JIS", "\n", null)]
    [InlineData("UTF-8", "\n", null)]
    [InlineData("Shift_JIS", "\r\n", null)]
    [InlineData("UTF-8", "\r\n", null)]
    [InlineData("Shift_JIS", "\r", 80000)]
    [InlineData("UTF-8", "\r", 80000)]
    [InlineData("Shift_JIS", "\n", 80000)]
    [InlineData("UTF-8", "\n", 80000)]
    [InlineData("Shift_JIS", "\r\n", 80000)]
    [InlineData("UTF-8", "\r\n", 80000)]
    [InlineData("Shift_JIS", "\r", 16001)]
    [InlineData("UTF-8", "\r", 24001)]
    [InlineData("Shift_JIS", "\n", 16001)]
    [InlineData("UTF-8", "\n", 8001)]
    [InlineData("Shift_JIS", "\r\n", 16001)]
    [InlineData("UTF-8", "\r\n", 24001)]
    [InlineData("Shift_JIS", "\r\n", 16002)]
    [InlineData("UTF-8", "\r\n", 24002)]
    public void ReadLine(string encodingName, string newline, int? bufferSize)
    {
        // Arrange
        var encoding = encodingName == "UTF-8"
            ? new UTF8Encoding(false)
            : Encoding.GetEncoding(encodingName);
        var first = new string('あ', 8000);
        var second = new string('A', 4096);
        const string third = "123";
        var forth = new string('B', 100);
        var content = first + newline + second + newline + third + newline + forth;
        var stream = new MemoryStream(encoding.GetBytes(content));
        using var reader = new ByteStreamReader(stream, encoding, bufferSize);

        // Act
        reader.ReadLine().Should().BeEquivalentTo(encoding.GetBytes(first));
        reader.ReadLine().Should().BeEquivalentTo(encoding.GetBytes(second));
        reader.ReadLine().Should().BeEquivalentTo(encoding.GetBytes(third));
        reader.ReadLine().Should().BeEquivalentTo(encoding.GetBytes(forth));
        reader.ReadLine().Should().BeNull();
    }

    [Theory]
    [InlineData("Shift_JIS", "\r", null)]
    [InlineData("UTF-8", "\r", null)]
    [InlineData("Shift_JIS", "\n", null)]
    [InlineData("UTF-8", "\n", null)]
    [InlineData("Shift_JIS", "\r\n", null)]
    [InlineData("UTF-8", "\r\n", null)]
    [InlineData("Shift_JIS", "\r", 80000)]
    [InlineData("UTF-8", "\r", 80000)]
    [InlineData("Shift_JIS", "\n", 80000)]
    [InlineData("UTF-8", "\n", 80000)]
    [InlineData("Shift_JIS", "\r\n", 80000)]
    [InlineData("UTF-8", "\r\n", 80000)]
    [InlineData("Shift_JIS", "\r", 16001)]
    [InlineData("UTF-8", "\r", 24001)]
    [InlineData("Shift_JIS", "\n", 16001)]
    [InlineData("UTF-8", "\n", 8001)]
    [InlineData("Shift_JIS", "\r\n", 16001)]
    [InlineData("UTF-8", "\r\n", 24001)]
    [InlineData("Shift_JIS", "\r\n", 16002)]
    [InlineData("UTF-8", "\r\n", 24002)]
    public async Task ReadLineAsync(string encodingName, string newline, int? bufferSize)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        var first = new string('あ', 8000);
        var second = new string('A', 4096);
        const string third = "123";
        var forth = new string('B', 100);
        var content = first + newline + second + newline + third + newline + forth;
        var stream = new MemoryStream(encoding.GetBytes(content));
#if NET48_OR_GREATER
        using IByteStreamReader reader = new ByteStreamReader(stream, encoding, bufferSize);
#else
        await using IByteStreamReader reader = new ByteStreamReader(stream, encoding, bufferSize);
#endif

        // Act
        (await reader.ReadLineAsync()).Should().BeEquivalentTo(encoding.GetBytes(first));
        (await reader.ReadLineAsync()).Should().BeEquivalentTo(encoding.GetBytes(second));
        (await reader.ReadLineAsync()).Should().BeEquivalentTo(encoding.GetBytes(third));
        (await reader.ReadLineAsync()).Should().BeEquivalentTo(encoding.GetBytes(forth));
        (await reader.ReadLineAsync(new CancellationTokenSource().Token)).Should().BeNull();
    }

    [Fact]
    public void CloseAndDispose()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
        IByteStreamReader reader = new ByteStreamReader(stream, Encoding.UTF8);

        reader.Close();
        reader.Dispose();

        stream.CanRead.Should().BeFalse();
    }

#if NET8_0_OR_GREATER
    [Fact]
    public async Task CloseAndDisposeAsync()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
        IByteStreamReader reader = new ByteStreamReader(stream, Encoding.UTF8);

        reader.Close();
        await reader.DisposeAsync();

        stream.CanRead.Should().BeFalse();
    }
#endif


    [Fact]
    public void Dispose_WhenCloseOfStreamThrowException()
    {
        // Arrange
        Mock<Stream> streamMock = new();
        streamMock.SetupGet(x => x.CanRead).Returns(true);
        streamMock.Setup(x => x.Close()).Throws(new Exception());
        

        IByteStreamReader reader = new ByteStreamReader(streamMock.Object, Encoding.UTF8);

        // Act
        var act = () => reader.Dispose();

        // Assert
        act.Should().NotThrow();
    }

#if NET8_0_OR_GREATER
    [Fact]
    public async Task DisposeAsync()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
        IByteStreamReader reader = new ByteStreamReader(stream, Encoding.UTF8);

        await reader.DisposeAsync();

        stream.CanRead.Should().BeFalse();
    }

    [Fact]
    public async Task DisposeAsync_WhenCloseOfStreamThrowException()
    {
        // Arrange
        Mock<Stream> streamMock = new();
        streamMock.SetupGet(x => x.CanRead).Returns(true);
        streamMock.Setup(x => x.DisposeAsync()).Throws(new Exception());


        IByteStreamReader reader = new ByteStreamReader(streamMock.Object, Encoding.UTF8);

        // Act
        var act = async () => await reader.DisposeAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }
#endif

    [Fact]
    public void Constructor_StreamCanNotRead()
    {
        var stream = new MemoryStream();
        stream.Close();
        stream.CanRead.Should().BeFalse();
        // ReSharper disable once ObjectCreationAsStatement
        Action action = () => new ByteStreamReader(stream, Encoding.UTF8);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_BufferNegativeOrZero(int bufferSize)
    {
        using var stream = new MemoryStream();
        // ReSharper disable once AccessToDisposedClosure
        // ReSharper disable once ObjectCreationAsStatement
        Action action = () => new ByteStreamReader(stream, Encoding.UTF8, bufferSize);
        action.Should().Throw<ArgumentException>();
    }

}