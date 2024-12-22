using System.Data;

namespace FixedLengthHelper;

/// <summary>
/// IDataReader implementation for fixed-length files.
/// </summary>
public class FixedLengthDataReader 
    : IDataReader
#if NET8_0_OR_GREATER
        , IAsyncDisposable
#endif
{
    /// <summary>
    /// IFixedLengthReader instance.
    /// </summary>
    private readonly IFixedLengthReader _fixedLengthReader;
    
    /// <summary>
    /// Name and ordinal of columns.
    /// </summary>
    private readonly IReadOnlyDictionary<string, int> _columnOrdinals;
    
    /// <summary>
    /// Columns.
    /// </summary>
    private readonly IReadOnlyList<Column> _columns;
    
    /// <summary>
    /// Disposed flag.
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedLengthDataReader"/> class.
    /// </summary>
    /// <param name="fixedLengthReader"></param>
    /// <param name="config"></param>
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

    /// <summary>
    /// Creates a new instance of the <see cref="FixedLengthDataReaderBuilder"/> class.
    /// </summary>
    /// <returns></returns>
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
    public object this[string name] => throw new NotSupportedException();

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
        if (_columns.Count <= i) throw new IndexOutOfRangeException($"Field with ordinal '{i}' was not found.");
        
        var column = _columns[i];
        var value = _fixedLengthReader.GetField(column.OffsetBytes, column.LengthBytes);
        var trimValue = column.TrimMode switch
        {
            TrimMode.None => value,
            TrimMode.Trim => value.Trim(column.TrimChars),
            TrimMode.TrimStart => value.TrimStart(column.TrimChars),
            TrimMode.TrimEnd => value.TrimEnd(column.TrimChars),
            _ => throw new ArgumentOutOfRangeException()
        };
        return column.IsDBNull(trimValue)
            ? DBNull.Value
            : trimValue;
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
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fixedLengthReader.Dispose();
        }
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
        throw new NotSupportedException();
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