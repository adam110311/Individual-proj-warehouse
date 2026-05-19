using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Core.Services;

namespace Warehouse.Desktop.ViewModels;

public partial class ItemFormViewModel : ObservableObject
{
    private readonly IItemRepository _itemRepository;
    private readonly ICategoryRepository _categoryRepository;

    private int? _editingItemId;

    public ItemFormViewModel(IItemRepository itemRepository, ICategoryRepository categoryRepository)
    {
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
    }

    public bool IsEditMode => _editingItemId.HasValue;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _brand = string.Empty;
    [ObservableProperty] private string _serialNumber = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private string _status = "In Stock";
    [ObservableProperty] private Category? _selectedCategory;
    [ObservableProperty] private ObservableCollection<Category> _categories = [];
    [ObservableProperty] private ObservableCollection<string> _statusOptions = new(["In Stock", "Defective"]);
    [ObservableProperty] private string _errorMessage = string.Empty;

    public event Action? SaveCompleted;

    /// <summary>
    /// Pre-populates the form for editing an existing item.
    /// </summary>
    public void LoadForEdit(Item item)
    {
        _editingItemId = item.I_ID;
        Name = item.Name;
        Brand = item.Brand;
        SerialNumber = item.SerialNumber;
        Description = item.Description;
        Status = item.Status;
    }

    public async Task LoadCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        Categories = new ObservableCollection<Category>(categories);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;

        var item = new Item
        {
            I_ID = _editingItemId ?? 0,
            Name = Name.Trim(),
            Brand = Brand.Trim(),
            SerialNumber = SerialNumber.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            Status = Status,
            C_ID = SelectedCategory?.C_ID ?? 0
        };

        // Use shared validation from Core
        var validationError = ItemValidator.Validate(item);
        if (validationError != null)
        {
            ErrorMessage = validationError;
            return;
        }

        try
        {
            if (IsEditMode)
                await _itemRepository.UpdateAsync(item);
            else
                await _itemRepository.CreateAsync(item);

            SaveCompleted?.Invoke();
        }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1062)
        {
            ErrorMessage = "An item with this serial number already exists.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save: {ex.Message}";
        }
    }
}
