using System;
using System.Data;

namespace FixedLengthHelper;

public class ColumnOptions(int ordinal, string name, int offsetBytes, int lengthBytes)
{
    private SqlDbType? SqlDbType { get; set; }
    private TrimMode? TrimMode { get; set; }
    private char[]? TrimChars { get; set; }
    private bool EmptyIsNull { get; set; }
    private Func<string, object>? Converter { get; set; }

    public ColumnOptions AsType(SqlDbType sqlDbType)
    {
        if (sqlDbType != System.Data.SqlDbType.Bit)
        {
            throw new ArgumentException($"sqlDbType is only supported for SqlDbType.Bit, not {sqlDbType}.");
        }
        SqlDbType = sqlDbType;
        return this;
    }

    public ColumnOptions Trim(char[]? trimChars = null, bool emptyIsNull = false)
    {
        TrimMode = FixedLengthHelper.TrimMode.Trim;
        TrimChars = trimChars;
        EmptyIsNull = emptyIsNull;
        return this;
    }

    public ColumnOptions TrimStart(char[]? trimChars = null, bool emptyIsNull = false)
    {
        TrimMode = FixedLengthHelper.TrimMode.TrimStart;
        TrimChars = trimChars;
        EmptyIsNull = emptyIsNull;
        return this;
    }

    public ColumnOptions TrimEnd(char[]? trimChars = null, bool emptyIsNull = false)
    {
        TrimMode = FixedLengthHelper.TrimMode.TrimEnd;
        TrimChars = trimChars;
        EmptyIsNull = emptyIsNull;
        return this;
    }

    public ColumnOptions Convert(Func<string, object> convert)
    {
        Converter = convert;
        return this;
    }

    public Column Build()
    {
        return new Column(
            ordinal,
            name,
            offsetBytes,
            lengthBytes,
            SqlDbType,
            TrimMode ?? FixedLengthHelper.TrimMode.None,
            TrimChars,
            EmptyIsNull,
            ConvertLocal);

        object ConvertLocal(string s)
        {
            var trimValue = TrimMode switch
            {
                null => s,
                FixedLengthHelper.TrimMode.None => s,
                FixedLengthHelper.TrimMode.Trim => s.Trim(TrimChars),
                FixedLengthHelper.TrimMode.TrimStart => s.TrimStart(TrimChars),
                FixedLengthHelper.TrimMode.TrimEnd => s.TrimEnd(TrimChars),
                _ => throw new ArgumentOutOfRangeException()
            };
            if (Converter is not null)
            {
                return Converter(trimValue);
            }

            if (SqlDbType == System.Data.SqlDbType.Bit)
            {
                return trimValue == "1";
            }

            if (EmptyIsNull && string.IsNullOrEmpty(trimValue))
            {
                return DBNull.Value;
            }

            return trimValue;
        }
    }
}