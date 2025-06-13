using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microshop.BuildingBlocks.Messaging;
using Microshop.PaymentsService.Models;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace Microshop.PaymentsService.Data;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _sp;

    public OutboxPublisher(IServiceProvider sp) => _sp = sp;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _sp.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var bus = scope.ServiceProvider.GetRequiredService<Microshop.BuildingBlocks.Messaging.IMessageBus>();

            var msgs = await db.Outbox.Where(o => !o.Processed).Take(20).ToListAsync(stoppingToken);
            foreach (var msg in msgs)
            {
                try
                {
                    await bus.PublishAsync(msg.Queue, msg.Body, stoppingToken);
                    msg.Processed = true;
                }
                catch { /* лог */ }
            }
            await db.SaveChangesAsync(stoppingToken);
            await Task.Delay(500, stoppingToken);
        }
    }
} 