# Tareas Pendientes - Observabilidad y Monitoreo

## Resumen
Este documento lista las tareas pendientes para completar la implementación de observabilidad y monitoreo según los requisitos del proyecto.

## Estado Actual

### ✅ Implementado
- [x] Logging estructurado con Serilog
- [x] Métricas con OpenTelemetry
- [x] Trazas distribuidas con OpenTelemetry
- [x] Correlation ID en solicitudes
- [x] Centralización de logs con Seq
- [x] Dashboard técnico con Grafana
- [x] Percentiles de latencia (P50, P95, P99)
- [x] Tasa de errores por componente
- [x] Asociación de solicitudes mediante CorrelationId

## Tareas Pendientes

### 1. Agregar Métrica de Tiempo Medio de Respuesta

**Descripción:** Agregar un panel en el dashboard de Grafana que muestre el tiempo medio de respuesta de las solicitudes HTTP.

**Archivos a modificar:**
- `src/monitoring/grafana/dashboards/gatekeep-overview.json`

**Implementación:**
- Agregar un nuevo panel que calcule el promedio usando PromQL:
  ```promql
  rate(http_server_requests_seconds_sum{job="gatekeep-api"}[5m]) / rate(http_server_requests_seconds_count{job="gatekeep-api"}[5m]) * 1000
  ```
- El panel debe mostrar el tiempo medio en milisegundos
- Configurar thresholds apropiados (verde < 500ms, amarillo 500-1000ms, rojo > 1000ms)

**Prioridad:** Media

---

### 2. Implementar Métricas de Backlog de Sincronizaciones

**Descripción:** Crear métricas que expongan el número de sincronizaciones pendientes entre PostgreSQL y MongoDB.

**Archivos a crear/modificar:**
- `src/GateKeep.Api/Infrastructure/Observability/ObservabilityService.cs` (extender)
- `src/GateKeep.Api/Infrastructure/Observability/IObservabilityService.cs` (extender)
- `src/GateKeep.Api/Infrastructure/Notificaciones/NotificacionSincronizacionService.cs` (modificar)
- `src/GateKeep.Api/Application/Notificaciones/INotificacionSincronizacionService.cs` (agregar método)

**Implementación:**
1. Agregar método en `IObservabilityService`:
   ```csharp
   void RecordSincronizacionPendiente(string tipo, long cantidad);
   void RecordSincronizacionCompletada(string tipo);
   ```

2. Crear métrica Gauge en `ObservabilityService`:
   ```csharp
   private readonly Gauge<long> _sincronizacionesPendientes;
   ```

3. Implementar lógica para contar sincronizaciones pendientes:
   - Contar registros huérfanos en MongoDB que necesitan sincronización
   - Contar eventos de sincronización en cola (si se implementa cola de mensajería)
   - Actualizar la métrica cuando se detecten o resuelvan sincronizaciones pendientes

4. Llamar a `RecordSincronizacionPendiente` desde `NotificacionSincronizacionService` cuando se detecten inconsistencias

**Métrica a exponer:**
- `gatekeep_sincronizaciones_pendientes` (Gauge) con labels: `tipo` (notificaciones, usuarios, etc.)

**Prioridad:** Alta

---

### 3. Implementar Métricas de Backlog de Eventos

**Descripción:** Crear métricas que expongan el número de eventos pendientes de procesar.

**Archivos a crear/modificar:**
- `src/GateKeep.Api/Infrastructure/Observability/ObservabilityService.cs` (extender)
- `src/GateKeep.Api/Infrastructure/Observability/IObservabilityService.cs` (extender)
- `src/GateKeep.Api/Infrastructure/Events/EventPublisher.cs` (modificar)
- `src/GateKeep.Api/Application/Events/IEventPublisher.cs` (revisar si necesita cambios)

**Implementación:**
1. Agregar método en `IObservabilityService`:
   ```csharp
   void RecordEventoPendiente(string tipoEvento);
   void RecordEventoProcesado(string tipoEvento);
   ```

2. Crear métrica Gauge en `ObservabilityService`:
   ```csharp
   private readonly Gauge<long> _eventosPendientes;
   ```

3. Si hay cola de eventos, contar elementos pendientes
4. Si no hay cola, rastrear eventos que están siendo procesados vs completados

**Métrica a exponer:**
- `gatekeep_eventos_pendientes` (Gauge) con labels: `tipo` (acceso_permitido, acceso_rechazado, etc.)

