# 📁 Endpoints/Shared

## 🎯 Propósito
**Minimal APIs** compartidas utilizadas por múltiples **Vertical Slices**.

## 📋 Contenido
- **HealthEndpoints**: Endpoints de salud del sistema
- **AuthEndpoints**: Endpoints de autenticación global
- **SystemEndpoints**: Endpoints del sistema
- **ErrorHandlingMiddleware**: Middleware de manejo de errores

## 🔧 Función
- Proporcionar endpoints comunes
- Manejar funcionalidades transversales
- Centralizar middleware compartido
- Facilitar mantenimiento

## 📝 Notas
- Contiene endpoints reutilizables
- Se utiliza por múltiples slices
- No debe contener lógica de negocio específica
- Facilita consistencia en toda la API
