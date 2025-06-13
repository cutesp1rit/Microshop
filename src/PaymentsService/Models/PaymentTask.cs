using System;
namespace Microshop.PaymentsService.Models;

public class PaymentTask
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public bool Processed { get; set; }
} 