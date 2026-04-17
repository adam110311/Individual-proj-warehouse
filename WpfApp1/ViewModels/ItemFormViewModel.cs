using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp1.Models;
using WpfApp1.Repositories;

namespace WpfApp1.ViewModels;

public partial class ItemFormViewModel : ObservableObject
{
    private readonly ItemRepository _itemRepository = new();
    private readonly CategoryRepository _categoryRepository = new();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _brand = string.Empty;

    [ObservableProperty]
    private string _serialNumber = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string _status = "In Stock";

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [ObservableProperty]
    private ObservableCollection<string> _statusOptions = new(["In Stock", "Defective"]);

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Fired when save succeeds. The View subscribes to close the dialog.
    /// </summary>
    public event Action? SaveCompleted;

    public async Task LoadCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        Categories = new ObservableCollection<Category>(categories);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;

        // Validation
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Brand))
        {
            ErrorMessage = "Brand is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SerialNumber))
        {
            ErrorMessage = "Serial number is required.";
            return;
        }

        if (SelectedCategory == null)
        {
            ErrorMessage = "Please select a category.";
            return;
        }

        try
        {
            var item = new Item
            {
                Name = Name.Trim(),
                Brand = Brand.Trim(),
                SerialNumber = SerialNumber.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                Status = Status,
                C_ID = SelectedCategory.C_ID
            };

            await _itemRepository.CreateAsync(item);
            SaveCompleted?.Invoke();
        }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1062)
        {
            // Duplicate entry — most likely the serial number
            ErrorMessage = "An item with this serial number already exists.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save: {ex.Message}";
        }
    }

    // Cancel is handled by the View's Cancel button using IsCancel="True",
    // which closes the dialog window automatically in WPF.
}
