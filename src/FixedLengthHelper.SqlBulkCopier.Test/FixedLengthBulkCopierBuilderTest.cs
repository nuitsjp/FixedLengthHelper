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
        var sqlBulkCopier = FixedLengthBulkCopierBuilder.CreateBuilder("[SalesLT].[SalesOrderDetail]")
            .AddColumnMapping("SalesOrderID", builder => builder.AddColumn(0, 10))
            .AddColumnMapping("SalesOrderDetailID", builder => builder.AddColumn(10, 10))
            .AddColumnMapping("OrderQty", builder => builder.AddColumn(20, 5))
            .AddColumnMapping("ProductID", builder => builder.AddColumn(25, 10))
            .AddColumnMapping("UnitPrice", builder => builder.AddColumn(35, 15))
            .AddColumnMapping("UnitPriceDiscount", builder => builder.AddColumn(50, 15))
            .AddColumnMapping("LineTotal", builder => builder.AddColumn(65, 20))
            .AddColumnMapping("ModifiedDate", builder => builder.AddColumn(85, 20))
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
}