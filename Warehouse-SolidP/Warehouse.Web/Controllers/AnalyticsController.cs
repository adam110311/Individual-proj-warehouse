using Microsoft.AspNetCore.Mvc;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Services;
using Warehouse.Web.ViewModels;

namespace Warehouse.Web.Controllers;

public class AnalyticsController : Controller
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public AnalyticsController(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<IActionResult> Index()
    {
        // Data layer: fetch raw numbers
        var dispatchCounts = await _analyticsRepository.GetDispatchCountsByCategoryAsync();
        var orderCount = await _analyticsRepository.GetCompletedOutboundOrderCountAsync();
        var earliestDate = await _analyticsRepository.GetEarliestCompletedOutboundDateAsync();
        var stockCounts = await _analyticsRepository.GetInStockCountsByCategoryAsync();

        // Business layer: compute insights from raw data
        var dispatchStats = AnalyticsService.ComputeDispatchStats(dispatchCounts, orderCount);
        var depletionEstimates = AnalyticsService.ComputeDepletionEstimates(
            stockCounts, dispatchCounts, earliestDate);

        var viewModel = new AnalyticsViewModel
        {
            DispatchStats = dispatchStats,
            DepletionEstimates = depletionEstimates,
            TotalCompletedOrders = orderCount,
            HasOrderHistory = orderCount > 0
        };

        return View(viewModel);
    }
}
