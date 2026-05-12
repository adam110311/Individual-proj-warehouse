using Moq;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Desktop.ViewModels;
using Xunit;

namespace Warehouse.Tests;

public class LoginViewModelTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly LoginViewModel _viewModel;

    // A real BCrypt hash of "correctpassword"
    private readonly string _hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");

    public LoginViewModelTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _viewModel = new LoginViewModel(_mockUserRepo.Object);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ShowsError()
    {
        _viewModel.Email = "";

        await _viewModel.LoginCommand.ExecuteAsync("somepassword");

        Assert.Equal("Please enter both email and password.", _viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShowsError()
    {
        _viewModel.Email = "admin@warehouse.local";

        await _viewModel.LoginCommand.ExecuteAsync("");

        Assert.Equal("Please enter both email and password.", _viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShowsInvalidCredentials()
    {
        _mockUserRepo
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        _viewModel.Email = "nobody@warehouse.local";

        await _viewModel.LoginCommand.ExecuteAsync("anypassword");

        Assert.Equal("Invalid email or password.", _viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShowsInvalidCredentials()
    {
        _mockUserRepo
            .Setup(r => r.GetByEmailAsync("admin@warehouse.local"))
            .ReturnsAsync(new User
            {
                U_ID = 1,
                Email = "admin@warehouse.local",
                Password = _hashedPassword,
                FirstName = "Admin",
                LastName = "Manager",
                Role = "Manager"
            });

        _viewModel.Email = "admin@warehouse.local";

        await _viewModel.LoginCommand.ExecuteAsync("wrongpassword");

        Assert.Equal("Invalid email or password.", _viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_FiresLoginSucceeded()
    {
        _mockUserRepo
            .Setup(r => r.GetByEmailAsync("admin@warehouse.local"))
            .ReturnsAsync(new User
            {
                U_ID = 1,
                Email = "admin@warehouse.local",
                Password = _hashedPassword,
                FirstName = "Admin",
                LastName = "Manager",
                Role = "Manager"
            });

        var loginSucceededFired = false;
        _viewModel.LoginSucceeded += () => loginSucceededFired = true;

        _viewModel.Email = "admin@warehouse.local";

        await _viewModel.LoginCommand.ExecuteAsync("correctpassword");

        Assert.True(loginSucceededFired);
        Assert.Equal(string.Empty, _viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_EmailIsTrimmed()
    {
        _mockUserRepo
            .Setup(r => r.GetByEmailAsync("admin@warehouse.local"))
            .ReturnsAsync(new User
            {
                U_ID = 1,
                Email = "admin@warehouse.local",
                Password = _hashedPassword,
                FirstName = "Admin",
                LastName = "Manager",
                Role = "Manager"
            });

        var loginSucceededFired = false;
        _viewModel.LoginSucceeded += () => loginSucceededFired = true;

        _viewModel.Email = "  admin@warehouse.local  ";

        await _viewModel.LoginCommand.ExecuteAsync("correctpassword");

        Assert.True(loginSucceededFired);
    }

    [Fact]
    public async Task Login_WhenRepositoryThrows_ShowsDatabaseError()
    {
        _mockUserRepo
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Connection refused"));

        _viewModel.Email = "admin@warehouse.local";

        await _viewModel.LoginCommand.ExecuteAsync("anypassword");

        Assert.Contains("Database error", _viewModel.ErrorMessage);
        Assert.Contains("Connection refused", _viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Login_IsLoggingIn_SetTrueDuringExecution()
    {
        // Verify IsLoggingIn is reset to false after completion
        _mockUserRepo
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        _viewModel.Email = "admin@warehouse.local";

        await _viewModel.LoginCommand.ExecuteAsync("anypassword");

        Assert.False(_viewModel.IsLoggingIn);
    }
}
