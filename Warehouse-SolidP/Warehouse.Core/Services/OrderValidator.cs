using Warehouse.Core.Models;

namespace Warehouse.Core.Services;

/// <summary>
/// Shared validation logic for Order operations.
/// Used by both the desktop client and web application.
/// </summary>
public static class OrderValidator
{
    public static readonly string[] ValidOrderTypes = ["Inbound", "Outbound"];
    public static readonly string[] ValidStatuses = ["Created", "In Progress", "Completed"];

    public static string? Validate(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.OrderType))
            return "Order type is required.";

        if (!ValidOrderTypes.Contains(order.OrderType))
            return $"Invalid order type '{order.OrderType}'.";

        return null;
    }

    /// <summary>
    /// Checks whether an order can be completed based on its current state.
    /// </summary>
    public static string? CanComplete(Order order)
    {
        if (order.Status == "Completed")
            return "This order is already completed.";

        if (!order.Items.Any())
            return "Cannot complete an order with no items.";

        if (order.OrderType == "Outbound")
        {
            var nonStockItems = order.Items.Where(i => i.Status != "In Stock").ToList();
            if (nonStockItems.Any())
                return $"{nonStockItems.Count} item(s) are not in 'In Stock' status and cannot be dispatched.";
        }

        if (order.OrderType == "Inbound")
        {
            var nonPendingItems = order.Items.Where(i => i.Status != "Pending").ToList();
            if (nonPendingItems.Any())
                return $"{nonPendingItems.Count} item(s) are not in 'Pending' status.";
        }

        return null;
    }
}
