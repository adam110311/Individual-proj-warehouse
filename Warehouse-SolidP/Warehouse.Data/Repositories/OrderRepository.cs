using MySqlConnector;
using Warehouse.Core.Interfaces;
using Warehouse.Core.Models;
using Warehouse.Data.Services;

namespace Warehouse.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly DatabaseService _db;

    public OrderRepository(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            @"SELECT o.O_ID, o.U_ID, o.OrderType, o.Status, o.Reference, 
                     o.CreatedDate, o.CompletedDate,
                     CONCAT(u.FirstName, ' ', u.LastName) AS CreatedByName
              FROM `Order` o
              INNER JOIN User u ON o.U_ID = u.U_ID
              ORDER BY o.O_ID DESC",
            conn);

        await using var reader = await cmd.ExecuteReaderAsync();
        var orders = new List<Order>();

        while (await reader.ReadAsync())
        {
            orders.Add(new Order
            {
                O_ID = reader.GetInt32("O_ID"),
                U_ID = reader.GetInt32("U_ID"),
                OrderType = reader.GetString("OrderType"),
                Status = reader.GetString("Status"),
                Reference = reader.IsDBNull(reader.GetOrdinal("Reference"))
                    ? null : reader.GetString("Reference"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                CompletedDate = reader.IsDBNull(reader.GetOrdinal("CompletedDate"))
                    ? null : reader.GetDateTime("CompletedDate"),
                CreatedByName = reader.GetString("CreatedByName")
            });
        }

        return orders;
    }

    public async Task<Order?> GetByIdWithItemsAsync(int id)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        // Get the order
        await using var orderCmd = new MySqlCommand(
            @"SELECT o.O_ID, o.U_ID, o.OrderType, o.Status, o.Reference, 
                     o.CreatedDate, o.CompletedDate,
                     CONCAT(u.FirstName, ' ', u.LastName) AS CreatedByName
              FROM `Order` o
              INNER JOIN User u ON o.U_ID = u.U_ID
              WHERE o.O_ID = @Id",
            conn);

        orderCmd.Parameters.AddWithValue("@Id", id);

        Order? order = null;

        await using (var reader = await orderCmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                order = new Order
                {
                    O_ID = reader.GetInt32("O_ID"),
                    U_ID = reader.GetInt32("U_ID"),
                    OrderType = reader.GetString("OrderType"),
                    Status = reader.GetString("Status"),
                    Reference = reader.IsDBNull(reader.GetOrdinal("Reference"))
                        ? null : reader.GetString("Reference"),
                    CreatedDate = reader.GetDateTime("CreatedDate"),
                    CompletedDate = reader.IsDBNull(reader.GetOrdinal("CompletedDate"))
                        ? null : reader.GetDateTime("CompletedDate"),
                    CreatedByName = reader.GetString("CreatedByName")
                };
            }
        }

        if (order == null) return null;

        // Get associated items
        await using var itemsCmd = new MySqlCommand(
            @"SELECT i.I_ID, i.C_ID, i.SerialNumber, i.Name, i.Brand, 
                     i.Description, i.Status, i.DateRegistered, c.Name AS CategoryName
              FROM OrderItem oi
              INNER JOIN Item i ON oi.I_ID = i.I_ID
              INNER JOIN Category c ON i.C_ID = c.C_ID
              WHERE oi.O_ID = @OrderId",
            conn);

        itemsCmd.Parameters.AddWithValue("@OrderId", id);

        await using var itemReader = await itemsCmd.ExecuteReaderAsync();

        while (await itemReader.ReadAsync())
        {
            order.Items.Add(new Item
            {
                I_ID = itemReader.GetInt32("I_ID"),
                C_ID = itemReader.GetInt32("C_ID"),
                SerialNumber = itemReader.GetString("SerialNumber"),
                Name = itemReader.GetString("Name"),
                Brand = itemReader.GetString("Brand"),
                Description = itemReader.IsDBNull(itemReader.GetOrdinal("Description"))
                    ? null : itemReader.GetString("Description"),
                Status = itemReader.GetString("Status"),
                DateRegistered = itemReader.GetDateTime("DateRegistered"),
                CategoryName = itemReader.GetString("CategoryName")
            });
        }

        return order;
    }

    /// <summary>
    /// Creates an order. For inbound orders, newItems contains the items to create 
    /// with Pending status. For outbound orders, the items already exist and are 
    /// linked via their I_ID.
    /// </summary>
    public async Task<int> CreateAsync(Order order, List<Item>? newItems = null)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            // Create the order
            await using var orderCmd = new MySqlCommand(
                @"INSERT INTO `Order` (U_ID, OrderType, Status, Reference)
                  VALUES (@U_ID, @OrderType, @Status, @Reference);
                  SELECT LAST_INSERT_ID();",
                conn, transaction);

            orderCmd.Parameters.AddWithValue("@U_ID", order.U_ID);
            orderCmd.Parameters.AddWithValue("@OrderType", order.OrderType);
            orderCmd.Parameters.AddWithValue("@Status", order.Status);
            orderCmd.Parameters.AddWithValue("@Reference", (object?)order.Reference ?? DBNull.Value);

            var orderId = Convert.ToInt32(await orderCmd.ExecuteScalarAsync());

            if (order.OrderType == "Inbound" && newItems != null)
            {
                // Create new items with Pending status and link them
                foreach (var item in newItems)
                {
                    await using var itemCmd = new MySqlCommand(
                        @"INSERT INTO Item (C_ID, SerialNumber, Name, Brand, Description, Status)
                          VALUES (@C_ID, @SerialNumber, @Name, @Brand, @Description, 'Pending');
                          SELECT LAST_INSERT_ID();",
                        conn, transaction);

                    itemCmd.Parameters.AddWithValue("@C_ID", item.C_ID);
                    itemCmd.Parameters.AddWithValue("@SerialNumber", item.SerialNumber);
                    itemCmd.Parameters.AddWithValue("@Name", item.Name);
                    itemCmd.Parameters.AddWithValue("@Brand", item.Brand);
                    itemCmd.Parameters.AddWithValue("@Description", (object?)item.Description ?? DBNull.Value);

                    var itemId = Convert.ToInt32(await itemCmd.ExecuteScalarAsync());

                    await using var linkCmd = new MySqlCommand(
                        "INSERT INTO OrderItem (O_ID, I_ID) VALUES (@O_ID, @I_ID)",
                        conn, transaction);

                    linkCmd.Parameters.AddWithValue("@O_ID", orderId);
                    linkCmd.Parameters.AddWithValue("@I_ID", itemId);
                    await linkCmd.ExecuteNonQueryAsync();
                }
            }
            else if (order.OrderType == "Outbound")
            {
                // Link existing items
                foreach (var item in order.Items)
                {
                    await using var linkCmd = new MySqlCommand(
                        "INSERT INTO OrderItem (O_ID, I_ID) VALUES (@O_ID, @I_ID)",
                        conn, transaction);

                    linkCmd.Parameters.AddWithValue("@O_ID", orderId);
                    linkCmd.Parameters.AddWithValue("@I_ID", item.I_ID);
                    await linkCmd.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(Order order)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            @"UPDATE `Order` 
              SET OrderType = @OrderType, Status = @Status, Reference = @Reference
              WHERE O_ID = @O_ID",
            conn);

        cmd.Parameters.AddWithValue("@O_ID", order.O_ID);
        cmd.Parameters.AddWithValue("@OrderType", order.OrderType);
        cmd.Parameters.AddWithValue("@Status", order.Status);
        cmd.Parameters.AddWithValue("@Reference", (object?)order.Reference ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            // Remove item links
            await using var unlinkCmd = new MySqlCommand(
                "DELETE FROM OrderItem WHERE O_ID = @Id", conn, transaction);
            unlinkCmd.Parameters.AddWithValue("@Id", id);
            await unlinkCmd.ExecuteNonQueryAsync();

            // Remove the order
            await using var cmd = new MySqlCommand(
                "DELETE FROM `Order` WHERE O_ID = @Id", conn, transaction);
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task AddItemToOrderAsync(int orderId, int itemId)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            "INSERT INTO OrderItem (O_ID, I_ID) VALUES (@O_ID, @I_ID)", conn);

        cmd.Parameters.AddWithValue("@O_ID", orderId);
        cmd.Parameters.AddWithValue("@I_ID", itemId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RemoveItemFromOrderAsync(int orderId, int itemId)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(
            "DELETE FROM OrderItem WHERE O_ID = @O_ID AND I_ID = @I_ID", conn);

        cmd.Parameters.AddWithValue("@O_ID", orderId);
        cmd.Parameters.AddWithValue("@I_ID", itemId);

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Completes an order within a transaction.
    /// Inbound: updates items from Pending to In Stock.
    /// Outbound: updates items from In Stock to Dispatched.
    /// </summary>
    public async Task CompleteOrderAsync(int orderId)
    {
        await using var conn = _db.GetConnection();
        await conn.OpenAsync();
        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            // Get order type
            await using var typeCmd = new MySqlCommand(
                "SELECT OrderType FROM `Order` WHERE O_ID = @Id", conn, transaction);
            typeCmd.Parameters.AddWithValue("@Id", orderId);
            var orderType = (string?)await typeCmd.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Order not found.");

            // Update item statuses based on order type
            string fromStatus, toStatus;
            if (orderType == "Inbound")
            {
                fromStatus = "Pending";
                toStatus = "In Stock";
            }
            else
            {
                fromStatus = "In Stock";
                toStatus = "Dispatched";
            }

            await using var itemsCmd = new MySqlCommand(
                @"UPDATE Item i
                  INNER JOIN OrderItem oi ON i.I_ID = oi.I_ID
                  SET i.Status = @ToStatus
                  WHERE oi.O_ID = @OrderId AND i.Status = @FromStatus",
                conn, transaction);

            itemsCmd.Parameters.AddWithValue("@ToStatus", toStatus);
            itemsCmd.Parameters.AddWithValue("@OrderId", orderId);
            itemsCmd.Parameters.AddWithValue("@FromStatus", fromStatus);
            await itemsCmd.ExecuteNonQueryAsync();

            // Update order status and completion date
            await using var orderCmd = new MySqlCommand(
                @"UPDATE `Order` 
                  SET Status = 'Completed', CompletedDate = NOW()
                  WHERE O_ID = @Id",
                conn, transaction);

            orderCmd.Parameters.AddWithValue("@Id", orderId);
            await orderCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
