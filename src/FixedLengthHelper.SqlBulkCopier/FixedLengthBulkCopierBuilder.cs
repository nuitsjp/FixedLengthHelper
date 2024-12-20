using System.Text;
using Microsoft.Data.SqlClient;

namespace FixedLengthHelper.SqlBulkCopier;

public interface ISqlBulkCopier
{
    Task WriteToServerAsync(SqlConnection connection, Stream stream, Encoding encoding);
}

public class SqlBulkCopier(
    FixedLengthDataReaderBuilder fixedLengthDataReaderBuilder,
    Dictionary<string, Column> columns) : ISqlBulkCopier
{
    public async Task WriteToServerAsync(SqlConnection connection, Stream stream, Encoding encoding)
    {
        using var sqlBulkCopy = new SqlBulkCopy(connection);
        foreach (var column in columns)
        {
            sqlBulkCopy.ColumnMappings.Add(column.Key, column.Value.Ordinal);
        }

        await using var dataReader = fixedLengthDataReaderBuilder.Build(stream, encoding);
        await sqlBulkCopy.WriteToServerAsync(dataReader);
    }
}
public class FixedLengthBulkCopierBuilder
{
    public static FixedLengthBulkCopierBuilder CreateBuilder(string tableName) => new();
    
    private readonly FixedLengthDataReaderBuilder _fixedLengthDataReaderBuilder = new();
    
    private readonly Dictionary<string, Column> _columns = new();
    
    public FixedLengthBulkCopierBuilder AddColumnMapping(string columnName, Action<FixedLengthDataReaderBuilder> addColumn)
    {
        addColumn(_fixedLengthDataReaderBuilder);
        var latestColumn = _fixedLengthDataReaderBuilder.Columns[^1];
        _columns[columnName] = latestColumn with { Name = columnName };;
        return this;
    }
    
    public ISqlBulkCopier Build()
    {
        return new SqlBulkCopier(_fixedLengthDataReaderBuilder, _columns);
    }

}