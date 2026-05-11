using Warehouse.Core.Models;

namespace Warehouse.Core.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
}
