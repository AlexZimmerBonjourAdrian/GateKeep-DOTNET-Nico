# Script de Pruebas para MongoDB y Notificaciones
# Configuración
$baseUrl = "http://localhost:5011"

Write-Host "=== Pruebas de MongoDB y Notificaciones ===" -ForegroundColor Cyan
Write-Host ""

# 1. Health Check MongoDB
Write-Host "1. Probando Health Check MongoDB..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health/mongodb" -Method Get
    Write-Host "✓ MongoDB Health Check: OK" -ForegroundColor Green
    Write-Host "  Status: $($response.status)" -ForegroundColor Gray
    Write-Host "  Database: $($response.database)" -ForegroundColor Gray
    Write-Host "  Message: $($response.message)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Error en MongoDB Health Check: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 2. Verificar si hay usuarios (necesario para login)
Write-Host "`n2. Verificando disponibilidad de endpoints..." -ForegroundColor Yellow
try {
    $swagger = Invoke-WebRequest -Uri "$baseUrl/swagger" -Method Get -UseBasicParsing
    Write-Host "✓ Swagger disponible" -ForegroundColor Green
    Write-Host "  URL: $baseUrl/swagger" -ForegroundColor Gray
} catch {
    Write-Host "✗ Swagger no disponible: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 3. Intentar login (requiere usuario válido)
Write-Host "`n3. Intentando login..." -ForegroundColor Yellow
Write-Host "  Nota: Necesitas tener un usuario en la base de datos" -ForegroundColor Gray
Write-Host "  Puedes crear uno con: POST $baseUrl/auth/register" -ForegroundColor Gray

$email = Read-Host "  Ingresa email (o presiona Enter para saltar)"
if ([string]::IsNullOrWhiteSpace($email)) {
    Write-Host "  Saltando pruebas que requieren autenticación..." -ForegroundColor Yellow
    $token = $null
} else {
    $password = Read-Host "  Ingresa password" -AsSecureString
    $passwordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))
    
    try {
        $loginBody = @{
            email = $email
            password = $passwordPlain
        } | ConvertTo-Json

        $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
        $token = $loginResponse.token
        $userId = $loginResponse.user.id
        Write-Host "✓ Login exitoso" -ForegroundColor Green
        Write-Host "  Usuario: $($loginResponse.user.email)" -ForegroundColor Gray
        Write-Host "  ID: $userId" -ForegroundColor Gray
        Write-Host "  Rol: $($loginResponse.user.tipoUsuario)" -ForegroundColor Gray
    } catch {
        Write-Host "✗ Error en login: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "  Continuando sin autenticación..." -ForegroundColor Yellow
        $token = $null
    }
}

