using Warehouse.Core.Models;

namespace Warehouse.Web.ViewModels;

public class AnalyticsViewModel
{
    public List<CategoryDispatchStat> DispatchStats { get; set; } = [];
    public List<StockDepletionEstimate> DepletionEstimates { get; set; } = [];
    public int TotalCompletedOrders { get; set; }
    public bool HasOrderHistory { get; set; }
}
