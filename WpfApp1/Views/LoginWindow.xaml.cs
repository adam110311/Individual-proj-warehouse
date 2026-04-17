using System.Windows;
using WpfApp1.ViewModels;

namespace WpfApp1.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        DataContext = _viewModel;

        _viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        // PasswordBox can't be bound for security reasons, so we pass the value manually
        _viewModel.LoginCommand.Execute(PasswordBox.Password);
    }

    private void OnLoginSucceeded()
    {
        var inventoryWindow = new InventoryWindow();
        inventoryWindow.Show();
        Close();
    }
}
