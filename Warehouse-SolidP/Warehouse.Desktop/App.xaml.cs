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
    /// <summary>
    /// The application-wide DI container. Used by Views to resolve ViewModels.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Data layer — singleton because it reads config once
        services.AddSingleton<DatabaseService>();

        // Repositories — transient so each usage gets a fresh instance
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<ICategoryRepository, CategoryRepository>();
        services.AddTransient<IItemRepository, ItemRepository>();

        // ViewModels — transient so each window gets its own instance
        services.AddTransient<LoginViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<ItemFormViewModel>();

        Services = services.BuildServiceProvider();

        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }
}
