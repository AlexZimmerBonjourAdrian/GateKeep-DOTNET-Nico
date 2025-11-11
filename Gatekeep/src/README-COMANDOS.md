# Comandos Rápidos - GateKeep

## 🚀 Inicio Rápido

### Desarrollo Local
```powershell
cd GateKeep.Api
dotnet run
```

### Con Docker
```powershell
docker-compose up -d
```

---

## 📝 Variables de Entorno

**Archivo:** `.env` (en este directorio)

```bash
# Editar variables
notepad .env

# Verificar que existe
Test-Path ".env"

# Ver contenido
Get-Content ".env"
```

---

## 🐳 Docker - Comandos Esenciales

```powershell
# Levantar todo
docker-compose up -d

# Ver logs en tiempo real
docker-compose logs -f api

# Ver estado
docker-compose ps

# Reiniciar API
docker-compose restart api

# Detener todo
docker-compose down

# Recrear todo (limpio)
docker-compose down && docker-compose build && docker-compose up -d

# Limpiar todo (incluyendo datos)
docker-compose down -v
```

---

## 🔄 Cambiar Puerto

1. Editar `.env`:
   ```env
   GATEKEEP_PORT=5020
   ```

2. Reiniciar:
   ```powershell
   # Docker
   docker-compose down && docker-compose up -d
   
   # Local
   Ctrl+C y dotnet run
   ```

---

## 🗄️ PostgreSQL

### Docker
```powershell
# Conectar
docker exec -it gatekeep-postgres psql -U postgres -d Gatekeep

# Backup
docker exec gatekeep-postgres pg_dump -U postgres Gatekeep > backup.sql

# Restore
docker exec -i gatekeep-postgres psql -U postgres Gatekeep < backup.sql
```

### Local
```powershell
psql -U postgres -h localhost -p 5432 -d Gatekeep
```

---

## 🔍 Diagnóstico

```powershell
# Ver puertos en uso
netstat -ano | findstr :5011

# Limpiar proyecto .NET
dotnet clean
dotnet restore
dotnet build

# Ver variables cargadas (en logs al iniciar)
docker-compose logs api | Select-String "Variables de entorno"
```

---

## 🌐 URLs de Servicios

| Servicio | URL |
|----------|-----|
| API Swagger | http://localhost:5011/swagger |
| Health Check | http://localhost:5011/health |
| Seq (Logs) | http://localhost:5341 |
| Prometheus | http://localhost:9090 |
| Grafana | http://localhost:3001 |

---

## ⚠️ Solución Rápida de Problemas

**Puerto ocupado:**
```powershell
netstat -ano | findstr :5011
taskkill /PID <PID> /F
```

**Variables no cargadas:**
```powershell
# Verificar que .env existe aquí
Test-Path ".env"
# Debe estar en: Gatekeep\src\.env
```

**Base de datos no conecta:**
```powershell
# Ver contraseña configurada
Get-Content ".env" | Select-String "DB_PASSWORD"
```

**Docker no responde:**
```powershell
docker-compose down
# Reiniciar Docker Desktop
docker-compose up -d
```

---

Ver documentación completa en: `../GUIA-INICIO.md`

