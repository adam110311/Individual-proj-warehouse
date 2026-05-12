using Microsoft.AspNetCore.Mvc;
using Warehouse.Core.Interfaces;
using Warehouse.Web.ViewModels;

namespace Warehouse.Web.Controllers;

public class HomeController : Controller
{
    private readonly IItemRepository _itemRepository;

    public HomeController(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _itemRepository.GetAllAsync();

        var viewModel = new DashboardViewModel
        {
            TotalItems = items.Count,
            InStockCount = items.Count(i => i.Status == "In Stock"),
            DispatchedCount = items.Count(i => i.Status == "Dispatched"),
            DefectiveCount = items.Count(i => i.Status == "Defective"),
            PendingCount = items.Count(i => i.Status == "Pending"),
            ItemsByCategory = items
                .GroupBy(i => i.CategoryName)
                .Select(g => new CategoryCount
                {
                    CategoryName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Count)
                .ToList()
        };

        return View(viewModel);
    }
}
