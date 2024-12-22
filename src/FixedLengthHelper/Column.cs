using System.Data;

namespace FixedLengthHelper;

/// <summary>
/// Column definition.
/// </summary>
public class Column
{
    private readonly Func<string, object> _convert;
    /// <summary>
    /// Column definition.
    /// </summary>
    public Column(
        int ordinal,
        string name,
        int offsetBytes,
        int lengthBytes,
        SqlDbType? sqlDbType,
        TrimMode trimMode,
        char[]? trimChars,
        bool emptyIsNull,
        Func<string, object> convert)
    {
        Ordinal = ordinal;
        Name = name;
        OffsetBytes = offsetBytes;
        LengthBytes = lengthBytes;
        _convert = convert;
    }
    public int Ordinal { get; }
    public string Name { get; }
    public int OffsetBytes { get; }
    public int LengthBytes { get; }

    public object Convert(string value) => _convert(value);
}