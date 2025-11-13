# Guía de Testing Manual - Mensajería Asíncrona con RabbitMQ

## 📋 Prerequisitos

1. Docker Desktop ejecutándose
2. Todos los servicios levantados con docker-compose
3. Acceso a Swagger UI: `http://localhost:5011/swagger`
4. Acceso a RabbitMQ Management: `http://localhost:15672` (user: guest, pass: guest)
5. Cliente de Redis (opcional): `docker exec -it gatekeep-redis redis-cli`

---

## 🚀 Paso 1: Levantar la Infraestructura

### 1.1 Iniciar todos los servicios

```powershell
cd C:\Users\Felipe\RiderProjects\GateKeep-DOTNET-Nico\Gatekeep\src
docker-compose up -d
```

### 1.2 Verificar que RabbitMQ esté funcionando

```powershell
# Ver logs de RabbitMQ
docker logs gatekeep-rabbitmq

# Debería mostrar: "Server startup complete"
```

### 1.3 Acceder a RabbitMQ Management UI

1. Abrir: `http://localhost:15672`
2. Login: **guest** / **guest**
3. Verificar que el vhost `/` esté disponible

---

## 🧪 Escenario 1: Probar AccesoRechazado (Evento Principal)

### Objetivo
Validar que cuando se rechaza un acceso, se publica un evento a RabbitMQ y se procesa de forma asíncrona.

### 1.1 Crear datos de prueba (si no existen)

#### Crear un Usuario
```http
POST http://localhost:5011/api/usuarios
Content-Type: application/json

{
  "nombre": "Juan",
  "apellido": "Pérez",
  "email": "juan.perez.test@test.com",
  "password": "juan123",
  "rol": "Estudiante",
  "carrera": "Ingeniería",
  "rut": "12345678-9"
}
```

**Respuesta esperada:** Status 201, guardar el `id` del usuario creado.

#### Crear un Espacio
```http
POST http://localhost:5011/api/espacios
Content-Type: application/json

{
  "nombre": "Laboratorio de Computación",
  "descripcion": "Laboratorio para prácticas",
  "capacidad": 30,
  "tipo": "Laboratorio",
  "estado": "Activo"
}
```

**Respuesta esperada:** Status 201, guardar el `id` del espacio.

#### Crear una Regla de Acceso Restrictiva
```http
POST http://localhost:5011/api/reglas-acceso
Content-Type: application/json

{
  "espacioId": 1,
  "rolesPermitidos": [1, 2],
  "vigenciaApertura": "2025-11-13T00:00:00Z",
  "vigenciaCierre": "2025-11-13T23:59:59Z",
  "horarioApertura": "2025-11-13T10:00:00Z",
  "horarioCierre": "2025-11-13T20:00:00Z"
}
```

**Nota:** Esta regla NO permite `Estudiante`, por lo que se rechazará el acceso.

### 1.2 Preparar el monitoreo ANTES de la prueba

#### Opción A: RabbitMQ Management UI
1. Ir a `http://localhost:15672/#/queues`
2. Observar las colas (aún no deberían existir)

#### Opción B: Logs en tiempo real
```powershell
# Terminal 1: Logs de la API
docker logs -f gatekeep-api

# Terminal 2: Logs de RabbitMQ
docker logs -f gatekeep-rabbitmq
```

### 1.3 Intentar validar acceso (debe ser RECHAZADO)

```http
POST http://localhost:5011/api/acceso/validar
Content-Type: application/json

{
  "usuarioId": <ID_USUARIO>,
  "espacioId": <ID_ESPACIO>,
  "puntoControl": "Entrada Principal"
}
```

### 1.4 Verificaciones

#### ✅ Verificación 1: Respuesta de la API
**Respuesta esperada:**
```json
{
  "permitido": false,
  "razon": "El rol del usuario (Estudiante) no tiene permiso...",
  "tipoError": "RolNoPermitido",
  "detallesAdicionales": {
    "usuarioId": <ID>,
    "rolUsuario": "Estudiante",
    "espacioId": <ID>,
    "rolesPermitidos": ["Docente", "Administrativo"]
  }
}
```

