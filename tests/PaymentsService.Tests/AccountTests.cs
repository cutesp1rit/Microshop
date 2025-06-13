using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microshop.PaymentsService.Controllers;
using Microshop.PaymentsService.Data;
using Microshop.PaymentsService.Models;
using Xunit;

namespace Microshop.PaymentsService.Tests;

public class AccountTests
{
    private readonly PaymentsDbContext _context;
    private readonly AccountsController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public AccountTests()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(databaseName: "PaymentsTestDb")
            .Options;
        
        _context = new PaymentsDbContext(options);
        _controller = new AccountsController(_context);
    }

    [Fact]
    public async Task CreateAccount_Success()
    {
        var request = new CreateAccountRequest { UserId = _testUserId };
        var result = await _controller.Create(request);
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == _testUserId);
        
        Assert.NotNull(account);
        Assert.Equal(0, account.Balance);
    }

    [Fact]
    public async Task CreateAccount_Duplicate_ReturnsBadRequest()
    {
        var request = new CreateAccountRequest { UserId = _testUserId };
        await _controller.Create(request);
        var result = await _controller.Create(request);
        
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetBalance_Success()
    {
        var account = new Account { UserId = _testUserId, Balance = 100 };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var result = await _controller.Balance(account.Id);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var balance = Assert.IsType<decimal>(okResult.Value.GetType().GetProperty("Balance").GetValue(okResult.Value));
        
        Assert.Equal(100, balance);
    }

    [Fact]
    public async Task GetBalance_NotFound()
    {
        var result = await _controller.Balance(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }
} 