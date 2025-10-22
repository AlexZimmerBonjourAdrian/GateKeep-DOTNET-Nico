# 📁 Domain/Enums

## 🎯 Propósito
Esta carpeta contiene las **enumeraciones base** del sistema que definen los valores constantes utilizados en todo el dominio.

## 📋 Contenido
- **TipoCredencial**: Estados de credenciales (Vigente, Revocada, Expirada)
- **Rol**: Roles de usuarios (Funcionario, Estudiante, Admin)
- **TipoBeneficio**: Tipos de beneficios (Canje, Consumo)

## 🔧 Función
- Definir valores fijos del dominio
- Evitar "magic strings" en el código
- Facilitar validaciones y comparaciones
- Centralizar constantes del negocio

## 📝 Notas
- Son la base de todo el sistema
- Se utilizan en entidades, DTOs y lógica de negocio
- No deben contener lógica, solo valores constantes
