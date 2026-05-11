using System.Collections.ObjectModel;
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

    public InventoryViewModel(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    [ObservableProperty]
    private ObservableCollection<Item> _items = [];

    [ObservableProperty]
    private Item? _selectedItem;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        try
        {
            var items = await _itemRepository.GetAllAsync();
            Items = new ObservableCollection<Item>(items);
            StatusMessage = $"{Items.Count} items";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load items: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        var formVm = App.Services.GetRequiredService<ItemFormViewModel>();
        await formVm.LoadCategoriesAsync();

        var window = new ItemFormWindow
        {
            DataContext = formVm
        };

        formVm.SaveCompleted += () => window.DialogResult = true;

        if (window.ShowDialog() == true)
        {
            await LoadItemsAsync();
        }
    }
}
