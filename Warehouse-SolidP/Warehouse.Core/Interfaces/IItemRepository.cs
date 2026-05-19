using Warehouse.Core.Models;

namespace Warehouse.Core.Interfaces;

public interface IItemRepository
{
    Task<List<Item>> GetAllAsync();
    Task<Item?> GetByIdAsync(int id);
    Task<int> CreateAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(int id);
    Task<List<Item>> SearchAsync(string? searchTerm, int? categoryId, string? status);
    Task UpdateStatusAsync(int itemId, string newStatus);
}
