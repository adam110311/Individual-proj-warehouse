using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;

namespace Warehouse.Core.Services;

/// <summary>
/// Computes analytics from raw data provided by the repository layer.
/// This is business logic: the repository fetches numbers from the database,
/// this service interprets them into actionable insights.
/// </summary>
public static class AnalyticsService
{
    /// <summary>
    /// Computes the most dispatched categories with average-per-order.
    /// </summary>
    public static List<CategoryDispatchStat> ComputeDispatchStats(
        List<CategoryDispatchRaw> dispatchCounts,
        int totalCompletedOrders)
    {
        return dispatchCounts
            .Select(d => new CategoryDispatchStat
            {
                CategoryName = d.CategoryName,
                TotalDispatched = d.ItemCount,
                OrderCount = totalCompletedOrders,
                AveragePerOrder = totalCompletedOrders > 0
                    ? Math.Floor((double)d.ItemCount / totalCompletedOrders * 10) / 10
                    : 0
            })
            .OrderByDescending(d => d.TotalDispatched)
            .ToList();
    }

    /// <summary>
    /// Estimates how long current stock will last per category based on
    /// historical dispatch rates.
    /// </summary>
    public static List<StockDepletionEstimate> ComputeDepletionEstimates(
        List<CategoryStockCount> stockCounts,
        List<CategoryDispatchRaw> dispatchCounts,
        DateTime? earliestOrderDate)
    {
        // Calculate the number of months of order history we have
        double monthsOfHistory = 1;

        if (earliestOrderDate.HasValue)
        {
            var span = DateTime.Now - earliestOrderDate.Value;
            monthsOfHistory = Math.Max(span.TotalDays / 30.0, 1);
        }

        // Build a lookup of dispatch counts by category
        var dispatchLookup = dispatchCounts.ToDictionary(d => d.CategoryName, d => d.ItemCount);

        var estimates = new List<StockDepletionEstimate>();

        foreach (var stock in stockCounts)
        {
            var dispatched = dispatchLookup.GetValueOrDefault(stock.CategoryName, 0);
            var monthlyRate = dispatched / monthsOfHistory;

            int? daysUntilEmpty = null;
            string urgency;

            if (monthlyRate <= 0)
            {
                // No dispatch history for this category
                urgency = "No data";
            }
            else
            {
                var dailyRate = monthlyRate / 30.0;
                var daysRemaining = stock.InStockCount / dailyRate;
                daysUntilEmpty = (int)Math.Floor(daysRemaining);

                urgency = daysUntilEmpty switch
                {
                    <= 7 => "Critical",
                    <= 30 => "Low",
                    <= 90 => "Adequate",
                    _ => "Comfortable"
                };
            }

            estimates.Add(new StockDepletionEstimate
            {
                CategoryName = stock.CategoryName,
                CurrentInStock = stock.InStockCount,
                MonthlyDispatchRate = Math.Round(monthlyRate, 1),
                EstimatedDaysUntilEmpty = daysUntilEmpty,
                Urgency = urgency
            });
        }

        // Critical and Low first, then by days remaining
        return estimates
            .OrderBy(e => e.Urgency switch
            {
                "Critical" => 0,
                "Low" => 1,
                "Adequate" => 2,
                "Comfortable" => 3,
                _ => 4
            })
            .ThenBy(e => e.EstimatedDaysUntilEmpty ?? int.MaxValue)
            .ToList();
    }
}
