using System.Windows;
using WpfApp1.ViewModels;

namespace WpfApp1.Views;

public partial class InventoryWindow : Window
{
    private readonly InventoryViewModel _viewModel;

    public InventoryWindow()
    {
        InitializeComponent();
        _viewModel = new InventoryViewModel();
        DataContext = _viewModel;

        Loaded += async (_, _) => await _viewModel.LoadItemsCommand.ExecuteAsync(null);
    }
}
