using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp1.Repositories;

namespace WpfApp1.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly UserRepository _userRepository = new();

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoggingIn;

    // Password can't be bound directly (PasswordBox doesn't support it for security).
    // The View passes it as a parameter to the command.

    /// <summary>
    /// Fired when login succeeds. The View subscribes to this to handle window navigation.
    /// </summary>
    public event Action? LoginSucceeded;

    [RelayCommand]
    private async Task LoginAsync(string password)
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Please enter both email and password.";
            return;
        }

        IsLoggingIn = true;

        try
        {
            var user = await _userRepository.GetByEmailAsync(Email.Trim());

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ErrorMessage = "Invalid email or password.";
                return;
            }

            LoginSucceeded?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Database error: {ex.Message}";
        }
        finally
        {
            IsLoggingIn = false;
        }
    }
}
