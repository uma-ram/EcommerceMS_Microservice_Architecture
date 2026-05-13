using System.ComponentModel.DataAnnotations;

namespace OrderService.Models;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}
public class Order
{
    public int Id { get; set; }
    [Required]
    public int UserId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set;} = DateTime.UtcNow;

    //Navigation Property - one order has many items
    public List<OrderItem> Items { get; set; } = new();
}
