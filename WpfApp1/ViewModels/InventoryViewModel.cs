using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp1.Models;
using WpfApp1.Repositories;
using WpfApp1.Views;

namespace WpfApp1.ViewModels;

public partial class InventoryViewModel : ObservableObject
{
    private readonly ItemRepository _itemRepository = new();

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
        var formVm = new ItemFormViewModel();
        await formVm.LoadCategoriesAsync();

        var window = new ItemFormWindow
        {
            DataContext = formVm
        };

        formVm.SaveCompleted += () => window.DialogResult = true;

        if (window.ShowDialog() == true)
        {
            // Reload the full list from DB to stay in sync
            await LoadItemsAsync();
        }
    }
}
