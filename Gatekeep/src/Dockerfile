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

# Instalar netcat-openbsd para el health check y wait script
RUN apt-get update && apt-get install -y netcat-openbsd curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .

# Copiar archivos de configuración desde el contexto de build si existen
# Esto garantiza que la imagen contiene config.Production.json y appsettings.* por defecto
COPY --from=build /src/GateKeep.Api/config.Production.json /app/config.Production.json
COPY --from=build /src/GateKeep.Api/appsettings.Production.json /app/appsettings.Production.json
COPY --from=build /src/GateKeep.Api/appsettings.json /app/appsettings.json

# Copiar también config.json (es obligatorio según el error en tiempo de ejecución)
COPY --from=build /src/GateKeep.Api/config.json /app/config.json

# Copiar el script de entrada
COPY docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5011
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]
CMD ["dotnet", "GateKeep.Api.dll"]
