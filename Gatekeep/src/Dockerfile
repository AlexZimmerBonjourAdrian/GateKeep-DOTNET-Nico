# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["GateKeep.Api/GateKeep.Api.csproj", "GateKeep.Api/"]
RUN dotnet restore "GateKeep.Api/GateKeep.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/GateKeep.Api"
RUN dotnet build "GateKeep.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GateKeep.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5011
EXPOSE 5012

# Instalar curl para el health check
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .

# Copiar archivos de configuración desde el contexto de build si existen
COPY --from=build /src/GateKeep.Api/config.Production.json /app/config.Production.json
COPY --from=build /src/GateKeep.Api/appsettings.Production.json /app/appsettings.Production.json
COPY --from=build /src/GateKeep.Api/appsettings.json /app/appsettings.json
COPY --from=build /src/GateKeep.Api/config.json /app/config.json

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5011
ENV ASPNETCORE_ENVIRONMENT=Production

# Usar directamente dotnet en lugar del entrypoint script
ENTRYPOINT ["dotnet", "GateKeep.Api.dll"]
