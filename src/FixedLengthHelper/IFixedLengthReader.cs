namespace FixedLengthHelper;

/// <summary>
/// Fixed-length reader.
/// </summary>
#if NET48_OR_GREATER
public interface IFixedLengthReader : IDisposable
#else
public interface IFixedLengthReader : IDisposable, IAsyncDisposable
#endif
{
    /// <summary>
    /// Gets a value indicating whether the stream is closed.
    /// </summary>
    bool IsClosed { get; }
    
    /// <summary>
    /// Reads a line of bytes from the stream.
    /// </summary>
    /// <returns></returns>
    bool Read();
    
    /// <summary>
    /// Reads a line of bytes from the stream asynchronously.
    /// </summary>
    /// <returns></returns>
    Task<bool> ReadAsync();
    
    /// <summary>
    /// Gets a field from the current line.
    /// </summary>
    /// <param name="offsetBytes"></param>
    /// <param name="lengthBytes"></param>
    /// <returns></returns>
    string GetField(int offsetBytes, int lengthBytes);
}