**Nota:** Si actualmente los eventos se procesan de forma síncrona, considerar implementar una cola de eventos para tener un backlog real.

**Prioridad:** Media-Alta

---

### 4. Agregar Paneles de Backlog al Dashboard de Grafana

**Descripción:** Agregar paneles en el dashboard para visualizar las métricas de backlog.

**Archivos a modificar:**
- `src/monitoring/grafana/dashboards/gatekeep-overview.json`

**Implementación:**
1. Agregar panel "Sincronizaciones Pendientes":
   - Tipo: Gauge o Stat
   - Query: `gatekeep_sincronizaciones_pendientes`
   - Mostrar por tipo de sincronización
   - Configurar alertas si el backlog supera un umbral (ej: > 100)

2. Agregar panel "Eventos Pendientes":
   - Tipo: Gauge o Stat
   - Query: `gatekeep_eventos_pendientes`
   - Mostrar por tipo de evento
   - Configurar alertas si el backlog supera un umbral

3. Agregar panel de tendencia temporal (opcional):
   - Tipo: Time Series
   - Mostrar evolución del backlog en el tiempo

**Prioridad:** Alta (depende de las tareas 2 y 3)

---

### 5. Verificar Exportación de Trazas a Backend de Trazas

**Descripción:** Verificar que las trazas distribuidas se estén exportando correctamente a un backend de trazas (Jaeger, Zipkin, etc.) o considerar agregarlo.

**Estado actual:** Las trazas se generan con OpenTelemetry pero solo se exportan a Prometheus (métricas). No hay exportador de trazas configurado.

**Archivos a modificar:**
- `src/GateKeep.Api/Program.cs`
- `src/GateKeep.Api/GateKeep.Api.csproj` (agregar paquete si es necesario)
- `src/docker-compose.yml` (agregar servicio Jaeger/Zipkin si se implementa)

**Implementación:**
1. Decidir backend de trazas (Jaeger recomendado)
2. Agregar paquete NuGet: `OpenTelemetry.Exporter.Jaeger` o `OpenTelemetry.Exporter.Zipkin`
3. Configurar exportador en `Program.cs`:
   ```csharp
   .WithTracing(tracerProviderBuilder =>
   {
       // ... configuración actual ...
       tracerProviderBuilder.AddJaegerExporter(options =>
       {
           options.AgentHost = "jaeger";
           options.AgentPort = 6831;
       });
   })
   ```
4. Agregar servicio Jaeger en `docker-compose.yml`

**Prioridad:** Baja (las trazas ya se generan, solo falta exportarlas a un backend dedicado)

---

### 6. Documentación de Uso

**Descripción:** Crear documentación sobre cómo usar las herramientas de observabilidad.

**Archivos a crear:**
- `docs/OBSERVABILIDAD_GUIA.md`

**Contenido sugerido:**
- Cómo acceder a Seq (logs centralizados)
- Cómo acceder a Grafana (dashboards)
- Cómo acceder a Prometheus (métricas)
- Cómo usar CorrelationId para rastrear solicitudes
- Cómo interpretar los dashboards
- Cómo configurar alertas

**Prioridad:** Baja

---

## Orden de Implementación Recomendado

1. **Tarea 2:** Implementar métricas de backlog de sincronizaciones (Alta prioridad)
2. **Tarea 3:** Implementar métricas de backlog de eventos (Media-Alta prioridad)
3. **Tarea 4:** Agregar paneles de backlog al dashboard (Alta prioridad, depende de 2 y 3)
4. **Tarea 1:** Agregar métrica de tiempo medio de respuesta (Media prioridad)
5. **Tarea 5:** Verificar/agregar exportación de trazas (Baja prioridad)
6. **Tarea 6:** Documentación (Baja prioridad)

## Notas Adicionales

- Las métricas de backlog requieren implementar lógica para contar elementos pendientes. Esto puede requerir:
  - Consultas a bases de datos para contar registros pendientes
  - Implementación de cola de mensajería si no existe
  - Tracking de estado de sincronizaciones/eventos

- El tiempo medio de respuesta se puede calcular desde las métricas HTTP existentes de OpenTelemetry, no requiere cambios en el código de la aplicación.

- Considerar agregar alertas en Prometheus/Grafana cuando:
  - El backlog de sincronizaciones supere un umbral
  - El backlog de eventos supere un umbral
  - El tiempo medio de respuesta supere un umbral
  - La tasa de errores supere un umbral

