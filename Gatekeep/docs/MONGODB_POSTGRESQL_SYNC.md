# Sincronización y Consistencia entre PostgreSQL y MongoDB

## Arquitectura Híbrida

El sistema GateKeep utiliza una arquitectura híbrida que divide los datos entre PostgreSQL y MongoDB:

- **PostgreSQL (EF Core)**: Usuario, Beneficio, Espacio, ReglaAcceso, EventoAcceso, etc.
- **MongoDB (MongoDB.Driver)**: Notificacion, NotificacionUsuario

## Problemas y Soluciones

### 1. Sin Transacciones Distribuidas

**Problema**: No existen transacciones atómicas entre PostgreSQL y MongoDB.

**Solución**: Implementamos patrones de compensación manual y validaciones previas.

### 2. Sin Integridad Referencial Real

**Problema**: `NotificacionUsuario` en MongoDB tiene `UsuarioId`, pero no hay foreign key real.

**Solución**: 
- `INotificacionUsuarioValidationService`: Valida que Usuario existe en PostgreSQL antes de crear relaciones.
- Validaciones en `NotificacionUsuarioRepository` antes de cada operación.

### 3. Consultas Combinadas

**Problema**: No se pueden hacer JOINs entre bases de datos diferentes.

**Solución**: 
- `INotificacionUsuarioService`: Consulta ambas bases y combina datos en memoria.
- Optimización para evitar N+1 usando agrupaciones.

## Servicios Implementados

### Validación de Integridad Referencial

**Interface**: `INotificacionUsuarioValidationService`
**Implementación**: `NotificacionUsuarioValidationService`

```csharp
// Valida que el usuario existe en PostgreSQL
await validationService.ValidarUsuarioExisteAsync(usuarioId);

// Valida que tanto el usuario como la notificación existen
await validationService.ValidarIntegridadReferencialAsync(usuarioId, notificacionId);
```

### Sincronización

**Interface**: `INotificacionSincronizacionService`
**Implementación**: `NotificacionSincronizacionService`

**Métodos**:
- `LimpiarRegistrosHuerfanosAsync`: Elimina registros en MongoDB cuando el usuario no existe en PostgreSQL.
- `ValidarConsistenciaAsync`: Valida consistencia entre ambas bases.
- `SincronizarEliminacionUsuarioAsync`: Sincroniza eliminación de usuario limpiando notificaciones.

**Interface**: `IUsuarioSincronizacionService`
**Implementación**: `UsuarioSincronizacionService`

**Métodos**:
- `EliminarUsuarioCompletoAsync`: Elimina usuario en PostgreSQL y sincroniza con MongoDB.

### Servicios Combinados

**Interface**: `INotificacionUsuarioService`
**Implementación**: `NotificacionUsuarioService`

**Métodos**:
- `ObtenerNotificacionesPorUsuarioAsync`: Obtiene notificaciones combinando datos de PostgreSQL y MongoDB.
- `ObtenerNotificacionCompletaAsync`: Obtiene una notificación específica con datos completos.
- `MarcarComoLeidaAsync`: Marca notificación como leída.
- `ContarNoLeidasAsync`: Cuenta notificaciones no leídas.

### Transacciones de Compensación

**Interface**: `INotificacionTransactionService`
**Implementación**: `NotificacionTransactionService`

**Métodos**:
- `CrearNotificacionUsuarioConCompensacionAsync`: Crea relación con validaciones y logging.
- `EliminarNotificacionUsuarioConCompensacionAsync`: Elimina con registro para posible rollback.

## Ejemplos de Uso

### Obtener Notificaciones de un Usuario

```csharp
var notificaciones = await notificacionUsuarioService
    .ObtenerNotificacionesPorUsuarioAsync(usuarioId);
```

**Proceso**:
1. Valida que el usuario existe en PostgreSQL.
2. Consulta relaciones en MongoDB.
3. Agrupa IDs de notificaciones.
4. Consulta notificaciones completas.
5. Combina datos en memoria.

### Eliminar Usuario Completo

```csharp
await usuarioSincronizacionService.EliminarUsuarioCompletoAsync(usuarioId);
```

**Proceso**:
1. Valida que el usuario existe.
2. Elimina usuario en PostgreSQL.
3. Sincroniza eliminación en MongoDB (limpia notificaciones).

### Validar Consistencia

```csharp
await notificacionSincronizacionService.ValidarConsistenciaAsync(usuarioId);
```

**Proceso**:
1. Consulta usuario en PostgreSQL.
2. Consulta notificaciones en MongoDB.
3. Detecta inconsistencias (usuario no existe pero tiene notificaciones).

## Endpoints API

### Notificaciones de Usuario

- `GET /api/usuarios/{usuarioId}/notificaciones`: Obtener todas las notificaciones.
- `GET /api/usuarios/{usuarioId}/notificaciones/{notificacionId}`: Obtener notificación específica.
- `PUT /api/usuarios/{usuarioId}/notificaciones/{notificacionId}/leer`: Marcar como leída.
- `GET /api/usuarios/{usuarioId}/notificaciones/no-leidas/count`: Contar no leídas.

## Mejores Prácticas

### 1. Siempre Validar Antes de Crear

```csharp
// Antes de crear NotificacionUsuario
await validationService.ValidarIntegridadReferencialAsync(usuarioId, notificacionId);
```

### 2. Sincronizar al Eliminar

```csharp
// Usar servicio de sincronización para eliminar usuario completo
await usuarioSincronizacionService.EliminarUsuarioCompletoAsync(usuarioId);
```

### 3. Optimizar Consultas Combinadas

```csharp
// Agrupar IDs antes de consultar para evitar N+1
var ids = notificacionesUsuario.Select(nu => nu.NotificacionId).Distinct().ToList();
foreach (var id in ids) {
    var notificacion = await repository.ObtenerPorIdAsync(id);
}
```

### 4. Manejar Errores

```csharp
try {
    await notificacionUsuarioService.MarcarComoLeidaAsync(usuarioId, notificacionId);
} catch (InvalidOperationException ex) {
    // Manejar error de validación
}
```

## Consideraciones

### Operaciones No Atómicas

Las operaciones que requieren ambas bases no son atómicas. Si una falla:
- Se registra el error.
- En producción, considerar patrón Saga más robusto.
- Implementar cola de mensajería para sincronización asíncrona.

### Monitoreo

- Monitorear inconsistencias periódicamente.
- Ejecutar `ValidarConsistenciaAsync` en tareas programadas.
- Alertar cuando se detecten registros huérfanos.

### Escalabilidad

- Cachear resultados de consultas combinadas frecuentes.
- Considerar replicación de datos críticos en una sola base si es necesario.
- Usar proyecciones en MongoDB para optimizar consultas.

