namespace FixedLengthHelper;

#if NET48_OR_GREATER
public interface IFixedLengthReader : IDisposable
#else
public interface IFixedLengthReader : IDisposable, IAsyncDisposable
#endif
{
    bool Read();

    Task<bool> ReadAsync();
    string GetField(int offsetBytes, int lengthBytes, TrimMode trimMode = TrimMode.None, params char[] trimChars);
}