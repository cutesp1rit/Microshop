using System;

namespace Microshop.OrderService.Models;

public class ProcessPaymentRequest
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
} 