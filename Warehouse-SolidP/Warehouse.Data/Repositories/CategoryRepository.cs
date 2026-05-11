using MySqlConnector;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Data.Services;

namespace Warehouse.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly DatabaseService _db;

    public CategoryRepository(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand("SELECT C_ID, Name FROM Category ORDER BY Name", conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var categories = new List<Category>();

        while (await reader.ReadAsync())
        {
            categories.Add(new Category
            {
                C_ID = reader.GetInt32("C_ID"),
                Name = reader.GetString("Name")
            });
        }

        return categories;
    }
}
