using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FixedLengthHelper;

/// <summary>
/// Reads a line of bytes from a stream.
/// </summary>
internal class ByteStreamReader : IDisposable
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable ConvertToConstant.Global
    public static readonly int DefaultBufferSize = 4096;  // Byte buffer size
    public static readonly int MinBufferSize = 128;
    // ReSharper restore ConvertToConstant.Global
    // ReSharper restore MemberCanBePrivate.Global

    /// <summary>
    /// Source stream.
    /// </summary>
    private readonly Stream _stream;

    /// <summary>
    /// Encoding used to read the stream.
    /// </summary>
    internal Encoding Encoding { get; }

    /// <summary>
    /// When true, the first line has been read.
    /// </summary>
    private bool _isFirstLine = true;

    /// <summary>
    /// Buffer used for reading from the stream.
    /// </summary>
    private byte[] _buffer;

    /// <summary>
    /// Number of bytes read into the buffer.
    /// </summary>
    private int _readLength;

    /// <summary>
    /// Position in the buffer.
    /// </summary>
    private int _readPosition;

    /// <summary>
    /// True if the writer has been disposed; otherwise, false.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Task representing the asynchronous read operation.
    /// </summary>
    private Task _asyncReadTask = Task.CompletedTask;

    /// <summary>
    /// Checks if an async read operation is in progress.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    [ExcludeFromCodeCoverage]
    private void CheckAsyncTaskInProgress()
    {
        // If the task is not yet completed, throw an exception.
        if (!_asyncReadTask.IsCompleted)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteStreamReader"/> class for the specified stream.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <param name="bufferSize"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal ByteStreamReader(Stream stream, Encoding encoding, int? bufferSize = null)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException("Stream can not read.");
        }

        if (bufferSize < MinBufferSize)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        _stream = stream;
        Encoding = encoding;
        _buffer = bufferSize is null
            ? new byte[DefaultBufferSize]
            : new byte[bufferSize.Value];
    }

    /// <summary>
    /// Gets a value indicating whether the stream is closed.
    /// </summary>
    internal bool IsClosed => _stream.CanRead is false;

    /// <summary>
    /// Reads a line of bytes from the stream.
    /// </summary>
    /// <returns>
    /// Read bytes. If the end of the stream has been reached, returns null.
    /// </returns>
    internal byte[]? ReadLine()
    {
        // If we're at the end of the buffer data, read from the stream.
        CheckAsyncTaskInProgress();

        // If we're at the end of the buffer data, read from the stream.
        if (_readPosition == _readLength)
        {
            // If we're already at the end of the stream, return null.
            if (ReadByteBuffer() == 0)
            {
                return null;
            }
        }

        do
        {
            // Span of the buffer data.
            ReadOnlySpan<byte> bufferSpan = _buffer.AsSpan(_readPosition, _readLength - _readPosition);

            // Look for '\r' or \'n'.
            var indexOfNewline = bufferSpan.IndexOfAny((byte)'\r', (byte)'\n');

            // If we found '\r' or '\n', return the line.
            if (0 <= indexOfNewline)
            {
                // Read the line.
                var line = _buffer.AsSpan(0, _readPosition + indexOfNewline).ToArray();

                // Get the matched 'new line' character.
                var matchedChar = bufferSpan[indexOfNewline];

                // Get the index of the matched character.
                var enterIndexOfNewline = indexOfNewline + 1;

                // Shift the read position.
                _readPosition += enterIndexOfNewline;

                // If we found '\r', consume any immediately following '\n'.
                if (matchedChar == '\r')
                {
                    // If we reached the end of the buffer, read the next buffer.
                    if (_readPosition == _readLength)
                    {
                        ReadByteBuffer();
                        enterIndexOfNewline = 0;
                    }

                    // If the next character is '\n', consume it.
                    if (_readPosition < _readLength)
                    {
                        if (bufferSpan[enterIndexOfNewline] == '\n')
                        {
                            _readPosition++;
                        }
                    }
                }

                // Shift the buffer data.
                var span = _buffer.AsSpan();
                span.Slice(_readPosition).CopyTo(span);

                // Update the read length and position.
                _readLength -= _readPosition;
                _readPosition = 0;

                return line;
            }

            _readPosition = _readLength;
        } while (0 < AppendReadBuffer());

        // Return the remaining buffer data.
        return _buffer.AsSpan(0, _readLength).ToArray();

        // Reads the buffer data from the stream.
        int ReadByteBuffer()
        {
            // Reset the position to 0 to read from the beginning of the buffer.
            _readPosition = 0;

            if (_isFirstLine)
            {
                var offset = Encoding.GetPreamble().Length;
                _ = _stream.Read(_buffer, 0, offset);
                _isFirstLine = false;
            }

            // Read the buffer data.
            _readLength = _stream.Read(_buffer, 0, _buffer.Length);

            // Return the number of bytes read.
            return _readLength;
        }

        // Appends the buffer data from the stream.
        int AppendReadBuffer()
        {
            // If the buffer is full, double the buffer size.
            if (_readPosition == _buffer.Length)
            {
                // same as Array.MaxLength
                const uint arrayMaxLength = 0x7FFFFFC7;

                // Calculate the new capacity. Double the buffer size, but not exceed the maximum array length.
                var newCapacity = (int)Math.Min((uint)_buffer.Length * 2, arrayMaxLength);

                // Create a new buffer with the new capacity.
                var buffer = new byte[newCapacity];

                // Copy the buffer data to the new buffer.
                _buffer.AsSpan().CopyTo(buffer);

                // Update the buffer.
                _buffer = buffer;
            }

            // Read the buffer data.
#if NET48_OR_GREATER
            var read = _stream.Read(_buffer, _readPosition, _buffer.Length - _readPosition);
#else
            var read = _stream.Read(_buffer.AsSpan(_readPosition, _buffer.Length - _readPosition));
#endif

            // Update the read length.
            _readLength += read;

            // Return the number of bytes read.
            return read;
        }
    }

    /// <summary>
    /// Reads a line of bytes from the stream asynchronously.
    /// </summary>
    /// <returns></returns>
    internal Task<byte[]?> ReadLineAsync() 
        => ReadLineAsync(CancellationToken.None).AsTask();

    /// <summary>
    /// Reads a line of bytes from the stream asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal ValueTask<byte[]?> ReadLineAsync(CancellationToken cancellationToken)
    {
        CheckAsyncTaskInProgress();

        var task = ReadLineAsyncInner(cancellationToken);
        _asyncReadTask = task;

        return new ValueTask<byte[]?>(task);
    }

    /// <summary>
    /// Reads a line of bytes from the stream asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<byte[]?> ReadLineAsyncInner(CancellationToken cancellationToken)
    {
        // If we're at the end of the buffer data, read from the stream.
        CheckAsyncTaskInProgress();

        // If we're at the end of the buffer data, read from the stream.
        if (_readPosition == _readLength)
        {
            if (await ReadBufferAsync().ConfigureAwait(false) == 0)
            {
                // If we're already at the end of the stream, return null.
                return null;
            }
        }

        do
        {
            // Span of the buffer data.
            ReadOnlySpan<byte> bufferSpan = _buffer.AsSpan(_readPosition, _readLength - _readPosition);

            // Look for '\r' or \'n'.
            var indexOfNewline = bufferSpan.IndexOfAny((byte)'\r', (byte)'\n');

            // If we found '\r' or '\n', return the line.
            if (0 <= indexOfNewline)
            {
                // Read the line.
                var line = _buffer.AsSpan(0, _readPosition + indexOfNewline).ToArray();

                // Get the matched 'new line' character.
                var matchedChar = bufferSpan[indexOfNewline];

                // Get the index of the matched character.
                var enterIndexOfNewline = indexOfNewline + 1;

                // Shift the read position.
                _readPosition += enterIndexOfNewline;

                // If we found '\r', consume any immediately following '\n'.
                if (matchedChar == '\r')
                {
                    // If we reached the end of the buffer, read the next buffer.
                    if (_readPosition == _readLength)
                    {
                        await ReadBufferAsync();
                    }

                    // If the next character is '\n', consume it.
                    if (_readPosition < _readLength)
                    {
                        if (_buffer[_readPosition] == '\n')
                        {
                            _readPosition++;
                        }
                    }
                }

                // Shift the buffer data.
                var span = _buffer.AsSpan();
                span.Slice(_readPosition).CopyTo(span);

                // Update the read length and position.
                _readLength -= _readPosition;

                // Reset the position to 0 to read from the beginning of the buffer.
                _readPosition = 0;

                // Return the line.
                return line;
            }

            _readPosition = _readLength;
        } while (0 < await AppendReadBufferAsync());

        // Return the remaining buffer data.
        return _buffer.AsSpan(0, _readLength).ToArray();

        // Reads the buffer data from the stream asynchronously.
        async Task<int> ReadBufferAsync()
        {
            // Reset the position to 0 to read from the beginning of the buffer.
            _readPosition = 0;

            if (_isFirstLine)
            {
                var offset = Encoding.GetPreamble().Length;
                _ = await _stream.ReadAsync(_buffer, 0, offset, cancellationToken);
                _isFirstLine = false;
            }

            // Read the buffer data.
            _readLength = await _stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken);

            // Return the number of bytes read.
            return _readLength;
        }

        // Appends the buffer data from the stream asynchronously.
        async Task<int> AppendReadBufferAsync()
        {
            // If the buffer is full, double the buffer size.
            if (_readPosition == _buffer.Length)
            {
                // same as Array.MaxLength
                const uint arrayMaxLength = 0x7FFFFFC7;

                // Calculate the new capacity. Double the buffer size, but not exceed the maximum array length.
                var newCapacity = (int)Math.Min((uint)_buffer.Length * 2, arrayMaxLength);
                var buffer = new byte[newCapacity];

                // Copy the buffer data to the new buffer.
                _buffer.AsSpan().CopyTo(buffer);
                _buffer = buffer;
            }

            // Read the buffer data.
            var read = await _stream.ReadAsync(_buffer,_readPosition, _buffer.Length - _readPosition, cancellationToken);

            // Update the read length.
            _readLength += read;

            // Return the number of bytes read.
            return read;
        }
    }

    /// <summary>
    /// Releases all resources used by the ByteStreamReader.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        try
        {
            _stream.Close();
        }
        catch
        {
            // Ignore any exceptions that might result from closing the stream.
        }
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Releases the unmanaged resources used by the ByteStreamReader asynchronously.
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            await _stream.DisposeAsync();
        }
        catch
        {
            // Ignore any exceptions that might result from closing the stream.
        }
    }
#endif
}