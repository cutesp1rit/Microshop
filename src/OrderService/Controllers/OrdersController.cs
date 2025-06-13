using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microshop.OrderService.Data;
using Microshop.OrderService.Models;
using Microshop.Contracts;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Microshop.OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(OrdersDbContext db, IHttpClientFactory httpClientFactory, ILogger<OrdersController> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Amount = request.Amount,
            Description = request.Description,
            Status = OrderStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "ProcessPayment",
                Data = JsonSerializer.Serialize(new
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    Amount = order.Amount
                }),
                CreatedAt = DateTime.UtcNow,
                IsProcessed = false
            };

            _db.OutboxMessages.Add(outboxMessage);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Error creating order");
        }
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid userId)
    {
        var orders = await _db.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        return Ok(order);
    }
}

public class CreateOrderRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
} 