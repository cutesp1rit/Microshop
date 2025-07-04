FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем только файлы проектов для кэширования
COPY ["src/PaymentsService/PaymentsService.csproj", "PaymentsService/"]
COPY ["src/Contracts/Contracts.csproj", "Contracts/"]
COPY ["src/BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/"]

RUN dotnet restore "PaymentsService/PaymentsService.csproj"

# Копируем весь исходный код
COPY . .

WORKDIR "/src/PaymentsService"
RUN dotnet build "PaymentsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaymentsService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Устанавливаем dotnet-ef
RUN dotnet tool install --global dotnet-ef

# Создаем скрипт для запуска
RUN echo '#!/bin/bash\n\
dotnet ef database update --no-build --project PaymentsService.dll\n\
dotnet PaymentsService.dll\n\
' > /app/entrypoint.sh && chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]