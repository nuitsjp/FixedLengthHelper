using System;
using System.Data;

namespace FixedLengthHelper;

public class ColumnOptions(int ordinal, string name, int offsetBytes, int lengthBytes)
{
    private SqlDbType? _sqlDbType;
    private TrimMode? _trimMode;
    private char[]? _trimChars;
    private bool _treatEmptyStringAsNull;
    private Func<string, object>? _converter;

    public ColumnOptions AsType(SqlDbType sqlDbType)
    {
        if (sqlDbType != SqlDbType.Bit)
        {
            throw new ArgumentException($"sqlDbType is only supported for SqlDbType.Bit, not {sqlDbType}.");
        }
        _sqlDbType = sqlDbType;
        return this;
    }

    public ColumnOptions Trim(char[]? trimChars = null)
    {
        _trimMode = TrimMode.Trim;
        _trimChars = trimChars;
        return this;
    }

    public ColumnOptions TrimStart(char[]? trimChars = null)
    {
        _trimMode = TrimMode.TrimStart;
        _trimChars = trimChars;
        return this;
    }

    public ColumnOptions TrimEnd(char[]? trimChars = null)
    {
        _trimMode = TrimMode.TrimEnd;
        _trimChars = trimChars;
        return this;
    }

    public ColumnOptions TreatEmptyStringAsNull()
    {
        _treatEmptyStringAsNull = true;
        return this;
    }

    public ColumnOptions Convert(Func<string, object> convert)
    {
        _converter = convert;
        return this;
    }

    public Column Build()
    {
        return new Column(
            ordinal,
            name,
            offsetBytes,
            lengthBytes,
            _sqlDbType,
            _trimMode ?? TrimMode.None,
            _trimChars,
            _treatEmptyStringAsNull,
            ConvertLocal);

        object ConvertLocal(string s)
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
            if (_converter is not null)
            {
                return _converter(trimValue);
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
}