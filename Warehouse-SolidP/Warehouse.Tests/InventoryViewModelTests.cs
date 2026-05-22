using Moq;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Desktop.ViewModels;
using Xunit;

namespace Warehouse.Tests;

public class InventoryViewModelTests
{
    private readonly Mock<IItemRepository> _mockItemRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly InventoryViewModel _viewModel;

    public InventoryViewModelTests()
    {
        _mockItemRepo = new Mock<IItemRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _viewModel = new InventoryViewModel(_mockItemRepo.Object, _mockCategoryRepo.Object);
    }

    [Fact]
    public async Task LoadItems_PopulatesItemsCollection()
    {
        var items = new List<Item>
        {
            new() { I_ID = 1, Name = "Latitude 5520", Brand = "Dell", CategoryName = "Laptop", SerialNumber = "DL-001", Status = "In Stock" },
            new() { I_ID = 2, Name = "EliteDisplay E243", Brand = "HP", CategoryName = "Monitor", SerialNumber = "HP-001", Status = "In Stock" },
            new() { I_ID = 3, Name = "MX Keys", Brand = "Logitech", CategoryName = "Keyboard", SerialNumber = "LG-001", Status = "Defective" }
        };

        _mockItemRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(items);

        await _viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(3, _viewModel.Items.Count);
        Assert.Equal("Latitude 5520", _viewModel.Items[0].Name);
    }

    [Fact]
    public async Task LoadItems_SetsStatusMessageWithCount()
    {
        _mockItemRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Item>
            {
                new() { I_ID = 1, Name = "Test Item", SerialNumber = "T-001" }
            });

        await _viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal("1 items", _viewModel.StatusMessage);
    }

    [Fact]
    public async Task LoadItems_WithEmptyDatabase_ShowsZeroItems()
    {
        _mockItemRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Item>());

        await _viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Empty(_viewModel.Items);
        Assert.Equal("0 items", _viewModel.StatusMessage);
    }

    [Fact]
    public async Task LoadItems_WhenRepositoryThrows_ShowsErrorInStatus()
    {
        _mockItemRepo
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Connection refused"));

        await _viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Contains("Failed to load items", _viewModel.StatusMessage);
        Assert.Contains("Connection refused", _viewModel.StatusMessage);
    }

    [Fact]
    public async Task LoadItems_ReplacesExistingCollection()
    {
        // First load
        _mockItemRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Item>
            {
                new() { I_ID = 1, Name = "Item A", SerialNumber = "A-001" },
                new() { I_ID = 2, Name = "Item B", SerialNumber = "B-001" }
            });

        await _viewModel.LoadCommand.ExecuteAsync(null);
        Assert.Equal(2, _viewModel.Items.Count);

        // Second load with different data
        _mockItemRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Item>
            {
                new() { I_ID = 3, Name = "Item C", SerialNumber = "C-001" }
            });

        await _viewModel.LoadCommand.ExecuteAsync(null);
        Assert.Single(_viewModel.Items);
        Assert.Equal("Item C", _viewModel.Items[0].Name);
    }

    [Fact]
    public void SelectedItem_DefaultsToNull()
    {
        Assert.Null(_viewModel.SelectedItem);
    }

    [Fact]
    public void SelectedItem_CanBeSet()
    {
        var item = new Item { I_ID = 1, Name = "Test", SerialNumber = "T-001" };
        _viewModel.SelectedItem = item;

        Assert.Equal(item, _viewModel.SelectedItem);
    }
}
