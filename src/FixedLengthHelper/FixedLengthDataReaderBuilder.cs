using System.Text;

namespace FixedLengthHelper;

public class FixedLengthDataReaderBuilder
{
    private readonly List<Column> _columns = [];

    public FixedLengthDataReader Build(Stream stream, Encoding encoding)
    {
        return new FixedLengthDataReader(
            new FixedLengthReader(new ByteStreamReader(stream), encoding),
            new FixedLengthDataReaderConfig(_columns));
    }

    public FixedLengthDataReaderBuilder AddColumn(int offsetBytes, int lengthBytes)
        => AddColumn(offsetBytes, lengthBytes, TrimMode.None);

    public FixedLengthDataReaderBuilder AddColumn(int offsetBytes, int lengthBytes, Func<string, bool> isDbNull)
        => AddColumn(offsetBytes, lengthBytes, TrimMode.None, null, isDbNull);


    public FixedLengthDataReaderBuilder AddColumn(int offsetBytes, int lengthBytes, TrimMode trimMode, char[]? trimChars = null, bool isEmptyNull = false)
        => AddColumn(offsetBytes, lengthBytes, trimMode, trimChars, s => isEmptyNull && string.IsNullOrEmpty(s));

    // ReSharper disable once MemberCanBePrivate.Global
    public FixedLengthDataReaderBuilder AddColumn(int offsetBytes, int lengthBytes, TrimMode trimMode, char[]? trimChars, Func<string, bool> isDbNull)
        => AddColumn(null, offsetBytes, lengthBytes, trimMode, trimChars, isDbNull);

    public FixedLengthDataReaderBuilder AddColumn(string? name, int offsetBytes, int lengthBytes)
        => AddColumn(name, offsetBytes, lengthBytes, TrimMode.None);

    public FixedLengthDataReaderBuilder AddColumn(string? name, int offsetBytes, int lengthBytes, Func<string, bool> isDbNull)
        => AddColumn(name, offsetBytes, lengthBytes, TrimMode.None, null, isDbNull);


    // ReSharper disable once MemberCanBePrivate.Global
    public FixedLengthDataReaderBuilder AddColumn(string? name, int offsetBytes, int lengthBytes, TrimMode trimMode, char[]? trimChars = null, bool isEmptyNull = false)
        => AddColumn(name, offsetBytes, lengthBytes, trimMode, trimChars, s => isEmptyNull && string.IsNullOrEmpty(s));

    // ReSharper disable once MemberCanBePrivate.Global
    public FixedLengthDataReaderBuilder AddColumn(string? name, int offsetBytes, int lengthBytes, TrimMode trimMode, char[]? trimChars, Func<string, bool> isDbNull)
    {
        _columns.Add(new Column(_columns.Count, name, offsetBytes, lengthBytes, trimMode, trimChars, isDbNull));
        return this;
    }
}