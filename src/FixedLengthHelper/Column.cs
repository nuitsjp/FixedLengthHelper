using System;
using System.Data;

namespace FixedLengthHelper;

/// <summary>
/// Column definition.
/// </summary>
public class Column
{
    private readonly SqlDbType? _sqlDbType;
    private readonly TrimMode? _trimMode;
    private readonly char[]? _trimChars;
    private readonly bool _treatEmptyStringAsNull;
    private readonly Func<string, object>? _convert;
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
        bool treatEmptyStringAsNull,
        Func<string, object> convert)
    {
        Ordinal = ordinal;
        Name = name;
        OffsetBytes = offsetBytes;
        LengthBytes = lengthBytes;
        _convert = convert;
        _sqlDbType = sqlDbType;
        _trimMode = trimMode;
        _trimChars = trimChars;
        _treatEmptyStringAsNull = treatEmptyStringAsNull;
    }
    public int Ordinal { get; }
    public string Name { get; }
    public int OffsetBytes { get; }
    public int LengthBytes { get; }

    public object Convert(string s)
    {
        var trimValue = _trimMode switch
        {
            null => s,
            TrimMode.None => s,
            TrimMode.Trim => s.Trim(_trimChars),
            TrimMode.TrimStart => s.TrimStart(_trimChars),
            TrimMode.TrimEnd => s.TrimEnd(_trimChars),
            _ => throw new ArgumentOutOfRangeException()
        };
        if (_convert is not null)
        {
            return _convert(trimValue);
        }

        if (_sqlDbType == SqlDbType.Bit)
        {
            return trimValue == "1";
        }

        if (_treatEmptyStringAsNull && string.IsNullOrEmpty(trimValue))
        {
            return DBNull.Value;
        }

        return trimValue;

    }
}