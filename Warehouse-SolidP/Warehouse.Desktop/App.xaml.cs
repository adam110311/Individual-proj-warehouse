using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Core.Interfaces;
using Warehouse.Data.Repositories;
using Warehouse.Data.Services;
using Warehouse.Desktop.ViewModels;
using Warehouse.Desktop.Views;

namespace Warehouse.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var connectionString = ReadConnectionString();

        var services = new ServiceCollection();

        services.AddSingleton(new DatabaseService(connectionString));

        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<ICategoryRepository, CategoryRepository>();
        services.AddTransient<IItemRepository, ItemRepository>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<ItemFormViewModel>();

        Services = services.BuildServiceProvider();

        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }

    private static string ReadConnectionString()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        if (!File.Exists(path))
            throw new FileNotFoundException(
                "appsettings.json not found. Make sure it is set to 'Copy if newer' in project properties.");

        var json = File.ReadAllText(path);
        var doc = JsonDocument.Parse(json);

        return doc.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("DefaultConnection")
            .GetString()
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
    }
}
