using Warehouse.Core.Models;

namespace Warehouse.Core.Interfaces;

public interface IItemRepository
{
    Task<List<Item>> GetAllAsync();
    Task<int> CreateAsync(Item item);
}
