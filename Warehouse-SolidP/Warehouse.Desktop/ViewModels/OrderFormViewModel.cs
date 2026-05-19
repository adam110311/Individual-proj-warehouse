using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Core.Services;

namespace Warehouse.Desktop.ViewModels;

public partial class OrderFormViewModel : ObservableObject
{
    private readonly IOrderRepository _orderRepository;
    private readonly IItemRepository _itemRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly int _currentUserId;

    public OrderFormViewModel(
        IOrderRepository orderRepository,
        IItemRepository itemRepository,
        ICategoryRepository categoryRepository)
    {
        _orderRepository = orderRepository;
        _itemRepository = itemRepository;
        _categoryRepository = categoryRepository;
        _currentUserId = App.CurrentUserId;
    }

    [ObservableProperty] private string _orderType = "Inbound";
    [ObservableProperty] private string? _reference;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // Outbound: available items to select from
    [ObservableProperty] private ObservableCollection<Item> _availableItems = [];
    [ObservableProperty] private Item? _selectedAvailableItem;

    // Items assigned to this order
    [ObservableProperty] private ObservableCollection<Item> _orderItems = [];
    [ObservableProperty] private Item? _selectedOrderItem;

    // Inbound: fields for entering new items
    [ObservableProperty] private string _newItemName = string.Empty;
    [ObservableProperty] private string _newItemBrand = string.Empty;
    [ObservableProperty] private string _newItemSerial = string.Empty;
    [ObservableProperty] private string? _newItemDescription;
    [ObservableProperty] private Category? _newItemCategory;
    [ObservableProperty] private ObservableCollection<Category> _categories = [];

    public ObservableCollection<string> OrderTypes { get; } = new(["Inbound", "Outbound"]);

    public event Action? SaveCompleted;

    public async Task LoadAsync()
    {
        var cats = await _categoryRepository.GetAllAsync();
        Categories = new ObservableCollection<Category>(cats);

        await LoadAvailableItemsAsync();
    }

    private async Task LoadAvailableItemsAsync()
    {
        var items = await _itemRepository.SearchAsync(null, null, "In Stock");
        AvailableItems = new ObservableCollection<Item>(items);
    }

    [RelayCommand]
    private void AddInboundItem()
    {
        if (string.IsNullOrWhiteSpace(NewItemName) || string.IsNullOrWhiteSpace(NewItemBrand)
            || string.IsNullOrWhiteSpace(NewItemSerial) || NewItemCategory == null)
        {
            ErrorMessage = "Fill in all item fields before adding.";
            return;
        }

        if (OrderItems.Any(i => i.SerialNumber == NewItemSerial.Trim()))
        {
            ErrorMessage = "An item with this serial number is already in the order.";
            return;
        }

        ErrorMessage = string.Empty;

        OrderItems.Add(new Item
        {
            Name = NewItemName.Trim(),
            Brand = NewItemBrand.Trim(),
            SerialNumber = NewItemSerial.Trim(),
            Description = string.IsNullOrWhiteSpace(NewItemDescription) ? null : NewItemDescription.Trim(),
            C_ID = NewItemCategory.C_ID,
            CategoryName = NewItemCategory.Name,
            Status = "Pending"
        });

        NewItemName = string.Empty;
        NewItemBrand = string.Empty;
        NewItemSerial = string.Empty;
        NewItemDescription = null;
        NewItemCategory = null;
    }

    [RelayCommand]
    private void AddOutboundItem()
    {
        if (SelectedAvailableItem == null) return;

        if (OrderItems.Any(i => i.I_ID == SelectedAvailableItem.I_ID))
        {
            ErrorMessage = "This item is already in the order.";
            return;
        }

        ErrorMessage = string.Empty;
        OrderItems.Add(SelectedAvailableItem);
        AvailableItems.Remove(SelectedAvailableItem);
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (SelectedOrderItem == null) return;

        if (OrderType == "Outbound")
            AvailableItems.Add(SelectedOrderItem);

        OrderItems.Remove(SelectedOrderItem);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;

        if (!OrderItems.Any())
        {
            ErrorMessage = "Add at least one item to the order.";
            return;
        }

        var order = new Order
        {
            U_ID = _currentUserId,
            OrderType = OrderType,
            Status = "Created",
            Reference = string.IsNullOrWhiteSpace(Reference) ? null : Reference.Trim(),
            Items = OrderItems.ToList()
        };

        var validationError = OrderValidator.Validate(order);
        if (validationError != null)
        {
            ErrorMessage = validationError;
            return;
        }

        try
        {
            if (OrderType == "Inbound")
                await _orderRepository.CreateAsync(order, OrderItems.ToList());
            else
                await _orderRepository.CreateAsync(order);

            SaveCompleted?.Invoke();
        }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1062)
        {
            ErrorMessage = "Duplicate serial number found. Check the items in this order.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save: {ex.Message}";
        }
    }
}
