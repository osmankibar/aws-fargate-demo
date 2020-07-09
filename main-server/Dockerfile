FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.5-alpine3.12 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY MainServer.csproj .
RUN dotnet restore MainServer.csproj
COPY . .
RUN dotnet build MainServer.csproj -c Debug -o /app

FROM build AS publish
RUN dotnet publish MainServer.csproj -c Debug -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MainServer.dll"]