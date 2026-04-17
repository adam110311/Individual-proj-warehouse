using MySqlConnector;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Repositories;

public class UserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var conn = DatabaseService.GetConnection();
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
