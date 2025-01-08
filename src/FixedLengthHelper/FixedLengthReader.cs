using System.Text;

namespace FixedLengthHelper;

/// <summary>
/// Fixed-length reader.
/// </summary>
public sealed class FixedLengthReader : IFixedLengthReader
{
    /// <summary>
    /// Fixed-length reader.
    /// </summary>
    private readonly IByteStreamReader _byteStreamReader;

    /// <summary>
    /// Encoding.
    /// </summary>
    private readonly Encoding _encoding;

    /// <summary>
    /// Current line.
    /// </summary>
    private byte[]? _currentLine;

    /// <summary>
    /// Initializes a new instance of the FixedLengthReader class.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="encoding"></param>
    public FixedLengthReader(string path, Encoding encoding) 
        : this(new ByteStreamReader(File.Open(path, FileMode.Open), encoding), encoding)
    {
    }

    /// <summary>
    /// Initializes a new instance of the FixedLengthReader class.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    public FixedLengthReader(Stream stream, Encoding encoding)
        : this(new ByteStreamReader(stream, encoding), encoding)
    {
    }

    /// <summary>
    /// Fixed-length reader.
    /// </summary>
    /// <param name="byteStreamReader"></param>
    /// <param name="encoding"></param>
    private FixedLengthReader(IByteStreamReader byteStreamReader, 
        Encoding encoding)
    {
        _byteStreamReader = byteStreamReader;
        _encoding = encoding;
    }

    /// <inheritdoc />
    public bool IsClosed => _byteStreamReader.IsClosed;

    /// <inheritdoc />
    public byte[] CurrentRow
    {
        get
        {
            if (_currentLine == null)
            {
                throw new InvalidOperationException("Read method must be called first. And the end of the stream must not be reached.");
            }
            return _currentLine;
        }
    }

    /// <summary>
    /// Reads a line of bytes from the stream.
    /// </summary>
    /// <returns>If the end of the stream has been reached, returns false. </returns>
    public bool Read()
    {
        _currentLine = _byteStreamReader.ReadLine();
        return _currentLine != null;
    }

    /// <summary>
    /// Reads a line of bytes from the stream asynchronously.
    /// </summary>
    /// <returns>If the end of the stream has been reached, returns false. </returns>
    public async Task<bool> ReadAsync()
    {
        _currentLine = await _byteStreamReader.ReadLineAsync();
        return _currentLine != null;
    }

    /// <summary>
    /// Gets a field from the current line.
    /// </summary>
    /// <param name="offsetBytes">Offset in lengthBytes from the beginning of the line. </param>
    /// <param name="lengthBytes">Length of the field in bytes.</param>
    /// <param name="trimMode">Trim mode.</param>
    /// <param name="trimChars">Characters to trim.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public string GetField(int offsetBytes, int lengthBytes, TrimMode trimMode = TrimMode.None, params char[] trimChars)
    {
        var span = _currentLine.AsSpan(offsetBytes, lengthBytes);
#if NET48_OR_GREATER
        var field = _encoding.GetString(span.ToArray());
#else
        var field = _encoding.GetString(span);
#endif
        return trimMode switch
        {
            TrimMode.None => field,
            TrimMode.Trim => field.Trim(trimChars),
            TrimMode.TrimStart => field.TrimStart(trimChars),
            TrimMode.TrimEnd => field.TrimEnd(trimChars),
            _ => throw new ArgumentException("Invalid TrimMode", nameof(trimMode))
        };
    }

    /// <summary>
    /// Closes the FixedLengthReader.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }
    
    /// <summary>
    /// Closes the FixedLengthReader.
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _byteStreamReader.Dispose();
        }
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Closes the FixedLengthReader asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _byteStreamReader.DisposeAsync();
    }
#endif
}