using System.Collections.Concurrent;

namespace Microshop.BuildingBlocks.Messaging;

public class InMemoryMessageBus : IMessageBus
{
    private readonly ConcurrentDictionary<string, List<Func<byte[], Task>>> _handlers = new();

    public Task PublishAsync(string queue, byte[] body, CancellationToken ct = default)
    {
        if (_handlers.TryGetValue(queue, out var handlers))
        {
            foreach (var handler in handlers)
                _ = handler(body); // fire-and-forget
        }
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(string queue, Func<byte[], Task> handler, CancellationToken ct = default)
    {
        _handlers.AddOrUpdate(queue,
            _ => new List<Func<byte[], Task>> { handler },
            (_, list) => { list.Add(handler); return list; });
        return Task.CompletedTask;
    }
} 