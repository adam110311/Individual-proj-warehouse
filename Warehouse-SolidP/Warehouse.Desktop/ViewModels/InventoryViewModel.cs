using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Desktop.Views;

namespace Warehouse.Desktop.ViewModels;

public partial class InventoryViewModel : ObservableObject
{
    private readonly IItemRepository _itemRepository;
    private readonly ICategoryRepository _categoryRepository;

    public InventoryViewModel(IItemRepository itemRepository, ICategoryRepository categoryRepository)
    {
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
    }

    [ObservableProperty]
    private ObservableCollection<Item> _items = [];

    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [ObservableProperty]
    private Item? _selectedItem;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Category? _selectedCategoryFilter;

    [ObservableProperty]
    private string? _selectedStatusFilter;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<string> StatusOptions { get; } = new(["", "In Stock", "Pending", "Dispatched", "Defective"]);

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            var cats = await _categoryRepository.GetAllAsync();
            cats.Insert(0, new Category { C_ID = 0, Name = "" });
            Categories = new ObservableCollection<Category>(cats);

            await SearchAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            var categoryId = SelectedCategoryFilter?.C_ID > 0 ? SelectedCategoryFilter.C_ID : (int?)null;
            var status = string.IsNullOrEmpty(SelectedStatusFilter) ? null : SelectedStatusFilter;
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;

            var items = await _itemRepository.SearchAsync(search, categoryId, status);
            Items = new ObservableCollection<Item>(items);
            StatusMessage = $"{Items.Count} items";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        var formVm = App.Services.GetRequiredService<ItemFormViewModel>();
        await formVm.LoadCategoriesAsync();

        var window = new ItemFormWindow { DataContext = formVm };
        formVm.SaveCompleted += () => window.DialogResult = true;

        if (window.ShowDialog() == true)
            await SearchAsync();
    }

    [RelayCommand]
    private async Task EditItemAsync()
    {
        if (SelectedItem == null) return;

        var formVm = App.Services.GetRequiredService<ItemFormViewModel>();
        formVm.LoadForEdit(SelectedItem);
        await formVm.LoadCategoriesAsync();
        formVm.SelectedCategory = formVm.Categories.FirstOrDefault(c => c.C_ID == SelectedItem.C_ID);

        var window = new ItemFormWindow
        {
            DataContext = formVm,
            Title = $"Edit Item - ID {SelectedItem.I_ID}"
        };

        formVm.SaveCompleted += () => window.DialogResult = true;

        if (window.ShowDialog() == true)
            await SearchAsync();
    }

    [RelayCommand]
    private async Task DeleteItemAsync()
    {
        if (SelectedItem == null) return;

        var result = MessageBox.Show(
            $"Delete '{SelectedItem.Name}' ({SelectedItem.SerialNumber})?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _itemRepository.DeleteAsync(SelectedItem.I_ID);
            await SearchAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to delete: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ViewItemDetail()
    {
        if (SelectedItem == null) return;

        var window = new ItemDetailWindow { DataContext = SelectedItem };
        window.Show();
    }

    [RelayCommand]
    private void OpenOrders()
    {
        var orderVm = App.Services.GetRequiredService<OrderListViewModel>();
        var window = new OrderListWindow { DataContext = orderVm };
        window.Show();
    }
}
