using Moq;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Desktop.ViewModels;
using Xunit;

namespace Warehouse.Tests;

public class ItemFormViewModelTests
{
    private readonly Mock<IItemRepository> _mockItemRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly ItemFormViewModel _viewModel;

    public ItemFormViewModelTests()
    {
        _mockItemRepo = new Mock<IItemRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _viewModel = new ItemFormViewModel(_mockItemRepo.Object, _mockCategoryRepo.Object);
    }

    // ---- Validation tests ----

    [Fact]
    public async Task Save_WithEmptyName_ShowsError()
    {
        _viewModel.Name = "";
        _viewModel.Brand = "Dell";
        _viewModel.SerialNumber = "ABC-123";
        _viewModel.SelectedCategory = new Category { C_ID = 1, Name = "Laptop" };

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Name is required.", _viewModel.ErrorMessage);
        _mockItemRepo.Verify(r => r.CreateAsync(It.IsAny<Item>()), Times.Never);
    }

    [Fact]
    public async Task Save_WithEmptyBrand_ShowsError()
    {
        _viewModel.Name = "Latitude 5520";
        _viewModel.Brand = "";
        _viewModel.SerialNumber = "ABC-123";
        _viewModel.SelectedCategory = new Category { C_ID = 1, Name = "Laptop" };

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Brand is required.", _viewModel.ErrorMessage);
        _mockItemRepo.Verify(r => r.CreateAsync(It.IsAny<Item>()), Times.Never);
    }

    [Fact]
    public async Task Save_WithEmptySerialNumber_ShowsError()
    {
        _viewModel.Name = "Latitude 5520";
        _viewModel.Brand = "Dell";
        _viewModel.SerialNumber = "";
        _viewModel.SelectedCategory = new Category { C_ID = 1, Name = "Laptop" };

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Serial number is required.", _viewModel.ErrorMessage);
        _mockItemRepo.Verify(r => r.CreateAsync(It.IsAny<Item>()), Times.Never);
    }

    [Fact]
    public async Task Save_WithNoCategory_ShowsError()
    {
        _viewModel.Name = "Latitude 5520";
        _viewModel.Brand = "Dell";
        _viewModel.SerialNumber = "ABC-123";
        _viewModel.SelectedCategory = null;

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Please select a category.", _viewModel.ErrorMessage);
        _mockItemRepo.Verify(r => r.CreateAsync(It.IsAny<Item>()), Times.Never);
    }

    [Fact]
    public async Task Save_WithWhitespaceOnlyName_ShowsError()
    {
        _viewModel.Name = "   ";
        _viewModel.Brand = "Dell";
        _viewModel.SerialNumber = "ABC-123";
        _viewModel.SelectedCategory = new Category { C_ID = 1, Name = "Laptop" };

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Name is required.", _viewModel.ErrorMessage);
    }

    // ---- Successful save tests ----

    [Fact]
    public async Task Save_WithValidData_CallsCreateAsync()
    {
        _mockItemRepo
            .Setup(r => r.CreateAsync(It.IsAny<Item>()))
            .ReturnsAsync(1);

        _viewModel.Name = "Latitude 5520";
        _viewModel.Brand = "Dell";
        _viewModel.SerialNumber = "DL-5520-001";
        _viewModel.Status = "In Stock";
        _viewModel.SelectedCategory = new Category { C_ID = 1, Name = "Laptop" };

        await _viewModel.SaveCommand.ExecuteAsync(null);

        _mockItemRepo.Verify(r => r.CreateAsync(It.Is<Item>(i =>
            i.Name == "Latitude 5520" &&
            i.Brand == "Dell" &&
            i.SerialNumber == "DL-5520-001" &&
            i.Status == "In Stock" &&
            i.C_ID == 1
        )), Times.Once);
    }

