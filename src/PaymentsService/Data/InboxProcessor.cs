using Microsoft.EntityFrameworkCore;
using Microshop.BuildingBlocks.Messaging;
using System.Text.Json;
using Microshop.Contracts;
using Microshop.PaymentsService.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microshop.PaymentsService.Data;

public class InboxProcessor : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly IMessageBus _bus;
    private readonly string _queue = QueueNames.PaymentTasks;

    public InboxProcessor(IServiceProvider sp, IMessageBus bus)
    {
        _sp = sp;
        _bus = bus;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _bus.SubscribeAsync(_queue, HandleMessage, stoppingToken);
    }

    private async Task HandleMessage(byte[] body)
    {
        await using var scope = _sp.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var taskMsg = JsonSerializer.Deserialize<PaymentTaskMessage>(body)!;

        // идемпотентность
        if (await db.Tasks.AnyAsync(t => t.OrderId == taskMsg.OrderId)) return;

        await db.Tasks.AddAsync(new PaymentTask
        {
            Id = Guid.NewGuid(),
            OrderId = taskMsg.OrderId,
            UserId = taskMsg.UserId,
            Amount = taskMsg.Amount
        });
        await db.SaveChangesAsync();

        // выполнить задачу сразу (можно вынести в отдельный воркер)
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.UserId == taskMsg.UserId);
        PaymentStatus status;
        if (account == null)
        {
            status = PaymentStatus.Fail;
        }
        else if (account.Balance < taskMsg.Amount)
        {
            status = PaymentStatus.Fail;
        }
        else
        {
            // CAS-подход
            account.Balance -= taskMsg.Amount;
            status = PaymentStatus.Success;
        }
        await db.SaveChangesAsync();

        var ev = new PaymentStatusEvent(taskMsg.OrderId, status);
        var outbox = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Queue = QueueNames.PaymentEvents,
            Body = JsonSerializer.SerializeToUtf8Bytes(ev)
        };
        await db.Outbox.AddAsync(outbox);
        await db.SaveChangesAsync();
    }
} 