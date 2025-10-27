# 📁 Infrastructure/Notificaciones

## 🎯 Propósito
Implementaciones concretas para el **Vertical Slice de Notificaciones** usando **MongoDB**.

## 📋 Contenido
- **NotificacionRepository**: Implementación del repositorio de notificaciones
- **NotificacionUsuarioRepository**: Implementación del repositorio de relaciones usuario-notificación

## 🔧 Función
- Implementar interfaces del slice de Notificaciones
- Manejar persistencia de datos en MongoDB
- Proporcionar operaciones CRUD específicas
- Optimizar consultas con índices MongoDB

## 🗄️ Implementación MongoDB
- **Driver**: MongoDB.Driver para .NET
- **Colecciones**: 
  - `notificaciones`: Documentos de notificaciones
  - `notificaciones_usuarios`: Relaciones usuario-notificación
- **Atributos BSON**: Mapeo automático de entidades a documentos

## 📝 Notas
- Implementa contratos definidos en Application/Notificaciones
- Puede cambiar implementación sin afectar lógica de negocio
- Contiene detalles técnicos de persistencia MongoDB
- Se registra en DI container
- Usa ObjectId como identificador principal
