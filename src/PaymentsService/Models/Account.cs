using System;
namespace Microshop.PaymentsService.Models;

public class Account
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
} 