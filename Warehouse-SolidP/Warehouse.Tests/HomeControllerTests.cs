using Microsoft.AspNetCore.Mvc;
using Moq;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Web.Controllers;
using Warehouse.Web.ViewModels;
using Xunit;

namespace Warehouse.Tests;

public class HomeControllerTests
{
    private readonly Mock<IItemRepository> _mockItemRepo;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _mockItemRepo = new Mock<IItemRepository>();
        _controller = new HomeController(_mockItemRepo.Object);
    }

    private List<Item> CreateTestItems()
    {
        return new List<Item>
        {
            new() { I_ID = 1, Name = "Latitude 5520", Status = "In Stock", CategoryName = "Laptop" },
            new() { I_ID = 2, Name = "ThinkPad X1", Status = "In Stock", CategoryName = "Laptop" },
            new() { I_ID = 3, Name = "EliteDisplay E243", Status = "Dispatched", CategoryName = "Monitor" },
            new() { I_ID = 4, Name = "MX Keys", Status = "Defective", CategoryName = "Keyboard" },
            new() { I_ID = 5, Name = "Catalyst 2960-X", Status = "Pending", CategoryName = "Networking" },
            new() { I_ID = 6, Name = "UltraSharp U2723QE", Status = "In Stock", CategoryName = "Monitor" }
        };
    }

    [Fact]
    public async Task Index_ReturnsDashboardViewModelWithCorrectTotalCount()
    {
        _mockItemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(CreateTestItems());

        var result = await _controller.Index() as ViewResult;
        var model = result?.Model as DashboardViewModel;

        Assert.NotNull(model);
        Assert.Equal(6, model.TotalItems);
    }

    [Fact]
    public async Task Index_CountsStatusesCorrectly()
    {
        _mockItemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(CreateTestItems());

        var result = await _controller.Index() as ViewResult;
        var model = result?.Model as DashboardViewModel;

        Assert.NotNull(model);
        Assert.Equal(3, model.InStockCount);
        Assert.Equal(1, model.DispatchedCount);
        Assert.Equal(1, model.DefectiveCount);
        Assert.Equal(1, model.PendingCount);
    }

    [Fact]
    public async Task Index_GroupsByCategoryCorrectly()
    {
        _mockItemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(CreateTestItems());

        var result = await _controller.Index() as ViewResult;
        var model = result?.Model as DashboardViewModel;

        Assert.NotNull(model);

        // 4 categories: Laptop(2), Monitor(2), Keyboard(1), Networking(1)
        Assert.Equal(4, model.ItemsByCategory.Count);

        // Ordered by count descending, so Laptop and Monitor (both 2) come first
        var topCategory = model.ItemsByCategory[0];
        Assert.Equal(2, topCategory.Count);
    }

    [Fact]
    public async Task Index_WithEmptyInventory_ReturnsZeroCounts()
    {
        _mockItemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Item>());

        var result = await _controller.Index() as ViewResult;
        var model = result?.Model as DashboardViewModel;

        Assert.NotNull(model);
        Assert.Equal(0, model.TotalItems);
        Assert.Equal(0, model.InStockCount);
        Assert.Equal(0, model.DispatchedCount);
        Assert.Equal(0, model.DefectiveCount);
        Assert.Equal(0, model.PendingCount);
        Assert.Empty(model.ItemsByCategory);
    }

    [Fact]
    public async Task Index_ReturnsViewResult()
    {
        _mockItemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Item>());

        var result = await _controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Index_CategoryBreakdownOrderedByCountDescending()
    {
        var items = new List<Item>
        {
            new() { I_ID = 1, Status = "In Stock", CategoryName = "Keyboard" },
            new() { I_ID = 2, Status = "In Stock", CategoryName = "Laptop" },
            new() { I_ID = 3, Status = "In Stock", CategoryName = "Laptop" },
            new() { I_ID = 4, Status = "In Stock", CategoryName = "Laptop" },
            new() { I_ID = 5, Status = "In Stock", CategoryName = "Monitor" },
            new() { I_ID = 6, Status = "In Stock", CategoryName = "Monitor" }
        };

        _mockItemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        var result = await _controller.Index() as ViewResult;
        var model = result?.Model as DashboardViewModel;

        Assert.NotNull(model);
        Assert.Equal("Laptop", model.ItemsByCategory[0].CategoryName);
        Assert.Equal(3, model.ItemsByCategory[0].Count);
        Assert.Equal("Monitor", model.ItemsByCategory[1].CategoryName);
        Assert.Equal(2, model.ItemsByCategory[1].Count);
        Assert.Equal("Keyboard", model.ItemsByCategory[2].CategoryName);
        Assert.Equal(1, model.ItemsByCategory[2].Count);
    }
}
