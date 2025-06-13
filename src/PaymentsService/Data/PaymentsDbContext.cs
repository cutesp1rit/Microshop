using Microsoft.EntityFrameworkCore;
using Microshop.PaymentsService.Models;

namespace Microshop.PaymentsService.Data;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<ProcessedPayment> ProcessedPayments { get; set; }
    public DbSet<PaymentTask> Tasks => Set<PaymentTask>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<ProcessedPayment>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<ProcessedPayment>()
            .HasIndex(p => new { p.OrderId, p.UserId })
            .IsUnique();
    }
} 