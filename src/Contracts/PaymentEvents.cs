namespace Microshop.Contracts;

public record PaymentTaskMessage(Guid OrderId, Guid UserId, decimal Amount);

public record PaymentStatusEvent(Guid OrderId, PaymentStatus Status);

public enum PaymentStatus { Success, Fail } 