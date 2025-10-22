# 📁 Infrastructure/Espacios

## 🎯 Propósito
Implementaciones concretas para el **Vertical Slice de Espacios**.

## 📋 Contenido
- **EspacioMemoryRepository**: Implementación en memoria del repositorio
- **EspacioSqlRepository**: Implementación con base de datos SQL
- **EspacioFactory**: Factory para crear diferentes tipos de espacios
- **EspacioCacheService**: Servicio de cache para espacios

## 🔧 Función
- Implementar interfaces del slice de Espacios
- Manejar persistencia de datos de espacios
- Crear instancias de diferentes tipos de espacios
- Optimizar acceso a datos con cache

## 📝 Notas
- Implementa contratos definidos en Application/Espacios
- Maneja herencia de espacios (Edificio, Salon, Laboratorio)
- Contiene Factory Pattern para creación de espacios
- Se registra en DI container
