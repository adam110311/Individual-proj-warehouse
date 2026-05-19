using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Core.Services;
using Warehouse.Desktop.Views;

namespace Warehouse.Desktop.ViewModels;

public partial class OrderListViewModel : ObservableObject
{
    private readonly IOrderRepository _orderRepository;

    public OrderListViewModel(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [ObservableProperty] private ObservableCollection<Order> _orders = [];
    [ObservableProperty] private Order? _selectedOrder;
    [ObservableProperty] private string _statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        try
        {
            var orders = await _orderRepository.GetAllAsync();
            Orders = new ObservableCollection<Order>(orders);
            StatusMessage = $"{Orders.Count} orders";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load orders: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddOrderAsync()
    {
        var formVm = App.Services.GetRequiredService<OrderFormViewModel>();
        await formVm.LoadAsync();

        var window = new OrderFormWindow { DataContext = formVm };
        formVm.SaveCompleted += () => window.DialogResult = true;

        if (window.ShowDialog() == true)
            await LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task CompleteOrderAsync()
    {
        if (SelectedOrder == null) return;

        // Load full order with items for validation
        var fullOrder = await _orderRepository.GetByIdWithItemsAsync(SelectedOrder.O_ID);
        if (fullOrder == null) return;

        var canComplete = OrderValidator.CanComplete(fullOrder);
        if (canComplete != null)
        {
            MessageBox.Show(canComplete, "Cannot Complete", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var msg = fullOrder.OrderType == "Outbound"
            ? $"Complete this outbound order? {fullOrder.Items.Count} item(s) will be marked as Dispatched."
            : $"Complete this inbound order? {fullOrder.Items.Count} item(s) will be activated to In Stock.";

        var result = MessageBox.Show(msg, "Confirm Complete", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _orderRepository.CompleteOrderAsync(SelectedOrder.O_ID);
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to complete order: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteOrderAsync()
    {
        if (SelectedOrder == null) return;

        if (SelectedOrder.Status == "Completed")
        {
            MessageBox.Show("Cannot delete a completed order.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Delete order #{SelectedOrder.O_ID}?",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _orderRepository.DeleteAsync(SelectedOrder.O_ID);
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to delete: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ViewOrderDetailAsync()
    {
        if (SelectedOrder == null) return;

        var fullOrder = await _orderRepository.GetByIdWithItemsAsync(SelectedOrder.O_ID);
        if (fullOrder == null) return;

        var window = new OrderDetailWindow { DataContext = fullOrder };
        window.Show();
    }
}
