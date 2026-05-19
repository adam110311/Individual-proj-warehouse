using Warehouse.Core.Models;

namespace Warehouse.Core.Services;

/// <summary>
/// Computes dashboard statistics from item data.
/// Shared between web and desktop to avoid duplicating aggregation logic.
/// </summary>
public static class DashboardService
{
    public static DashboardStats ComputeStats(List<Item> items)
    {
        return new DashboardStats
        {
            TotalItems = items.Count,
            InStockCount = items.Count(i => i.Status == "In Stock"),
            DispatchedCount = items.Count(i => i.Status == "Dispatched"),
            DefectiveCount = items.Count(i => i.Status == "Defective"),
            PendingCount = items.Count(i => i.Status == "Pending"),
            ItemsByCategory = items
                .GroupBy(i => i.CategoryName)
                .Select(g => new CategoryStat { CategoryName = g.Key, Count = g.Count() })
                .OrderByDescending(c => c.Count)
                .ToList()
        };
    }
}

public class DashboardStats
{
    public int TotalItems { get; set; }
    public int InStockCount { get; set; }
    public int DispatchedCount { get; set; }
    public int DefectiveCount { get; set; }
    public int PendingCount { get; set; }
    public List<CategoryStat> ItemsByCategory { get; set; } = [];
}

public class CategoryStat
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}
