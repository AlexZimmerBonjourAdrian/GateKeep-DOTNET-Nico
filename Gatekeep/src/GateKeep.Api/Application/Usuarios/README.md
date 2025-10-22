# 📁 Application/Usuarios

## 🎯 Propósito
**Vertical Slice** para la gestión completa de usuarios del sistema.

## 📋 Contenido
- **IUsuarioRepository**: Interface para acceso a datos de usuarios
- **IUsuarioService**: Interface para lógica de negocio de usuarios
- **UsuarioService**: Implementación de reglas de negocio
- **IUsuarioValidator**: Interface para validaciones específicas

## 🔧 Función
- Definir contratos para operaciones de usuarios
- Contener lógica de negocio específica de usuarios
- Validar reglas del dominio para usuarios
- Coordinar operaciones complejas de usuarios

## 📝 Notas
- Es un slice independiente y cohesivo
- Puede evolucionar sin afectar otros slices
- Contiene toda la lógica relacionada con usuarios
- Se integra con Infrastructure y Endpoints
