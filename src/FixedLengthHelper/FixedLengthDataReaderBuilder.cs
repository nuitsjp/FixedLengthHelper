using System.Text;

namespace FixedLengthHelper;

/// <summary>
/// Builder for FixedLengthDataReader.
/// </summary>
public class FixedLengthDataReaderBuilder
{
    /// <summary>
    /// Columns.
    /// </summary>
    private readonly List<Column> _columns = [];
    
    /// <summary>
    /// Columns.
    /// </summary>
    public IReadOnlyList<Column> Columns => _columns;

    /// <summary>
    /// Builds a FixedLengthDataReader.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public FixedLengthDataReader Build(Stream stream, Encoding encoding)
    {
        return new FixedLengthDataReader(
            new FixedLengthReader(new ByteStreamReader(stream), encoding),
            new FixedLengthDataReaderConfig(_columns));
    }

    /// <summary>
    /// Adds a column.
    /// </summary>
    /// <param name="offsetBytes"></param>
    /// <param name="lengthBytes"></param>
    /// <returns></returns>
    public FixedLengthDataReaderBuilder AddColumn(int offsetBytes, int lengthBytes)
        => AddColumn(offsetBytes, lengthBytes, TrimMode.None);

    /// <summary>
    /// Adds a column.
    /// </summary>
    /// <param name="offsetBytes"></param>
    /// <param name="lengthBytes"></param>
    /// <param name="isDbNull"></param>
    /// <returns></returns>
    public FixedLengthDataReaderBuilder AddColumn(int offsetBytes, int lengthBytes, Func<string, bool> isDbNull)
        => AddColumn(offsetBytes, lengthBytes, TrimMode.None, null, isDbNull);


    /// <summary>
    /// Adds a column.
    /// </summary>
    /// <param name="offsetBytes"></param>
    /// <param name="lengthBytes"></param>
    /// <param name="trimMode"></param>
    /// <param name="trimChars"></param>
    /// <param name="isEmptyNull"></param>
    /// <returns></returns>
    public FixedLengthDataReaderBuilder AddColumn(int offsetBytes, int lengthBytes, TrimMode trimMode, char[]? trimChars = null, bool isEmptyNull = false)
        => AddColumn(offsetBytes, lengthBytes, trimMode, trimChars, s => isEmptyNull && string.IsNullOrEmpty(s));

    /// <summary>
    /// Adds a column.
    /// </summary>
    /// <param name="offsetBytes"></param>
    /// <param name="lengthBytes"></param>
    /// <param name="trimMode"></param>
    /// <param name="trimChars"></param>
    /// <param name="isDbNull"></param>
    /// <returns></returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public FixedLengthDataReaderBuilder AddColumn(int offsetBytes, int lengthBytes, TrimMode trimMode, char[]? trimChars, Func<string, bool> isDbNull)
        => AddColumn(null, offsetBytes, lengthBytes, trimMode, trimChars, isDbNull);

    /// <summary>
    /// Adds a column.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="offsetBytes"></param>
    /// <param name="lengthBytes"></param>
    /// <returns></returns>
    public FixedLengthDataReaderBuilder AddColumn(string? name, int offsetBytes, int lengthBytes)
        => AddColumn(name, offsetBytes, lengthBytes, TrimMode.None);

    /// <summary>
    /// Adds a column.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="offsetBytes"></param>
    /// <param name="lengthBytes"></param>
    /// <param name="isDbNull"></param>
    /// <returns></returns>
    public FixedLengthDataReaderBuilder AddColumn(string? name, int offsetBytes, int lengthBytes, Func<string, bool> isDbNull)
        => AddColumn(name, offsetBytes, lengthBytes, TrimMode.None, null, isDbNull);


    /// <summary>
    /// Adds a column.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="offsetBytes"></param>
    /// <param name="lengthBytes"></param>
    /// <param name="trimMode"></param>
    /// <param name="trimChars"></param>
    /// <param name="isEmptyNull"></param>
    /// <returns></returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public FixedLengthDataReaderBuilder AddColumn(string? name, int offsetBytes, int lengthBytes, TrimMode trimMode, char[]? trimChars = null, bool isEmptyNull = false)
        => AddColumn(name, offsetBytes, lengthBytes, trimMode, trimChars, s => isEmptyNull && string.IsNullOrEmpty(s));

    /// <summary>
    /// Adds a column.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="offsetBytes"></param>
    /// <param name="lengthBytes"></param>
    /// <param name="trimMode"></param>
    /// <param name="trimChars"></param>
    /// <param name="isDbNull"></param>
    /// <returns></returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public FixedLengthDataReaderBuilder AddColumn(string? name, int offsetBytes, int lengthBytes, TrimMode trimMode, char[]? trimChars, Func<string, bool> isDbNull)
    {
        _columns.Add(new Column(_columns.Count, name, offsetBytes, lengthBytes, trimMode, trimChars, isDbNull));
        return this;
    }
}