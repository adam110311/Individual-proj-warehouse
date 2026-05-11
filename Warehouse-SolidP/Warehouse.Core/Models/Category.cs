namespace Warehouse.Core.Models;

public class Category
{
    public int C_ID { get; set; }
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}
