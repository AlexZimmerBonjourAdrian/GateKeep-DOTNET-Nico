# 📁 Contracts/Shared

## 🎯 Propósito
**DTOs (Data Transfer Objects)** compartidos utilizados por múltiples **Vertical Slices**.

## 📋 Contenido
- **BaseResponse**: DTO base para respuestas
- **ErrorResponse**: DTO para respuestas de error
- **PaginationRequest**: DTO para paginación
- **PaginationResponse**: DTO para respuestas paginadas
- **ValidationErrorResponse**: DTO para errores de validación

## 🔧 Función
- Proporcionar DTOs comunes
- Evitar duplicación de código
- Centralizar estructuras compartidas
- Facilitar mantenimiento

## 📝 Notas
- Contiene DTOs reutilizables
- Se utiliza por múltiples slices
- No debe contener lógica de negocio específica
- Facilita consistencia en toda la API
