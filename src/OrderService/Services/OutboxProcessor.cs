using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microshop.OrderService.Data;
using Microshop.OrderService.Models;
using System.Linq;
using System.Net.Http.Json;

namespace Microshop.OrderService.Services;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private const int MaxRetries = 3;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger,
        IHttpClientFactory httpClientFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessages();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessages()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        var httpClient = _httpClientFactory.CreateClient("PaymentsService");

        var messages = await dbContext.OutboxMessages
            .Where(m => !m.IsProcessed && m.RetryCount < MaxRetries)
            .OrderBy(m => m.CreatedAt)
            .Take(10)
            .ToListAsync();

        foreach (var message in messages)
        {
            try
            {
                if (message.Type == "ProcessPayment")
                {
                    var paymentData = JsonSerializer.Deserialize<ProcessPaymentRequest>(message.Data);
                    var response = await httpClient.PostAsJsonAsync("/api/Accounts/process-payment", paymentData);

                    if (response.IsSuccessStatusCode)
                    {
                        message.IsProcessed = true;
                        message.ProcessedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        message.RetryCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId}", message.Id);
                message.RetryCount++;
            }
        }

        await dbContext.SaveChangesAsync();
    }
}

public class ProcessPaymentRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
} 