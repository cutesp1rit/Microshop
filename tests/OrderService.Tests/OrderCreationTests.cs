using Microsoft.EntityFrameworkCore;
using Microshop.OrderService.Controllers;
using Microshop.OrderService.Data;
using Microshop.OrderService.Models;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Microshop.OrderService.Tests;

public class OrderCreationTests
{
    private readonly OrdersDbContext _context;
    private readonly OrdersController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public OrderCreationTests()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(databaseName: "OrdersTestDb")
            .Options;
        
        _context = new OrdersDbContext(options);
        var logger = new LoggerFactory().CreateLogger<OrdersController>();
        _controller = new OrdersController(_context, null, logger);
    }

    [Fact]
    public async Task GetOrderStatus_Success()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Amount = 100,
            Description = "Test order",
            Status = OrderStatus.Created
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var result = await _controller.GetStatus(order.Id);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var orderResult = Assert.IsType<Order>(okResult.Value);

        Assert.NotNull(orderResult);
        Assert.Equal(OrderStatus.Created, orderResult.Status);
    }
}