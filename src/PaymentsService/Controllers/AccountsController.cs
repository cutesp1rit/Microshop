using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microshop.PaymentsService.Data;
using Microshop.PaymentsService.Models;
using System;
using System.Threading.Tasks;

namespace Microshop.PaymentsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly PaymentsDbContext _db;

    public AccountsController(PaymentsDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request)
    {
        var existingAccount = await _db.Accounts.FirstOrDefaultAsync(a => a.UserId == request.UserId);
        if (existingAccount != null)
            return BadRequest("Account already exists");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Balance = 0
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        return Ok(account);
    }

    [HttpPost("{id}/topup")]
    public async Task<IActionResult> TopUp(Guid id, [FromBody] TopUpRequest request)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var account = await _db.Accounts.FindAsync(id);
            if (account == null)
                return NotFound();

            account.Balance += request.Amount;
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(account);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpGet("{id}/balance")]
    public async Task<IActionResult> Balance(Guid id)
    {
        var account = await _db.Accounts.FindAsync(id);
        if (account == null)
            return NotFound();

        return Ok(new { Balance = account.Balance });
    }

    [HttpPost("process-payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // Проверяем, не был ли платеж уже обработан
            var existingPayment = await _db.ProcessedPayments
                .FirstOrDefaultAsync(p => p.OrderId == request.OrderId && p.UserId == request.UserId);

            if (existingPayment != null)
            {
                await transaction.CommitAsync();
                return Ok(new { Message = "Payment already processed" });
            }

            var account = await _db.Accounts.FirstOrDefaultAsync(a => a.UserId == request.UserId);
            if (account == null)
                return NotFound("Account not found");

            if (account.Balance < request.Amount)
                return BadRequest("Insufficient funds");

            // Атомарно обновляем баланс
            account.Balance -= request.Amount;

            // Записываем информацию об обработанном платеже
            var processedPayment = new ProcessedPayment
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                UserId = request.UserId,
                Amount = request.Amount,
                ProcessedAt = DateTime.UtcNow
            };

            _db.ProcessedPayments.Add(processedPayment);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Message = "Payment processed successfully" });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

public class CreateAccountRequest
{
    public Guid UserId { get; set; }
}

public class TopUpRequest
{
    public decimal Amount { get; set; }
}

public class ProcessPaymentRequest
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
} 