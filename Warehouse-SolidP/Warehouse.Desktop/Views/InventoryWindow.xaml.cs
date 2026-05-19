using System.Windows;
using System.Windows.Input;
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

        Loaded += async (_, _) => await _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        _viewModel.ViewItemDetailCommand.Execute(null);
    }
}
