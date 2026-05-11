using MySqlConnector;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Data.Services;

namespace Warehouse.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DatabaseService _db;

    public UserRepository(DatabaseService db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            "SELECT U_ID, Email, Password, FirstName, LastName, Role FROM User WHERE Email = @Email",
            conn);

        cmd.Parameters.AddWithValue("@Email", email);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            U_ID = reader.GetInt32("U_ID"),
            Email = reader.GetString("Email"),
            Password = reader.GetString("Password"),
            FirstName = reader.GetString("FirstName"),
            LastName = reader.GetString("LastName"),
            Role = reader.GetString("Role")
        };
    }
}
