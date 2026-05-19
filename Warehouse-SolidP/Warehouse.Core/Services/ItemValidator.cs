using Warehouse.Core.Models;

namespace Warehouse.Core.Services;

/// <summary>
/// Shared validation logic for Item operations. 
/// Used by both the desktop client and web application.
/// </summary>
public static class ItemValidator
{
    public static readonly string[] ValidStatuses = ["Pending", "In Stock", "Dispatched", "Defective"];

    /// <summary>
    /// Validates an item before create or update. Returns null if valid, 
    /// or an error message string if invalid.
    /// </summary>
    public static string? Validate(Item item)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
            return "Name is required.";

        if (string.IsNullOrWhiteSpace(item.Brand))
            return "Brand is required.";

        if (string.IsNullOrWhiteSpace(item.SerialNumber))
            return "Serial number is required.";

        if (item.C_ID <= 0)
            return "Please select a category.";

        if (!ValidStatuses.Contains(item.Status))
            return $"Invalid status '{item.Status}'.";

        return null;
    }

    /// <summary>
    /// Checks whether a status transition is allowed.
    /// Pending -> In Stock (inbound order completion)
    /// In Stock -> Dispatched (outbound order completion)
    /// In Stock -> Defective (manual)
    /// Defective -> In Stock (repaired)
    /// </summary>
    public static bool IsValidStatusTransition(string from, string to)
    {
        return (from, to) switch
        {
            ("Pending", "In Stock") => true,
            ("In Stock", "Dispatched") => true,
            ("In Stock", "Defective") => true,
            ("Defective", "In Stock") => true,
            _ => false
        };
    }
}
