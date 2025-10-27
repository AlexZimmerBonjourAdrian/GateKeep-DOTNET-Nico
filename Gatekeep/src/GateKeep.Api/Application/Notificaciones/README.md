# 📁 Application/Notificaciones

## 🎯 Propósito
**Vertical Slice** para el sistema de notificaciones usando **MongoDB** como base de datos principal.

## 📋 Contenido
- **INotificacionRepository**: Interface para acceso a datos de notificaciones en MongoDB
- **INotificacionUsuarioRepository**: Interface para gestión de relaciones usuario-notificación
- **INotificacionService**: Interface para lógica de negocio de notificaciones
- **NotificacionService**: Implementación de reglas de negocio

## 🔧 Función
- Crear y gestionar notificaciones en MongoDB
- Controlar envío de notificaciones a usuarios
- Manejar estados de lectura de notificaciones
- Coordinar diferentes tipos de notificaciones

## 🗄️ Base de Datos
- **MongoDB Atlas**: Base de datos principal para notificaciones
- **Colecciones**: 
  - `notificaciones`: Almacena las notificaciones del sistema
  - `notificaciones_usuarios`: Relación many-to-many entre usuarios y notificaciones

## 📝 Notas
- Maneja relaciones many-to-many con usuarios
- Contiene lógica de envío y seguimiento
- Se integra con el slice de Usuarios
- Usa ObjectId de MongoDB para identificadores únicos
- Implementa patrones de arquitectura ECS del proyecto