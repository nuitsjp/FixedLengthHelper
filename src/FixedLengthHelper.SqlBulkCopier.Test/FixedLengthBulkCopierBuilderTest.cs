using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace FixedLengthHelper.SqlBulkCopier.Test;

public class FixedLengthBulkCopierBuilderTest
{
    private static readonly string ConnectionString = new SqlConnectionStringBuilder()
    {
        DataSource = @".",
        InitialCatalog = "AdventureWorksLT2022",
        IntegratedSecurity = true,
        TrustServerCertificate = true
    }.ToString();
    
    [Fact]
    public async Task Test1()
    {
        var sqlBulkCopier = FixedLengthBulkCopierBuilder.CreateBuilder("[SalesLT].[SalesOrderDetail2]")
            .AddColumnMapping("SalesOrderID", 0, 10)
            .AddColumnMapping("SalesOrderDetailID", 10, 10)
            .AddColumnMapping("OrderQty", 20, 5)
            .AddColumnMapping("ProductID", 25, 10)
            .AddColumnMapping("UnitPrice", 35, 15)
            .AddColumnMapping("UnitPriceDiscount", 50, 15)
            .AddColumnMapping("ModifiedDate", 85, 20)
            .Build();

        await using SqlConnection sqlConnection = new(ConnectionString);
        await sqlConnection.OpenAsync();
        Stream stream = File.Open("SalesOrderDetail.txt", FileMode.Open);
        await sqlBulkCopier.WriteToServerAsync(sqlConnection, stream, Encoding.UTF8);
    }

    [Fact]
    public void Export()
    {
                // データを取得するクエリ
        string query = $"""
                        SELECT
                            SalesOrderID,
                            SalesOrderDetailID,
                            OrderQty,
                            ProductID,
                            UnitPrice,
                            UnitPriceDiscount,
                            LineTotal,
                            ModifiedDate
                        FROM
                            SalesLT.SalesOrderDetail
                        """;

        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            
            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                // 出力ファイルを開く
                using (StreamWriter writer = new StreamWriter("SalesOrderDetail.txt"))
                {
                    while (reader.Read())
                    {
                        // 各列のデータを固定長で書き込む
                        string salesOrderID = reader["SalesOrderID"].ToString().PadRight(10);         // 10文字にパディング
                        string salesOrderDetailID = reader["SalesOrderDetailID"].ToString().PadRight(10); // 10文字にパディング
                        string orderQty = reader["OrderQty"].ToString().PadRight(5);                // 5文字にパディング
                        string productID = reader["ProductID"].ToString().PadRight(10);             // 10文字にパディング
                        string unitPrice = reader["UnitPrice"].ToString().PadRight(15);             // 15文字にパディング
                        string unitPriceDiscount = reader["UnitPriceDiscount"].ToString().PadRight(15); // 15文字にパディング
                        string lineTotal = reader["LineTotal"].ToString().PadRight(20);             // 20文字にパディング
                        string modifiedDate = reader["ModifiedDate"].ToString().PadRight(20);       // 20文字にパディング

                        // ファイルに書き込む
                        writer.WriteLine($"{salesOrderID}{salesOrderDetailID}{orderQty}{productID}{unitPrice}{unitPriceDiscount}{lineTotal}{modifiedDate}");
                    }
                }
            }
        }
    }

    [Fact]
    public async Task Foo()
    {
        await using SqlConnection sqlConnection = new(ConnectionString);
        await sqlConnection.OpenAsync();
        SalesOrderDetail[] details =
        [
            new() { SalesOrderID = 71774 },
            new() { SalesOrderID = 71776 }
        ];
        await BulkInsertAsync(sqlConnection, details);
    }

    public static async ValueTask<int> BulkInsertAsync<T>(SqlConnection connection, IEnumerable<T> data, SqlBulkCopyOptions options = default, int? timeout = null, CancellationToken cancellationToken = default)
    {
        using (var executor = new SqlBulkCopy(connection, options, null))
        {
            // テーブル名と型名が一致しているとする
            executor.DestinationTableName = "SalesLT.SalesOrderDetail";

            // データを流し込む
            using (var reader = new SqlBulkCopyDataReader<T>(data))
                await executor.WriteToServerAsync(reader, cancellationToken);

            // 影響した行数 (= 流し込んだ件数) を返すのが一般的
            return executor.RowsCopied;
        }
    }

    public class SalesOrderDetail
    {
        public int SalesOrderID { get; set; }
    }
}