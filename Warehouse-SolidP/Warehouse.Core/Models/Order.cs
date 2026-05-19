namespace Warehouse.Core.Models;

public class Order
{
    public int O_ID { get; set; }
    public int U_ID { get; set; }
    public string OrderType { get; set; } = string.Empty;
    public string Status { get; set; } = "Created";
    public string? Reference { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    // Navigation properties, populated on detail load
    public string CreatedByName { get; set; } = string.Empty;
    public List<Item> Items { get; set; } = [];
}
