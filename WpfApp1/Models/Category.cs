namespace WpfApp1.Models;

public class Category
{
    public int C_ID { get; set; }
    public string Name { get; set; } = string.Empty;

    // Displayed in ComboBox dropdowns
    public override string ToString() => Name;
}
