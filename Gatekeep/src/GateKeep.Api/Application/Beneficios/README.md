# 📁 Application/Beneficios

## 🎯 Propósito
**Vertical Slice** para la gestión de beneficios y su asignación a usuarios.

## 📋 Contenido
- **IBeneficioRepository**: Interface para acceso a datos de beneficios
- **IBeneficioService**: Interface para lógica de negocio de beneficios
- **BeneficioService**: Implementación de reglas de negocio
- **IBeneficioUsuarioService**: Interface para gestión de relaciones usuario-beneficio

## 🔧 Función
- Gestionar beneficios del sistema
- Controlar asignación de beneficios a usuarios
- Validar reglas de canje y consumo
- Manejar cupos y vigencia de beneficios

## 📝 Notas
- Maneja relaciones many-to-many con usuarios
- Contiene lógica de validación de beneficios
- Se integra con el slice de Usuarios
- Gestiona estados de canje y consumo
