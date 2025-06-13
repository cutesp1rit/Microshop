using System;
namespace Microshop.PaymentsService.Models;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Queue { get; set; } = string.Empty;
    public byte[] Body { get; set; } = Array.Empty<byte>();
    public bool Processed { get; set; }
} 