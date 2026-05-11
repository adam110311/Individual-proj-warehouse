using System.IO;
using System.Text.Json;
using MySqlConnector;

namespace Warehouse.Data.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        if (!File.Exists(path))
            throw new FileNotFoundException(
                "appsettings.json not found. Make sure it is set to 'Copy if newer' in project properties.");

        var json = File.ReadAllText(path);
        var doc = JsonDocument.Parse(json);
        _connectionString = doc.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("DefaultConnection")
            .GetString()
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
    }

    public MySqlConnection GetConnection() => new(_connectionString);
}