if ($token) {
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    # 4. Crear notificación
    Write-Host "`n4. Creando notificación..." -ForegroundColor Yellow
    try {
        $notifBody = @{
            mensaje = "Notificación de prueba desde script PowerShell - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
            tipo = "Info"
        } | ConvertTo-Json

        $notifResponse = Invoke-RestMethod -Uri "$baseUrl/api/notificaciones" -Method Post -Body $notifBody -Headers $headers
        $notifId = $notifResponse.id
        Write-Host "✓ Notificación creada" -ForegroundColor Green
        Write-Host "  ID: $notifId" -ForegroundColor Gray
        Write-Host "  Mensaje: $($notifResponse.mensaje)" -ForegroundColor Gray
        Write-Host "  Tipo: $($notifResponse.tipo)" -ForegroundColor Gray
    } catch {
        Write-Host "✗ Error al crear notificación: $($_.Exception.Message)" -ForegroundColor Red
        $notifId = $null
    }

    # 5. Obtener todas las notificaciones
    Write-Host "`n5. Obteniendo todas las notificaciones..." -ForegroundColor Yellow
    try {
        $notifs = Invoke-RestMethod -Uri "$baseUrl/api/notificaciones" -Method Get -Headers $headers
        Write-Host "✓ Notificaciones obtenidas: $($notifs.Count)" -ForegroundColor Green
        if ($notifs.Count -gt 0) {
            Write-Host "  Primera notificación:" -ForegroundColor Gray
            Write-Host "    ID: $($notifs[0].id)" -ForegroundColor Gray
            Write-Host "    Mensaje: $($notifs[0].mensaje)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "✗ Error al obtener notificaciones: $($_.Exception.Message)" -ForegroundColor Red
    }

    # 6. Obtener notificación por ID (si se creó una)
    if ($notifId) {
        Write-Host "`n6. Obteniendo notificación por ID..." -ForegroundColor Yellow
        try {
            $notif = Invoke-RestMethod -Uri "$baseUrl/api/notificaciones/$notifId" -Method Get -Headers $headers
            Write-Host "✓ Notificación obtenida" -ForegroundColor Green
            Write-Host "  Mensaje: $($notif.mensaje)" -ForegroundColor Gray
            Write-Host "  Tipo: $($notif.tipo)" -ForegroundColor Gray
            Write-Host "  Fecha: $($notif.fechaEnvio)" -ForegroundColor Gray
        } catch {
            Write-Host "✗ Error al obtener notificación: $($_.Exception.Message)" -ForegroundColor Red
        }

        # 7. Actualizar notificación
        Write-Host "`n7. Actualizando notificación..." -ForegroundColor Yellow
        try {
            $updateBody = @{
                mensaje = "Notificación ACTUALIZADA desde script - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
                tipo = "Warning"
            } | ConvertTo-Json

            $updated = Invoke-RestMethod -Uri "$baseUrl/api/notificaciones/$notifId" -Method Put -Body $updateBody -Headers $headers
            Write-Host "✓ Notificación actualizada" -ForegroundColor Green
            Write-Host "  Nuevo mensaje: $($updated.mensaje)" -ForegroundColor Gray
            Write-Host "  Nuevo tipo: $($updated.tipo)" -ForegroundColor Gray
        } catch {
            Write-Host "✗ Error al actualizar notificación: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    # 8. Obtener notificaciones del usuario
    Write-Host "`n8. Obteniendo notificaciones del usuario $userId..." -ForegroundColor Yellow
    try {
        $userNotifs = Invoke-RestMethod -Uri "$baseUrl/api/usuarios/$userId/notificaciones" -Method Get -Headers $headers
        Write-Host "✓ Notificaciones del usuario: $($userNotifs.Count)" -ForegroundColor Green
        if ($userNotifs.Count -gt 0) {
            Write-Host "  Primera notificación del usuario:" -ForegroundColor Gray
            Write-Host "    Notificación ID: $($userNotifs[0].notificacionId)" -ForegroundColor Gray
            Write-Host "    Leída: $($userNotifs[0].leido)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "✗ Error al obtener notificaciones del usuario: $($_.Exception.Message)" -ForegroundColor Red
    }

    # 9. Contar notificaciones no leídas
    Write-Host "`n9. Contando notificaciones no leídas..." -ForegroundColor Yellow
    try {
        $count = Invoke-RestMethod -Uri "$baseUrl/api/usuarios/$userId/notificaciones/no-leidas/count" -Method Get -Headers $headers
        Write-Host "✓ Notificaciones no leídas: $($count.count)" -ForegroundColor Green
    } catch {
        Write-Host "✗ Error al contar notificaciones: $($_.Exception.Message)" -ForegroundColor Red
    }

    # 10. Obtener eventos históricos (requiere FuncionarioOrAdmin)
    Write-Host "`n10. Obteniendo eventos históricos..." -ForegroundColor Yellow
    try {
        $eventos = Invoke-RestMethod -Uri "$baseUrl/api/auditoria/eventos?page=1&pageSize=10" -Method Get -Headers $headers
        Write-Host "✓ Eventos históricos obtenidos" -ForegroundColor Green
        Write-Host "  Total eventos: $($eventos.paginacion.totalCount)" -ForegroundColor Gray
        Write-Host "  Eventos en página: $($eventos.eventos.Count)" -ForegroundColor Gray
    } catch {
        Write-Host "✗ Error al obtener eventos (puede requerir rol Funcionario/Admin): $($_.Exception.Message)" -ForegroundColor Yellow
    }

    # 11. Obtener estadísticas de eventos
    Write-Host "`n11. Obteniendo estadísticas de eventos..." -ForegroundColor Yellow
    try {
        $fechaDesde = (Get-Date).AddDays(-30).ToString("yyyy-MM-dd")
        $fechaHasta = (Get-Date).ToString("yyyy-MM-dd")
        $stats = Invoke-RestMethod -Uri "$baseUrl/api/auditoria/eventos/estadisticas?fechaDesde=$fechaDesde&fechaHasta=$fechaHasta" -Method Get -Headers $headers
        Write-Host "✓ Estadísticas obtenidas" -ForegroundColor Green
        Write-Host "  Período: $($stats.fechaDesde) a $($stats.fechaHasta)" -ForegroundColor Gray
        if ($stats.estadisticasPorTipo) {
            foreach ($stat in $stats.estadisticasPorTipo.PSObject.Properties) {
                Write-Host "    $($stat.Name): $($stat.Value)" -ForegroundColor Gray
            }
        } else {
            Write-Host "  No hay estadísticas disponibles" -ForegroundColor Gray
        }
    } catch {
        Write-Host "✗ Error al obtener estadísticas (puede requerir rol Funcionario/Admin): $($_.Exception.Message)" -ForegroundColor Yellow
    }
} else {
    Write-Host "`n--- Pruebas que requieren autenticación omitidas ---" -ForegroundColor Yellow
    Write-Host "Para probar los endpoints de notificaciones, necesitas:" -ForegroundColor Gray
    Write-Host "1. Crear un usuario con: POST $baseUrl/auth/register" -ForegroundColor Gray
    Write-Host "2. O usar un usuario existente para hacer login" -ForegroundColor Gray
}

Write-Host "`n=== Pruebas completadas ===" -ForegroundColor Cyan
Write-Host "`nURLs disponibles:" -ForegroundColor Cyan
Write-Host "  - API: $baseUrl" -ForegroundColor Gray
Write-Host "  - Swagger: $baseUrl/swagger" -ForegroundColor Gray
Write-Host "  - Health: $baseUrl/health" -ForegroundColor Gray
Write-Host "  - MongoDB Health: $baseUrl/health/mongodb" -ForegroundColor Gray

