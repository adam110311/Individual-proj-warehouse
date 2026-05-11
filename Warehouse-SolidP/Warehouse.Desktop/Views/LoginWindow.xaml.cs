using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Desktop.ViewModels;

namespace Warehouse.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow()
    {
        InitializeComponent();
        _viewModel = App.Services.GetRequiredService<LoginViewModel>();
        DataContext = _viewModel;

        _viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.LoginCommand.Execute(PasswordBox.Password);
    }

    private void OnLoginSucceeded()
    {
        var inventoryWindow = new InventoryWindow();
        inventoryWindow.Show();
        Close();
    }
}
