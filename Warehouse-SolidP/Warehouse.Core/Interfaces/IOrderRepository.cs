using Warehouse.Core.Models;

namespace Warehouse.Core.Interfaces;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdWithItemsAsync(int id);
    Task<int> CreateAsync(Order order, List<Item>? newItems = null);
    Task UpdateAsync(Order order);
    Task DeleteAsync(int id);
    Task AddItemToOrderAsync(int orderId, int itemId);
    Task RemoveItemFromOrderAsync(int orderId, int itemId);
    Task CompleteOrderAsync(int orderId);
}
