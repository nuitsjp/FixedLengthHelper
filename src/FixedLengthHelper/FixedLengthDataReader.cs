using System.Data;

namespace FixedLengthHelper;

public class FixedLengthDataReader 
    : IDataReader
#if NET8_0_OR_GREATER
        , IAsyncDisposable
#endif
{
    private readonly FixedLengthReader _fixedLengthReader;
    private readonly IReadOnlyDictionary<string, int> _columnOrdinals;
    private readonly IReadOnlyList<Column> _columns;

    public FixedLengthDataReader(
        FixedLengthReader fixedLengthReader,
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

    public void Dispose()
    {
        _fixedLengthReader.Dispose();
    }


#if NET8_0_OR_GREATER
    public async ValueTask DisposeAsync()
    {
        await _fixedLengthReader.DisposeAsync();
    }
#endif


    public int FieldCount => _columns.Count;
    public int Depth => 0;
    public bool IsClosed => _fixedLengthReader.IsClosed;
    public int RecordsAffected => 0;
    public object this[int i] => GetValue(i);

    public object this[string name] => GetValue(GetOrdinal(name));
    public int GetOrdinal(string name)
    {
        return _columnOrdinals[name];
    }

    public object GetValue(int i)
    {
        var column = _columns[i];
        return _fixedLengthReader.GetField(column.OffsetBytes, column.LengthBytes);
    }

    #region NotSupported
    public string GetName(int i)
    {
        throw new NotSupportedException();
    }

    public string GetDataTypeName(int i)
    {
        throw new NotSupportedException();
    }

    public Type GetFieldType(int i)
    {
        throw new NotSupportedException();
    }


    public int GetValues(object[] values)
    {
        throw new NotSupportedException();
    }

    public bool GetBoolean(int i)
    {
        throw new NotSupportedException();
    }

    public byte GetByte(int i)
    {
        throw new NotSupportedException();
    }

    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
    {
        throw new NotSupportedException();
    }

    public char GetChar(int i)
    {
        throw new NotSupportedException();
    }

    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
    {
        throw new NotSupportedException();
    }

    public Guid GetGuid(int i)
    {
        throw new NotSupportedException();
    }

    public short GetInt16(int i)
    {
        throw new NotSupportedException();
    }

    public int GetInt32(int i)
    {
        throw new NotSupportedException();
    }

    public long GetInt64(int i)
    {
        throw new NotSupportedException();
    }

    public float GetFloat(int i)
    {
        throw new NotSupportedException();
    }

    public double GetDouble(int i)
    {
        throw new NotSupportedException();
    }

    public string GetString(int i)
    {
        throw new NotSupportedException();
    }

    public decimal GetDecimal(int i)
    {
        throw new NotSupportedException();
    }

    public DateTime GetDateTime(int i)
    {
        throw new NotSupportedException();
    }

    public IDataReader? GetData(int i)
    {
        throw new NotSupportedException();
    }

    public bool IsDBNull(int i)
    {
        throw new NotSupportedException();
    }

    public void Close()
    {
        throw new NotSupportedException();
    }

    public DataTable? GetSchemaTable()
    {
        throw new NotSupportedException();
    }

    public bool NextResult()
    {
        throw new NotSupportedException();
    }

    public bool Read()
        => _fixedLengthReader.Read();
    #endregion
}