# Microshop

Микросервисная архитектура для интернет-магазина, состоящая из двух основных сервисов:
- Payments Service - управление счетами и платежами
- Orders Service - управление заказами

## Архитектура

### Payments Service
- Создание счета пользователя
- Пополнение счета
- Просмотр баланса
- Обработка платежей за заказы

### Orders Service
- Создание заказа
- Просмотр списка заказов
- Просмотр статуса заказа
- Асинхронная обработка оплаты заказа

## Технологии
- .NET 8
- PostgreSQL
- Entity Framework Core
- Swagger/OpenAPI

## Запуск проекта

1. Запустить PostgreSQL:
```bash
docker run --name postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_USER=postgres -e POSTGRES_DB=orders -p 5432:5432 -d postgres:latest
```

2. Запустить Payments Service:
```bash
dotnet run --project src/PaymentsService/PaymentsService.csproj --urls=http://localhost:5001
```

3. Запустить Orders Service:
```bash
dotnet run --project src/OrderService/OrderService.csproj --urls=http://localhost:5002
```

## API Documentation

- Payments Service Swagger UI: http://localhost:5001/swagger
- Orders Service Swagger UI: http://localhost:5002/swagger

---

Минимальные комментарии на русском добавлены прямо в код. 

## Изменения

1. Добавлена зависимость `RabbitMQ.Client` в проект `BuildingBlocks`.

---

**Выполню команду:**
```sh
dotnet add src/BuildingBlocks/BuildingBlocks.csproj package RabbitMQ.Client --version 6.9.0
```
Затем снова попробую сборку. Продолжаю? 