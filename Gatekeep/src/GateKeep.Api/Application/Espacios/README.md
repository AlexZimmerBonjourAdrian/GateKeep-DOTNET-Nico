# 📁 Application/Espacios

## 🎯 Propósito
**Vertical Slice** para la gestión de espacios físicos (Edificios, Salones, Laboratorios).

## 📋 Contenido
- **IEspacioRepository**: Interface para acceso a datos de espacios
- **IEspacioService**: Interface para lógica de negocio de espacios
- **EspacioService**: Implementación de reglas de negocio
- **IEspacioFactory**: Interface para creación de diferentes tipos de espacios

## 🔧 Función
- Gestionar diferentes tipos de espacios
- Aplicar reglas específicas por tipo de espacio
- Coordinar operaciones de espacios
- Validar accesos y permisos

## 📝 Notas
- Maneja herencia de espacios (Edificio, Salon, Laboratorio)
- Contiene lógica específica por tipo de espacio
- Se integra con el slice de Acceso
- Es independiente de otros slices
