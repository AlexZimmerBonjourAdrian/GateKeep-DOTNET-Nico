# 📁 Application/Acceso

## 🎯 Propósito
**Vertical Slice** para el control de acceso a espacios y gestión de reglas de seguridad.

## 📋 Contenido
- **IReglaAccesoRepository**: Interface para acceso a datos de reglas
- **IAccesoService**: Interface para lógica de control de acceso
- **AccesoService**: Implementación de reglas de acceso
- **IEventoAccesoService**: Interface para registro de eventos de acceso
- **IReglaAccesoStrategy**: Interface para diferentes tipos de validación

## 🔧 Función
- Validar acceso de usuarios a espacios
- Aplicar reglas de horarios y permisos
- Registrar eventos de acceso
- Coordinar diferentes estrategias de validación

## 📝 Notas
- Implementa Strategy Pattern para reglas de acceso
- Se integra con slices de Usuarios y Espacios
- Registra auditoría de accesos
- Contiene lógica de seguridad del sistema
