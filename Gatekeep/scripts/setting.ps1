# Configuración básica de variables de entorno para GateKeep
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5011"

# Cadena de conexión a PostgreSQL para EF Core (toma variables si existen)
if (-not $env:DB_HOST) { $env:DB_HOST = "localhost" }
if (-not $env:DB_PORT) { $env:DB_PORT = "5432" }
if (-not $env:DB_NAME) { $env:DB_NAME = "Gatekeep" }
if (-not $env:DB_USER) { $env:DB_USER = "postgres" }
if (-not $env:DB_PASS) { $env:DB_PASS = "897888fg2" }

$env:ConnectionStrings__Postgres = "Host=$($env:DB_HOST);Port=$($env:DB_PORT);Database=$($env:DB_NAME);Username=$($env:DB_USER);Password=$($env:DB_PASS)"

Write-Host "=== Configuración de GateKeep ===" -ForegroundColor Green
Write-Host "ASPNETCORE_ENVIRONMENT: $env:ASPNETCORE_ENVIRONMENT"
Write-Host "ASPNETCORE_URLS: $env:ASPNETCORE_URLS"
Write-Host "ConnectionStrings__Postgres: (establecida) DB=$($env:DB_NAME), User=$($env:DB_USER), Host=$($env:DB_HOST), Port=$($env:DB_PORT)"
