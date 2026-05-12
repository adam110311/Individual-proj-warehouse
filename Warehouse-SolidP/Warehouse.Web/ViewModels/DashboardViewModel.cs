namespace Warehouse.Web.ViewModels;

public class DashboardViewModel
{
    public int TotalItems { get; set; }
    public int InStockCount { get; set; }
    public int DispatchedCount { get; set; }
    public int DefectiveCount { get; set; }
    public int PendingCount { get; set; }

    public List<CategoryCount> ItemsByCategory { get; set; } = [];
}

public class CategoryCount
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}
