Convenciones:
- Tablas en snake_case plural.
- Enums mapeados a int.
- Herencia TPT (Table Per Type) para `Espacio` y sus subclases (Edificio, Salon, Laboratorio).
- Claves compuestas para tablas de unión.
- Notificacion y NotificacionUsuario solo usan MongoDB (no tienen configuración EF Core en PostgreSQL).

Bases de Datos:
- PostgreSQL (EF Core): Usuario, Beneficio, Espacio, ReglaAcceso, EventoAcceso, etc.
- MongoDB (MongoDB.Driver): Notificacion, NotificacionUsuario

Patrón de Consultas Combinadas:
- Las consultas que requieren datos de ambas bases se hacen en memoria después de consultar cada base por separado.
- Se usa INotificacionUsuarioService para optimizar consultas y evitar N+1.
- Validaciones de integridad referencial se implementan manualmente usando INotificacionUsuarioValidationService.
- Sincronización entre bases se maneja con INotificacionSincronizacionService e IUsuarioSincronizacionService.

Ver documentación completa en: docs/MONGODB_POSTGRESQL_SYNC.md


