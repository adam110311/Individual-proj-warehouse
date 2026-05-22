using MySqlConnector;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Data.Services;

namespace Warehouse.Data.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly DatabaseService _db;

    public AnalyticsRepository(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<CategoryDispatchRaw>> GetDispatchCountsByCategoryAsync()
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            @"SELECT c.Name AS CategoryName, COUNT(*) AS ItemCount
              FROM OrderItem oi
              INNER JOIN Item i ON oi.I_ID = i.I_ID
              INNER JOIN Category c ON i.C_ID = c.C_ID
              INNER JOIN `Order` o ON oi.O_ID = o.O_ID
              WHERE o.OrderType = 'Outbound' AND o.Status = 'Completed'
              GROUP BY c.Name
              ORDER BY ItemCount DESC",
            conn);

        await using var reader = await cmd.ExecuteReaderAsync();
        var results = new List<CategoryDispatchRaw>();

        while (await reader.ReadAsync())
        {
            results.Add(new CategoryDispatchRaw
            {
                CategoryName = reader.GetString("CategoryName"),
                ItemCount = reader.GetInt32("ItemCount")
            });
        }

        return results;
    }

    public async Task<int> GetCompletedOutboundOrderCountAsync()
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            "SELECT COUNT(*) FROM `Order` WHERE OrderType = 'Outbound' AND Status = 'Completed'",
            conn);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<DateTime?> GetEarliestCompletedOutboundDateAsync()
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            "SELECT MIN(CompletedDate) FROM `Order` WHERE OrderType = 'Outbound' AND Status = 'Completed'",
            conn);

        var result = await cmd.ExecuteScalarAsync();
        return result == DBNull.Value || result == null ? null : (DateTime)result;
    }

    public async Task<List<CategoryStockCount>> GetInStockCountsByCategoryAsync()
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            @"SELECT c.Name AS CategoryName, COUNT(*) AS InStockCount
              FROM Item i
              INNER JOIN Category c ON i.C_ID = c.C_ID
              WHERE i.Status = 'In Stock'
              GROUP BY c.Name
              ORDER BY InStockCount DESC",
            conn);

        await using var reader = await cmd.ExecuteReaderAsync();
        var results = new List<CategoryStockCount>();

        while (await reader.ReadAsync())
        {
            results.Add(new CategoryStockCount
            {
                CategoryName = reader.GetString("CategoryName"),
                InStockCount = reader.GetInt32("InStockCount")
            });
        }

        return results;
    }
}
