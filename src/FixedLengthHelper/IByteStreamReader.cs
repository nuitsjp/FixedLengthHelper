namespace FixedLengthHelper;

#if NET48_OR_GREATER
public interface IByteStreamReader : IDisposable
#else
public interface IByteStreamReader : IDisposable, IAsyncDisposable
#endif
{
    /// <summary>
    /// Reads a line of bytes from the stream.
    /// </summary>
    /// <returns>
    /// Read bytes. If the end of the stream has been reached, returns null.
    /// </returns>
    byte[]? ReadLine();

    /// <summary>
    /// Reads a line of bytes from the stream asynchronously.
    /// </summary>
    /// <returns></returns>
    Task<byte[]?> ReadLineAsync();

    /// <summary>
    /// Reads a line of bytes from the stream asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<byte[]?> ReadLineAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Closes the ByteStreamReader.
    /// </summary>
    void Close();
}