using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Warehouse.Core.Interfaces;

namespace Warehouse.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IUserRepository _userRepository;

    public LoginViewModel(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoggingIn;

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
