using System.Data;

namespace FixedLengthHelper;

public interface IDataReaderColumn
{
    object Convert(string value);
}

public class DataReaderColumn : IDataReaderColumn
{
    private readonly Func<string, object>? _convert;
    private readonly SqlDbType? _sqlDbType;

    private DataReaderColumn()
    {
        _sqlDbType = null;
        _convert = null;
    }
    private DataReaderColumn(Func<string, object>? convert)
    {
        _sqlDbType = null;
        _convert = convert;
    }

    private DataReaderColumn(SqlDbType? sqlDbType)
    {
        _sqlDbType = sqlDbType;
        _convert = null;
    }

    public object Convert(string value)
    {
        if (_sqlDbType == SqlDbType.Bit)
        {
            return value == "1";
        }

        return value;
    }

    public static IDataReaderColumn Create() => new DataReaderColumn();
    public static IDataReaderColumn Create(SqlDbType sqlDbType) => new DataReaderColumn(sqlDbType);
    public static IDataReaderColumn Create(Func<string, object> convert) => new DataReaderColumn(convert);
}