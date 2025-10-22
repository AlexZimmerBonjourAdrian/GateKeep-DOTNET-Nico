# 📁 Application/Notificaciones

## 🎯 Propósito
**Vertical Slice** para el sistema de notificaciones y comunicación con usuarios.

## 📋 Contenido
- **INotificacionRepository**: Interface para acceso a datos de notificaciones
- **INotificacionService**: Interface para lógica de negocio de notificaciones
- **NotificacionService**: Implementación de reglas de negocio
- **INotificacionUsuarioService**: Interface para gestión de relaciones usuario-notificación

## 🔧 Función
- Crear y gestionar notificaciones
- Controlar envío de notificaciones a usuarios
- Manejar estados de lectura de notificaciones
- Coordinar diferentes tipos de notificaciones

## 📝 Notas
- Maneja relaciones many-to-many con usuarios
- Contiene lógica de envío y seguimiento
- Se integra con el slice de Usuarios
- Puede implementar Observer Pattern para notificaciones
