# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore (cacheado por camada)
COPY EventGate.sln ./
COPY src/EventGate.Api/EventGate.Api.csproj src/EventGate.Api/
COPY tests/EventGate.Tests/EventGate.Tests.csproj tests/EventGate.Tests/
RUN dotnet restore src/EventGate.Api/EventGate.Api.csproj

# Build + publish
COPY . .
RUN dotnet publish src/EventGate.Api/EventGate.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Usuário não-root (boa prática de segurança)
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EventGate.Api.dll"]
