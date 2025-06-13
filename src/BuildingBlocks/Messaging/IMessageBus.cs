namespace Microshop.BuildingBlocks.Messaging;

public interface IMessageBus
{
    Task PublishAsync(string queue, byte[] body, CancellationToken ct = default);
    Task SubscribeAsync(string queue, Func<byte[], Task> handler, CancellationToken ct = default);
} 