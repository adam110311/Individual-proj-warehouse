using System.Windows;
using System.Windows.Input;
using Warehouse.Desktop.ViewModels;

namespace Warehouse.Desktop.Views;

public partial class OrderListWindow : Window
{
    public OrderListWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is OrderListViewModel vm)
                await vm.LoadOrdersCommand.ExecuteAsync(null);
        };
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is OrderListViewModel vm)
            vm.ViewOrderDetailCommand.Execute(null);
    }
}
