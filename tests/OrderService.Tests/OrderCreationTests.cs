using Microshop.OrderService.Controllers;
using Microshop.OrderService.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class OrderCreationTests
{
    [Fact]
    public async Task Creates_New_Order()
    {
        var opts = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var ctx = new OrdersDbContext(opts);
        var ctrl = new OrdersController(ctx);

        var dto = new CreateOrderDto(Guid.NewGuid(), 100m, "test");
        var result = await ctrl.Create(dto, default);
        Assert.NotNull(result);
        Assert.Single(ctx.Orders);
        Assert.Single(ctx.Outbox);
    }
} 