    [Fact]
    public async Task Save_WithValidData_FiresSaveCompleted()
    {
        _mockItemRepo
            .Setup(r => r.CreateAsync(It.IsAny<Item>()))
            .ReturnsAsync(1);

        var saveCompletedFired = false;
        _viewModel.SaveCompleted += () => saveCompletedFired = true;

        _viewModel.Name = "Latitude 5520";
        _viewModel.Brand = "Dell";
        _viewModel.SerialNumber = "DL-5520-001";
        _viewModel.SelectedCategory = new Category { C_ID = 1, Name = "Laptop" };

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.True(saveCompletedFired);
    }

    [Fact]
    public async Task Save_TrimsWhitespaceFromFields()
    {
        _mockItemRepo
            .Setup(r => r.CreateAsync(It.IsAny<Item>()))
            .ReturnsAsync(1);

        _viewModel.Name = "  Latitude 5520  ";
        _viewModel.Brand = "  Dell  ";
        _viewModel.SerialNumber = "  DL-5520-001  ";
        _viewModel.Description = "  Some description  ";
        _viewModel.SelectedCategory = new Category { C_ID = 1, Name = "Laptop" };

        await _viewModel.SaveCommand.ExecuteAsync(null);

        _mockItemRepo.Verify(r => r.CreateAsync(It.Is<Item>(i =>
            i.Name == "Latitude 5520" &&
            i.Brand == "Dell" &&
            i.SerialNumber == "DL-5520-001" &&
            i.Description == "Some description"
        )), Times.Once);
    }

    [Fact]
    public async Task Save_WithEmptyDescription_SetsNull()
    {
        _mockItemRepo
            .Setup(r => r.CreateAsync(It.IsAny<Item>()))
            .ReturnsAsync(1);

        _viewModel.Name = "Latitude 5520";
        _viewModel.Brand = "Dell";
        _viewModel.SerialNumber = "DL-5520-001";
        _viewModel.Description = "   ";
        _viewModel.SelectedCategory = new Category { C_ID = 1, Name = "Laptop" };

        await _viewModel.SaveCommand.ExecuteAsync(null);

        _mockItemRepo.Verify(r => r.CreateAsync(It.Is<Item>(i =>
            i.Description == null
        )), Times.Once);
    }

    [Fact]
    public async Task Save_WhenRepositoryThrows_ShowsError()
    {
        _mockItemRepo
            .Setup(r => r.CreateAsync(It.IsAny<Item>()))
            .ThrowsAsync(new Exception("Connection lost"));

        _viewModel.Name = "Latitude 5520";
        _viewModel.Brand = "Dell";
        _viewModel.SerialNumber = "DL-5520-001";
        _viewModel.SelectedCategory = new Category { C_ID = 1, Name = "Laptop" };

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Contains("Failed to save", _viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Save_ClearsErrorOnRetry()
    {
        // First attempt: fail validation
        _viewModel.Name = "";
        await _viewModel.SaveCommand.ExecuteAsync(null);
        Assert.NotEmpty(_viewModel.ErrorMessage);

        // Second attempt: fix the issue
        _mockItemRepo
            .Setup(r => r.CreateAsync(It.IsAny<Item>()))
            .ReturnsAsync(1);

        _viewModel.Name = "Latitude 5520";
        _viewModel.Brand = "Dell";
        _viewModel.SerialNumber = "DL-5520-001";
        _viewModel.SelectedCategory = new Category { C_ID = 1, Name = "Laptop" };

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Empty(_viewModel.ErrorMessage);
    }

    // ---- LoadCategories tests ----

    [Fact]
    public async Task LoadCategories_PopulatesCategoriesCollection()
    {
        var categories = new List<Category>
        {
            new() { C_ID = 1, Name = "Laptop" },
            new() { C_ID = 2, Name = "Monitor" },
            new() { C_ID = 3, Name = "Keyboard" }
        };

        _mockCategoryRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(categories);

        await _viewModel.LoadCategoriesAsync();

        Assert.Equal(3, _viewModel.Categories.Count);
        Assert.Equal("Laptop", _viewModel.Categories[0].Name);
    }
}
