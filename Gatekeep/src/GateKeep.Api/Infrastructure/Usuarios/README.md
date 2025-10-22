# 📁 Infrastructure/Usuarios

## 🎯 Propósito
Implementaciones concretas para el **Vertical Slice de Usuarios**.

## 📋 Contenido
- **UsuarioMemoryRepository**: Implementación en memoria del repositorio
- **UsuarioSqlRepository**: Implementación con base de datos SQL
- **UsuarioCacheService**: Servicio de cache para usuarios
- **UsuarioEmailService**: Servicio de envío de emails

## 🔧 Función
- Implementar interfaces del slice de Usuarios
- Manejar persistencia de datos de usuarios
- Proporcionar servicios técnicos específicos
- Optimizar acceso a datos con cache

## 📝 Notas
- Implementa contratos definidos en Application/Usuarios
- Puede cambiar implementación sin afectar lógica de negocio
- Contiene detalles técnicos de persistencia
- Se registra en DI container
