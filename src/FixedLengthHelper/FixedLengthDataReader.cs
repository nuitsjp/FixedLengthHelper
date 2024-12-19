using System.Data;

namespace FixedLengthHelper;

public class FixedLengthDataReader 
    : IDataReader
#if NET8_0_OR_GREATER
        , IAsyncDisposable
#endif
{
    private readonly IFixedLengthReader _fixedLengthReader;
    private readonly IReadOnlyDictionary<string, int> _columnOrdinals;
    private readonly IReadOnlyList<Column> _columns;
    private bool _isDisposed;

    public FixedLengthDataReader(
        IFixedLengthReader fixedLengthReader,
        FixedLengthDataReaderConfig config)
    {
        _fixedLengthReader = fixedLengthReader;
        _columnOrdinals = config
            .Columns
            .Where(x => x.Name is not null)
            .ToDictionary(x => x.Name!, column => column.Ordinal);
        _columns = config.Columns;
    }

    public static FixedLengthDataReaderBuilder CreateBuilder() => new();

    /// <inheritdoc />
    public int FieldCount => _columns.Count;

    /// <inheritdoc />
    public int Depth => 0;

    /// <inheritdoc />
    public bool IsClosed => _fixedLengthReader.IsClosed;

    /// <inheritdoc />
    public int RecordsAffected => 0;

    /// <inheritdoc />
    public object this[int i] => GetValue(i);

    /// <inheritdoc />
    public object this[string name] => GetValue(GetOrdinal(name));

    /// <inheritdoc />
    public int GetOrdinal(string name)
    {
        if (_columnOrdinals.TryGetValue(name, out var ordinal))
        {
            return ordinal;
        }
        throw new IndexOutOfRangeException($"Field with name '{name}' was not found.");
    }

    /// <inheritdoc />
    public object GetValue(int i)
    {
        if (i < _columns.Count)
        {
            var column = _columns[i];
            var value = column.TrimChars is null 
                ? _fixedLengthReader.GetField(column.OffsetBytes, column.LengthBytes, column.TrimMode) 
                : _fixedLengthReader.GetField(column.OffsetBytes, column.LengthBytes, column.TrimMode, column.TrimChars);
            return column.IsDBNull(value)
                ? DBNull.Value
                : value;
        }

        throw new IndexOutOfRangeException($"Field with ordinal '{i}' was not found.");
    }

    /// <inheritdoc />
    public void Close()
    {
        Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;

        _fixedLengthReader.Dispose();
    }

#if NET8_0_OR_GREATER
    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;

        await _fixedLengthReader.DisposeAsync();
    }
#endif

    #region NotSupported
    /// <inheritdoc />
    public string GetName(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public string GetDataTypeName(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public Type GetFieldType(int i) => typeof(string);

    /// <inheritdoc />
    public int GetValues(object[] values)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public bool GetBoolean(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public byte GetByte(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public char GetChar(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public Guid GetGuid(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public short GetInt16(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public int GetInt32(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public long GetInt64(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public float GetFloat(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public double GetDouble(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public string GetString(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public decimal GetDecimal(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public DateTime GetDateTime(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public IDataReader GetData(int i)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public bool IsDBNull(int i)
    {
        if (i < _columns.Count)
        {
            var column = _columns[i];
            var value = column.TrimChars is null
                ? _fixedLengthReader.GetField(column.OffsetBytes, column.LengthBytes, column.TrimMode)
                : _fixedLengthReader.GetField(column.OffsetBytes, column.LengthBytes, column.TrimMode, column.TrimChars);
            return column.IsDBNull(value);
        }

        throw new IndexOutOfRangeException($"Field with ordinal '{i}' was not found.");
    }

    /// <inheritdoc />
    public DataTable GetSchemaTable()
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public bool NextResult()
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public bool Read()
        => _fixedLengthReader.Read();
    #endregion
}