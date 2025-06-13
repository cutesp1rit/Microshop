FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем только файлы проектов для кэширования
COPY ["src/OrderService/OrderService.csproj", "OrderService/"]
COPY ["src/Contracts/Contracts.csproj", "Contracts/"]
COPY ["src/BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/"]

RUN dotnet restore "OrderService/OrderService.csproj"

# Копируем весь исходный код
COPY . .

WORKDIR "/src/OrderService"
RUN dotnet build "OrderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Устанавливаем dotnet-ef
RUN dotnet tool install --global dotnet-ef

# Создаем скрипт для запуска
RUN echo '#!/bin/bash\n\
dotnet ef database update --no-build --project OrderService.dll\n\
dotnet OrderService.dll\n\
' > /app/entrypoint.sh && chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]