#### ✅ Verificación 2: Logs de la API
Buscar en los logs:
```
[INFO] Evento AccesoRechazado publicado a RabbitMQ - Usuario: X, Espacio: Y
[INFO] Procesando AccesoRechazadoEvent - EventId: xxx, UsuarioId: X
[INFO] Notificación creada para usuario X por acceso rechazado
[INFO] AccesoRechazadoEvent procesado exitosamente
```

#### ✅ Verificación 3: RabbitMQ Management UI
1. Ir a **Queues** (`http://localhost:15672/#/queues`)
2. Deberías ver la cola: `acceso-rechazado-queue`
3. **Ready**: 0 (ya fue consumido)
4. **Total**: 1+ (contador de mensajes procesados)

#### ✅ Verificación 4: Verificar que se creó la notificación
```http
GET http://localhost:5011/api/notificaciones/usuario/<ID_USUARIO>
```

Deberías ver una notificación con el mensaje de rechazo.

#### ✅ Verificación 5: Verificar idempotencia en Redis
```powershell
# Conectar a Redis
docker exec -it gatekeep-redis redis-cli

# Buscar clave de idempotencia
KEYS idempotency:*

# Ver detalles (copiar la clave completa del resultado anterior)
GET idempotency:acceso-rechazado-<USUARIO_ID>-<ESPACIO_ID>-entrada-principal-<TIMESTAMP>
TTL idempotency:acceso-rechazado-<USUARIO_ID>-<ESPACIO_ID>-entrada-principal-<TIMESTAMP>
```

**Resultado esperado:** 
- La clave existe
- TTL es aproximadamente 604800 segundos (7 días)

---

## 🔄 Escenario 2: Probar Idempotencia (Mensaje Duplicado)

### Objetivo
Verificar que si el mismo mensaje se procesa dos veces, la segunda vez se ignora.

### 2.1 Forzar un procesamiento duplicado

**Método 1: Reintentar la misma validación dentro del mismo segundo**

Ejecutar 3 veces rápidamente (en menos de 1 segundo):
```http
POST http://localhost:5011/api/acceso/validar
Content-Type: application/json

{
  "usuarioId": <ID_USUARIO>,
  "espacioId": <ID_ESPACIO>,
  "puntoControl": "Entrada Principal"
}
```

### 2.2 Verificaciones

#### ✅ Verificación: Logs del consumidor
Deberías ver en los logs:
```
[INFO] Procesando AccesoRechazadoEvent - EventId: xxx
[WARN] Mensaje duplicado detectado - IdempotencyKey: xxx. Ignorando.
```

#### ✅ Verificación: Solo UNA notificación creada
```http
GET http://localhost:5011/api/notificaciones/usuario/<ID_USUARIO>
```

Aunque enviaste 3 requests, solo debería haber **1 notificación** (o máximo 2 si las requests fueron en segundos diferentes).

---

## ⚠️ Escenario 3: Probar Reintentos con Backoff

### Objetivo
Simular un error en el consumidor para verificar que los reintentos funcionen.

### 3.1 Preparación: Apagar Redis temporalmente

```powershell
# Detener Redis para causar error en el consumidor
docker stop gatekeep-redis
```

### 3.2 Enviar request de validación

```http
POST http://localhost:5011/api/acceso/validar
Content-Type: application/json

{
  "usuarioId": <ID_USUARIO>,
  "espacioId": <ID_ESPACIO>,
  "puntoControl": "Test Reintentos"
}
```

### 3.3 Observar los reintentos en logs

Deberías ver en los logs de la API:
```
[ERROR] Error procesando AccesoRechazadoEvent
[INFO] Retry 1 in 5 seconds...
[ERROR] Error procesando AccesoRechazadoEvent
[INFO] Retry 2 in 15 seconds...
[ERROR] Error procesando AccesoRechazadoEvent
[INFO] Retry 3 in 25 seconds...
```

### 3.4 Verificar en RabbitMQ Management UI

1. Ir a **Queues** > `acceso-rechazado-queue`
2. Ver la métrica **Redelivered** aumentar

### 3.5 Restaurar Redis

```powershell
# Reiniciar Redis
docker start gatekeep-redis
```

### 3.6 Verificación final

