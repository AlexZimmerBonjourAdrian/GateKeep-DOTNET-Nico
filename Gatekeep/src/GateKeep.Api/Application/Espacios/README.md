# 📁 Application/Espacios

## 🎯 Propósito
**Vertical Slice** para la gestión de espacios físicos (Edificios, Salones, Laboratorios) implementando el **Factory Pattern**.

## 📋 Contenido
- **IEspacioRepository**: Interface para acceso a datos de espacios
- **IEspacioFactory**: Interface para creación de diferentes tipos de espacios
- **EspacioFactory**: Implementación del Factory Pattern para espacios
- **EspacioRepository**: Implementación del repositorio usando Entity Framework

## 🏭 Factory Pattern Implementado

### Características del Factory:
- ✅ **Creación tipada**: Métodos específicos para cada tipo de espacio
- ✅ **Validaciones**: Reglas de negocio específicas por tipo
- ✅ **Flexibilidad**: Método genérico TryCrear para casos dinámicos
- ✅ **Herencia**: Respeta la jerarquía Espacio → Edificio/Salón/Laboratorio

### Métodos del Factory:

#### 1. Creación Específica
```csharp
// Crear edificio
var edificio = await factory.CrearEdificioAsync(request);

// Crear salón
var salon = await factory.CrearSalonAsync(request);

// Crear laboratorio
var laboratorio = await factory.CrearLaboratorioAsync(request);
```

#### 2. Creación Genérica
```csharp
// Crear espacio dinámicamente
if (factory.TryCrear("edificio", request, out var espacioTask))
{
    var espacio = await espacioTask;
}
```

#### 3. Validación de Tipos
```csharp
// Verificar si un tipo es válido
bool esValido = factory.EsTipoValido("edificio"); // true
bool esValido = factory.EsTipoValido("aula");     // false
```

## 🔧 Validaciones Implementadas

### Para Edificios:
- ✅ Código de edificio único
- ✅ Número de pisos mayor a 0
- ✅ Capacidad mayor a 0

### Para Salones:
- ✅ Edificio padre debe existir
- ✅ Número de salón único por edificio
- ✅ Capacidad mayor a 0

### Para Laboratorios:
- ✅ Edificio padre debe existir
- ✅ Número de laboratorio único por edificio
- ✅ Capacidad mayor a 0

## 🚀 Endpoints Disponibles

### Creación Específica:
- `POST /espacios/edificio` - Crear edificio
- `POST /espacios/salon` - Crear salón
- `POST /espacios/laboratorio` - Crear laboratorio

### Creación Genérica:
- `POST /espacios` - Crear espacio (auto-detecta tipo)

### Consultas:
- `GET /espacios` - Obtener todos los espacios
- `GET /espacios/{id}` - Obtener espacio por ID
- `DELETE /espacios/{id}` - Eliminar espacio

## 📝 Ejemplos de Uso

### Crear un Edificio:
```json
POST /espacios/edificio
{
  "nombre": "Edificio Central",
  "descripcion": "Edificio principal del campus",
  "ubicacion": "Campus Norte",
  "capacidad": 500,
  "numeroPisos": 5,
  "codigoEdificio": "EC001",
  "activo": true
}
```

### Crear un Salón:
```json
POST /espacios/salon
{
  "nombre": "Aula 101",
  "descripcion": "Aula de clases generales",
  "ubicacion": "Primer piso",
  "capacidad": 30,
  "edificioId": 1,
  "numeroSalon": 101,
  "tipoSalon": "Teoría",
  "activo": true
}
```

### Crear un Laboratorio:
```json
POST /espacios/laboratorio
{
  "nombre": "Lab de Computación",
  "descripcion": "Laboratorio de informática",
  "ubicacion": "Segundo piso",
  "capacidad": 25,
  "edificioId": 1,
  "numeroLaboratorio": 201,
  "tipoLaboratorio": "Computación",
  "equipamientoEspecial": true,
  "activo": true
}
```

## 🎯 Ventajas del Factory Pattern

1. **Centralización**: Toda la lógica de creación en un solo lugar
2. **Validaciones**: Reglas específicas por tipo de espacio
3. **Extensibilidad**: Fácil agregar nuevos tipos de espacios
4. **Mantenibilidad**: Código organizado y fácil de mantener
5. **Testabilidad**: Fácil crear mocks para testing
6. **Consistencia**: Misma interfaz para todos los tipos

## 🔄 Flujo de Creación

```
Request → Endpoint → Factory → Validaciones → Entidad → Repository → Database
```

1. **Request**: DTO específico para el tipo de espacio
2. **Endpoint**: Recibe y valida el request
3. **Factory**: Aplica validaciones específicas y crea la entidad
4. **Repository**: Persiste la entidad en la base de datos
5. **Response**: Retorna la entidad creada

## 📚 Notas
- Maneja herencia de espacios (Edificio, Salon, Laboratorio)
- Contiene lógica específica por tipo de espacio
- Se integra con el slice de Acceso
- Es independiente de otros slices
- Implementa el Factory Pattern de manera limpia y extensible