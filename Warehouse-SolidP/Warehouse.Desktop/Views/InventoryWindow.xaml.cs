using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Desktop.ViewModels;

namespace Warehouse.Desktop.Views;

public partial class InventoryWindow : Window
{
    private readonly InventoryViewModel _viewModel;

    public InventoryWindow()
    {
        InitializeComponent();
        _viewModel = App.Services.GetRequiredService<InventoryViewModel>();
        DataContext = _viewModel;

        Loaded += async (_, _) => await _viewModel.LoadItemsCommand.ExecuteAsync(null);
    }
}
