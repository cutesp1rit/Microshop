using System;
namespace Microshop.OrderService.Models;

public enum OrderStatus
{
    Created = 0,
    Paid = 1,
    Failed = 2
}

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 