- El mensaje eventualmente debería procesarse exitosamente
- O ir a la DLQ después de 3 intentos fallidos

---

## 💀 Escenario 4: Probar Dead Letter Queue (DLQ)

### Objetivo
Verificar que los mensajes que fallan después de todos los reintentos van a la DLQ.

### 4.1 Forzar múltiples fallos

1. Detener Redis: `docker stop gatekeep-redis`
2. Enviar validación de acceso rechazado
3. Esperar ~45 segundos (todos los reintentos)
4. NO reiniciar Redis aún

### 4.2 Verificar mensaje en DLQ

1. Ir a RabbitMQ UI: `http://localhost:15672/#/queues`
2. Buscar la cola: `acceso-rechazado-queue-dlq`
3. Deberías ver **1 mensaje** en Ready

### 4.3 Inspeccionar el mensaje en DLQ

1. Hacer clic en `acceso-rechazado-queue-dlq`
2. Ir a la pestaña **"Get messages"**
3. Configurar: **Ack Mode: Nack message requeue false**, **Messages: 1**
4. Click **"Get Message(s)"**

Verás el contenido del mensaje y los headers con información del error.

### 4.4 Reprocessar desde DLQ (opcional)

Una vez corregido el problema (Redis funcionando):

1. Ir a la DLQ
2. Click en **"Move messages"**
3. Destination queue: `acceso-rechazado-queue`
4. El mensaje se reprocesará correctamente

### 4.5 Limpiar

```powershell
# Reiniciar Redis
docker start gatekeep-redis
```

---

## 🎁 Escenario 5: Probar BeneficioCanjeado (Evento Secundario)

### Objetivo
Validar el segundo evento de negocio implementado.

### 5.1 Crear un beneficio (si no existe)

```http
POST http://localhost:5011/api/beneficios
Content-Type: application/json

{
  "nombre": "Descuento Cafetería 10%",
  "descripcion": "10% de descuento en compras en cafetería",
  "puntosRequeridos": 100,
  "tipo": "Descuento",
  "estado": "Activo"
}
```

### 5.2 Publicar evento manualmente (simulado)

Como el canje de beneficios puede no estar completamente implementado, puedes probar el consumidor directamente publicando un mensaje desde RabbitMQ UI:

1. Ir a: `http://localhost:15672/#/queues/%2F/beneficio-canjeado-queue`
2. Click en **"Publish message"**
3. En **Payload**, poner:

```json
{
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "timestamp": "2025-11-13T12:00:00Z",
  "usuarioId": <ID_USUARIO>,
  "beneficioId": <ID_BENEFICIO>,
  "nombreBeneficio": "Descuento Cafetería 10%",
  "puntoControl": "Sistema Web",
  "puntosCanjeados": 100,
  "idempotencyKey": "beneficio-canjeado-<ID_USUARIO>-<ID_BENEFICIO>-20251113120000"
}
```

4. Click **"Publish message"**

### 5.3 Verificaciones

#### ✅ Verificación: Logs
```
[INFO] Procesando BeneficioCanjeadoEvent - EventId: xxx
[INFO] Notificación de canje enviada a usuario X
[INFO] BeneficioCanjeadoEvent procesado exitosamente
```

#### ✅ Verificación: Notificación creada
```http
GET http://localhost:5011/api/notificaciones/usuario/<ID_USUARIO>
```

Deberías ver la notificación de canje exitoso.

---

## 📊 Monitoreo y Métricas

### Ver estado general de RabbitMQ

1. **Overview**: `http://localhost:15672/#/`
   - Message rates
   - Connections
   - Channels

2. **Queues**: `http://localhost:15672/#/queues`
   - acceso-rechazado-queue
   - acceso-rechazado-queue-dlq
   - beneficio-canjeado-queue
   - beneficio-canjeado-queue-dlq

3. **Exchanges**: `http://localhost:15672/#/exchanges`
   - Exchanges automáticos creados por MassTransit

### Métricas clave a observar

| Métrica | Descripción | Valor esperado |
|---------|-------------|----------------|
| **Ready** | Mensajes esperando ser consumidos | 0 (en estado normal) |
| **Unacked** | Mensajes siendo procesados | 0-8 (según carga) |
| **Total** | Total de mensajes procesados | Incrementa con cada evento |
| **Publish rate** | Mensajes/segundo publicados | Variable según carga |
| **Consumer rate** | Mensajes/segundo consumidos | Similar a publish rate |
| **Redelivered** | Mensajes reintentados | 0 (si no hay errores) |

