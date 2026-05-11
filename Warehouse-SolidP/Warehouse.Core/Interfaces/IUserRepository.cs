using Warehouse.Core.Models;

namespace Warehouse.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
}
