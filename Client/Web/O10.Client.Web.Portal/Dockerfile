#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_14.x | bash \
    && apt-get install nodejs -yq
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_14.x | bash \
    && apt-get install nodejs -yq
WORKDIR /src
COPY ["Common/O10.Core/O10.Core.csproj", "Common/O10.Core/"]
COPY ["Common/Persistency/O10.Core.Persistency/O10.Core.Persistency.csproj", "Common/Persistency/O10.Core.Persistency/"]
COPY ["Common/O10.Transactions.Core/O10.Transactions.Core.csproj", "Common/O10.Transactions.Core/"]
COPY ["Common/Crypto/O10.Crypto/O10.Crypto.csproj", "Common/Crypto/O10.Crypto/"]
COPY ["Common/Crypto/HashLib/CoreHashLib/CoreHashLib.csproj", "Common/Crypto/HashLib/CoreHashLib/"]
COPY ["Common/Crypto/Chaos.NaCl/Chaos.NaCl.csproj", "Common/Crypto/Chaos.NaCl/"]
COPY ["Common/O10.Tracking.ApplicationInsights/O10.Tracking.ApplicationInsights.csproj", "Common/O10.Tracking.ApplicationInsights/"]
COPY ["Client/Common/O10.Client.Common/O10.Client.Common.csproj", "Client/Common/O10.Client.Common/"]
COPY ["Client/Common/O10.Client.DataLayer/O10.Client.DataLayer.csproj", "Client/Common/O10.Client.DataLayer/"]
COPY ["Client/Common/O10.Client.DataLayer.SQLite/O10.Client.DataLayer.SQLite.csproj", "Client/Common/O10.Client.DataLayer.SQLite/"]
COPY ["Client/Common/O10.Client.DataLayer.SqlServer/O10.Client.DataLayer.SqlServer.csproj", "Client/Common/O10.Client.DataLayer.SqlServer/"]
COPY ["Client/Web/O10.Client.Web.Common/O10.Client.Web.Common.csproj", "Client/Web/O10.Client.Web.Common/"]
COPY ["Client/Web/O10.Client.Web.DataContracts/O10.Client.Web.DataContracts.csproj", "Client/Web/O10.Client.Web.DataContracts/"]
COPY ["Client/Web/O10.Client.Web.Portal/O10.Client.Web.Portal.csproj", "Client/Web/O10.Client.Web.Portal/"]
COPY ["Client/Web/IdentityProvider/O10.Client.Web.IdentityProvider.Common/O10.Client.Web.IdentityProvider.Common.csproj", "Client/Web/IdentityProvider/O10.Client.Web.IdentityProvider.Common/"]
COPY ["Client/Web/IdentityProvider/O10.Client.Web.IdentityProvider.DataLayer/O10.Client.Web.IdentityProvider.DataLayer.csproj", "Client/Web/IdentityProvider/O10.Client.Web.IdentityProvider.DataLayer/"]
COPY ["Client/Web/IdentityProvider/O10.Client.Web.IdentityProvider.DataLayer.SQLite/O10.Client.Web.IdentityProvider.DataLayer.SQLite.csproj", "Client/Web/IdentityProvider/O10.Client.Web.IdentityProvider.DataLayer.SQLite/"]
COPY ["Client/Web/IdentityProvider/O10.Client.Web.IdentityProvider.DataLayer.SqlServer/O10.Client.Web.IdentityProvider.DataLayer.SqlServer.csproj", "Client/Web/IdentityProvider/O10.Client.Web.IdentityProvider.DataLayer.SqlServer/"]
COPY ["Client/Web/Saml/O10.Client.Web.Saml.Common/O10.Client.Web.Saml.Common.csproj", "Client/Web/Saml/O10.Client.Web.Saml.Common/"]
COPY ["Client/Web/Saml/Saml2.Authentication.Core/Saml2.Authentication.Core.csproj", "Client/Web/Saml/Saml2.Authentication.Core/"]
COPY ["Integrations/Rsk/O10.Integrations.Rsk.Web/O10.Integrations.Rsk.Web.csproj", "Integrations/Rsk/O10.Integrations.Rsk.Web/"]
COPY ["Integrations/Rsk/Wrappers/O10Idp/O10Idp.csproj", "Integrations/Rsk/Wrappers/O10Idp/"]
RUN dotnet restore "Client/Web/O10.Client.Web.Portal/O10.Client.Web.Portal.csproj"
COPY . .
WORKDIR "/src/Client/Web/O10.Client.Web.Portal"
RUN dotnet build "O10.Client.Web.Portal.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "O10.Client.Web.Portal.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "O10.Client.Web.Portal.dll"]