---

## 🐛 Troubleshooting

### Problema: No se crean las colas en RabbitMQ

**Causa:** La API no se conectó correctamente a RabbitMQ.

**Solución:**
```powershell
# Verificar logs de la API
docker logs gatekeep-api | Select-String "RabbitMQ"

# Debería mostrar:
# [INFO] Configurando RabbitMQ - Host: rabbitmq:5672

# Reiniciar la API
docker restart gatekeep-api
```

### Problema: Mensajes no se consumen

**Causa:** Consumidores no están registrados o hay error de conexión.

**Solución:**
1. Verificar en RabbitMQ UI > Queues > (click en la cola) > **Consumers**
2. Debería mostrar "1" consumidor activo
3. Si no hay consumidores, reiniciar la API

### Problema: Todos los mensajes van a DLQ

**Causa:** Error en el consumidor que no permite procesamiento.

**Solución:**
1. Verificar logs: `docker logs gatekeep-api | Select-String "Error procesando"`
2. Verificar que Redis esté corriendo: `docker ps | Select-String redis`
3. Corregir el problema y mover mensajes de DLQ a la cola principal

### Problema: No se crean notificaciones

**Causa:** Servicio de notificaciones tiene error.

**Solución:**
1. Verificar logs del consumidor
2. El flujo debería continuar aunque falle la notificación (no es crítico)

---

## ✅ Checklist de Validación Final

Marca cada item al completarlo:

- [ ] RabbitMQ levantado y accesible en puerto 15672
- [ ] Colas creadas automáticamente al iniciar la API
- [ ] Evento AccesoRechazado se publica correctamente
- [ ] Consumidor procesa el evento y crea notificación
- [ ] Idempotencia funciona (mensajes duplicados se ignoran)
- [ ] Reintentos con backoff exponencial funcionan (5s, 15s, 25s)
- [ ] Dead Letter Queue recibe mensajes fallidos después de 3 reintentos
- [ ] Evento BeneficioCanjeado funciona (si aplica)
- [ ] Redis almacena claves de idempotencia con TTL de 7 días
- [ ] Logs muestran información clara del flujo

---

## 📈 Prueba de Carga (Opcional)

Para validar el comportamiento bajo carga:

```powershell
# Usar PowerShell para enviar 50 requests
1..50 | ForEach-Object -Parallel {
    $body = @{
        usuarioId = <ID_USUARIO>
        espacioId = <ID_ESPACIO>
        puntoControl = "Test Carga $_"
    } | ConvertTo-Json
    
    Invoke-RestMethod -Uri "http://localhost:5011/api/acceso/validar" `
        -Method Post -Body $body -ContentType "application/json"
} -ThrottleLimit 10
```

Observar en RabbitMQ UI:
- Los mensajes se procesan de forma ordenada
- Prefetch de 16 y concurrency de 8 limitan procesamiento paralelo
- No hay pérdida de mensajes

---

## 📝 Notas Finales

1. **Logs son tu mejor amigo**: Siempre revisa logs de la API para entender el flujo
2. **RabbitMQ UI es esencial**: Usa la interfaz web para monitorear en tiempo real
3. **Redis es crítico**: La idempotencia depende de Redis, asegúrate que esté funcionando
4. **DLQ es para análisis**: No es un error que haya mensajes en DLQ, es el comportamiento esperado cuando algo falla persistentemente

---

## 🎯 Objetivo Cumplido

Si completaste todos los escenarios exitosamente, has validado:

✅ **Evento de negocio**: AccesoRechazado publicado y procesado  
✅ **Reintentos con backoff**: 3 intentos con intervalos exponenciales  
✅ **Dead Letter Queue**: Mensajes fallidos van a DLQ después de reintentos  
✅ **Idempotencia**: Mensajes duplicados se ignoran correctamente  

**¡Felicitaciones! La implementación de mensajería asíncrona con RabbitMQ está completa y funcional. 🎉**
