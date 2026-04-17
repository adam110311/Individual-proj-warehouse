using MySqlConnector;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Repositories;

public class ItemRepository
{
    public async Task<List<Item>> GetAllAsync()
    {
        await using var conn = DatabaseService.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            @"SELECT i.I_ID, i.C_ID, i.SerialNumber, i.Name, i.Brand, 
                     i.Description, i.Status, i.DateRegistered, c.Name AS CategoryName
              FROM Item i
              INNER JOIN Category c ON i.C_ID = c.C_ID
              ORDER BY i.I_ID DESC",
            conn);

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

    public async Task<int> CreateAsync(Item item)
    {
        await using var conn = DatabaseService.GetConnection();
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
}
