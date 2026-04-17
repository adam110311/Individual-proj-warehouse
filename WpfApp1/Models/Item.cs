namespace WpfApp1.Models;

public class Item
{
    public int I_ID { get; set; }
    public int C_ID { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "In Stock";
    public DateTime DateRegistered { get; set; }

    // Navigation property — populated by repository on load
    public string CategoryName { get; set; } = string.Empty;
}
