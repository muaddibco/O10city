#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Common/O10.Core/O10.Core.csproj", "Common/O10.Core/"]
COPY ["Common/Persistency/O10.Core.Persistency/O10.Core.Persistency.csproj", "Common/Persistency/O10.Core.Persistency/"]
COPY ["Common/O10.Transactions.Core/O10.Transactions.Core.csproj", "Common/O10.Transactions.Core/"]
COPY ["Common/Crypto/O10.Crypto/O10.Crypto.csproj", "Common/Crypto/O10.Crypto/"]
COPY ["Common/Crypto/HashLib/CoreHashLib/CoreHashLib.csproj", "Common/Crypto/HashLib/CoreHashLib/"]
COPY ["Common/Crypto/Chaos.NaCl/Chaos.NaCl.csproj", "Common/Crypto/Chaos.NaCl/"]
COPY ["Common/O10.Tracking.Core/O10.Tracking.Core.csproj", "Common/O10.Tracking.Core/"]
COPY ["Common/O10.Tracking.ApplicationInsights/O10.Tracking.ApplicationInsights.csproj", "Common/O10.Tracking.ApplicationInsights/"]
COPY ["Common/O10.Tracking.HealthCheck/O10.Tracking.HealthCheck.csproj", "Common/O10.Tracking.HealthCheck/"]
COPY ["Gateway/O10.Gateway.DataLayer/O10.Gateway.DataLayer.csproj", "Gateway/O10.Gateway.DataLayer/"]
COPY ["Gateway/O10.Gateway.DataLayer.SQLite/O10.Gateway.DataLayer.SQLite.csproj", "Gateway/O10.Gateway.DataLayer.SQLite/"]
COPY ["Gateway/O10.Gateway.DataLayer.SqlServer/O10.Gateway.DataLayer.SqlServer.csproj", "Gateway/O10.Gateway.DataLayer.SqlServer/"]
COPY ["Gateway/O10.Gateway.WebApp/O10.Gateway.WebApp.csproj", "Gateway/O10.Gateway.WebApp/"]
COPY ["Gateway/O10.Gateway.WebApp.Common/O10.Gateway.WebApp.Common.csproj", "Gateway/O10.Gateway.WebApp.Common/"]
COPY ["Gateway/O10.Gateway.Common/O10.Gateway.Common.csproj", "Gateway/O10.Gateway.Common/"]
RUN dotnet restore "Gateway/O10.Gateway.WebApp/O10.Gateway.WebApp.csproj"
COPY . .
WORKDIR "/src/Gateway/O10.Gateway.WebApp"
RUN dotnet build "O10.Gateway.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "O10.Gateway.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "O10.Gateway.WebApp.dll"]
