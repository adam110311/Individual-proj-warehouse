namespace Warehouse.Core.Models;

public class CategoryDispatchStat
{
    public string CategoryName { get; set; } = string.Empty;
    public int TotalDispatched { get; set; }
    public int OrderCount { get; set; }
    public double AveragePerOrder { get; set; }
}

public class StockDepletionEstimate
{
    public string CategoryName { get; set; } = string.Empty;
    public int CurrentInStock { get; set; }
    public double MonthlyDispatchRate { get; set; }
    public int? EstimatedDaysUntilEmpty { get; set; }
    public string Urgency { get; set; } = string.Empty;
}
