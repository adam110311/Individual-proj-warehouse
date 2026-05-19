using MySqlConnector;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Data.Services;

namespace Warehouse.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly DatabaseService _db;

    public ItemRepository(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<Item>> GetAllAsync()
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            @"SELECT i.I_ID, i.C_ID, i.SerialNumber, i.Name, i.Brand, 
                     i.Description, i.Status, i.DateRegistered, c.Name AS CategoryName
              FROM Item i
              INNER JOIN Category c ON i.C_ID = c.C_ID
              ORDER BY i.I_ID DESC",
            conn);

        return await ReadItemsAsync(cmd);
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            @"SELECT i.I_ID, i.C_ID, i.SerialNumber, i.Name, i.Brand, 
                     i.Description, i.Status, i.DateRegistered, c.Name AS CategoryName
              FROM Item i
              INNER JOIN Category c ON i.C_ID = c.C_ID
              WHERE i.I_ID = @Id",
            conn);

        cmd.Parameters.AddWithValue("@Id", id);
        var items = await ReadItemsAsync(cmd);
        return items.FirstOrDefault();
    }

    public async Task<List<Item>> SearchAsync(string? searchTerm, int? categoryId, string? status)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var sql = @"SELECT i.I_ID, i.C_ID, i.SerialNumber, i.Name, i.Brand, 
                           i.Description, i.Status, i.DateRegistered, c.Name AS CategoryName
                    FROM Item i
                    INNER JOIN Category c ON i.C_ID = c.C_ID
                    WHERE 1=1";

        var parameters = new List<MySqlParameter>();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            sql += " AND (i.Name LIKE @Search OR i.SerialNumber LIKE @Search OR i.Brand LIKE @Search)";
            parameters.Add(new MySqlParameter("@Search", $"%{searchTerm.Trim()}%"));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            sql += " AND i.C_ID = @CategoryId";
            parameters.Add(new MySqlParameter("@CategoryId", categoryId.Value));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            sql += " AND i.Status = @Status";
            parameters.Add(new MySqlParameter("@Status", status));
        }

        sql += " ORDER BY i.I_ID DESC";

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters.ToArray());

        return await ReadItemsAsync(cmd);
    }

    public async Task<int> CreateAsync(Item item)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            @"INSERT INTO Item (C_ID, SerialNumber, Name, Brand, Description, Status)
              VALUES (@C_ID, @SerialNumber, @Name, @Brand, @Description, @Status);
              SELECT LAST_INSERT_ID();",
            conn);

        cmd.Parameters.AddWithValue("@C_ID", item.C_ID);
        cmd.Parameters.AddWithValue("@SerialNumber", item.SerialNumber);
        cmd.Parameters.AddWithValue("@Name", item.Name);
        cmd.Parameters.AddWithValue("@Brand", item.Brand);
        cmd.Parameters.AddWithValue("@Description", (object?)item.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Status", item.Status);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task UpdateAsync(Item item)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            @"UPDATE Item 
              SET C_ID = @C_ID, SerialNumber = @SerialNumber, Name = @Name, 
                  Brand = @Brand, Description = @Description, Status = @Status
              WHERE I_ID = @I_ID",
            conn);

        cmd.Parameters.AddWithValue("@I_ID", item.I_ID);
        cmd.Parameters.AddWithValue("@C_ID", item.C_ID);
        cmd.Parameters.AddWithValue("@SerialNumber", item.SerialNumber);
        cmd.Parameters.AddWithValue("@Name", item.Name);
        cmd.Parameters.AddWithValue("@Brand", item.Brand);
        cmd.Parameters.AddWithValue("@Description", (object?)item.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Status", item.Status);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        // Remove from any order associations first
        await using var unlinkCmd = new MySqlCommand(
            "DELETE FROM OrderItem WHERE I_ID = @Id", conn);
        unlinkCmd.Parameters.AddWithValue("@Id", id);
        await unlinkCmd.ExecuteNonQueryAsync();

        await using var cmd = new MySqlCommand(
            "DELETE FROM Item WHERE I_ID = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateStatusAsync(int itemId, string newStatus)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            "UPDATE Item SET Status = @Status WHERE I_ID = @Id", conn);

        cmd.Parameters.AddWithValue("@Status", newStatus);
        cmd.Parameters.AddWithValue("@Id", itemId);

        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<List<Item>> ReadItemsAsync(MySqlCommand cmd)
    {
        await using var reader = await cmd.ExecuteReaderAsync();
        var items = new List<Item>();

        while (await reader.ReadAsync())
        {
            items.Add(new Item
            {
                I_ID = reader.GetInt32("I_ID"),
                C_ID = reader.GetInt32("C_ID"),
                SerialNumber = reader.GetString("SerialNumber"),
                Name = reader.GetString("Name"),
                Brand = reader.GetString("Brand"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? null
                    : reader.GetString("Description"),
                Status = reader.GetString("Status"),
                DateRegistered = reader.GetDateTime("DateRegistered"),
                CategoryName = reader.GetString("CategoryName")
            });
        }

        return items;
    }
}
