using Microsoft.AspNetCore.Mvc;
using Warehouse.Core.Interfaces;

namespace Warehouse.Web.Controllers;

public class InventoryController : Controller
{
    private readonly IItemRepository _itemRepository;

    public InventoryController(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _itemRepository.GetAllAsync();
        return View(items);
    }
}
