using Warehouse.Core.Models;

namespace Warehouse.Core.Interfaces;

/// <summary>
/// Analytics-specific data queries. Separated from IOrderRepository
/// following the Interface Segregation Principle: consumers that only
/// need analytics data do not depend on order CRUD methods.
/// </summary>
public interface IAnalyticsRepository
{
    /// <summary>
    /// Returns the number of dispatched items per category from completed outbound orders.
    /// </summary>
    Task<List<CategoryDispatchRaw>> GetDispatchCountsByCategoryAsync();

    /// <summary>
    /// Returns the total number of completed outbound orders.
    /// </summary>
    Task<int> GetCompletedOutboundOrderCountAsync();

    /// <summary>
    /// Returns the date of the earliest completed outbound order, or null if none exist.
    /// Used to calculate the time window for monthly dispatch rates.
    /// </summary>
    Task<DateTime?> GetEarliestCompletedOutboundDateAsync();

    /// <summary>
    /// Returns the current in-stock count per category.
    /// </summary>
    Task<List<CategoryStockCount>> GetInStockCountsByCategoryAsync();
}

/// <summary>
/// Raw data returned by the repository.
/// </summary>
public class CategoryDispatchRaw
{
    public string CategoryName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

public class CategoryStockCount
{
    public string CategoryName { get; set; } = string.Empty;
    public int InStockCount { get; set; }
}
