FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["BBCFinanceBot/BBCFinanceBot.csproj", "BBCFinanceBot/"]
RUN dotnet restore "BBCFinanceBot/BBCFinanceBot.csproj"
COPY . .
WORKDIR "/src/BBCFinanceBot"
RUN dotnet build "BBCFinanceBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BBCFinanceBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BBCFinanceBot.dll"]
