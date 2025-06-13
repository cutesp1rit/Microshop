using Microsoft.EntityFrameworkCore;
using Microshop.BuildingBlocks.Messaging;
using Microshop.OrderService.Data;
using Microshop.Contracts;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microshop.OrderService.Services;
using System;

namespace Microshop.OrderService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var cs = builder.Configuration.GetConnectionString("Default");
        builder.Services.AddDbContext<OrdersDbContext>(o => o.UseNpgsql(cs));

        builder.Services.AddSingleton<IMessageBus, InMemoryMessageBus>();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderService", Version = "v1" });
        });

        // Добавляем HttpClient для PaymentsService
        builder.Services.AddHttpClient("PaymentsService", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5001/");
        });

        // Регистрируем OutboxProcessor как фоновый сервис
        builder.Services.AddHostedService<OutboxProcessor>();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
} 