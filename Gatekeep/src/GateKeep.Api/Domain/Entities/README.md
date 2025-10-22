# 📁 Domain/Entities

## 🎯 Propósito
Esta carpeta contiene las **entidades de dominio** que representan los objetos principales del negocio.

## 📋 Contenido
- **Usuario**: Entidad principal del sistema con roles y credenciales
- **Espacio**: Clase base abstracta para espacios físicos
- **Beneficio**: Entidad para beneficios del sistema
- **Notificacion**: Entidad para notificaciones
- **ReglaAcceso**: Reglas de acceso a espacios
- **EventoAcceso**: Registro de eventos de acceso

## 🔧 Función
- Representar conceptos del dominio de negocio
- Contener solo datos, sin lógica de aplicación
- Ser inmutables cuando sea posible (records)
- Definir la estructura base del sistema

## 📝 Notas
- Son el corazón del dominio
- No dependen de capas externas
- Pueden tener validaciones básicas
- Se utilizan en toda la aplicación
