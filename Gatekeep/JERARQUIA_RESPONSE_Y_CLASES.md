# Jerarquía de Response y Arquitectura de Clases

## Resumen Ejecutivo

Este documento describe la arquitectura completa del sistema Demo1.Api, enfocándose en la jerarquía de response y el uso de patrones de diseño para crear un sistema modular, extensible y mantenible.

## 1. Jerarquía de Response

### 1.1 Flujo Principal de Response

```
HTTP Request → Endpoint → Factory → Component Chain → Response
```

### 1.2 Estructura de Response

#### PricingResponse (DTO Principal)
```csharp
public sealed record PricingResponse(string Country, decimal BaseAmount, decimal FinalAmount, string Receipt);
```

**Propósito**: Contrato de respuesta para el endpoint de pricing
**Uso**: Serialización JSON automática en Minimal APIs
**Ventajas**: 
- Inmutabilidad (record)
- Estructura clara y predecible
- Compatible con OpenAPI/Swagger

## 2. Arquitectura de Clases por Capas

### 2.1 Capa de Contratos (DTOs)
```
Contracts/
├── PricingResponse.cs          # DTO de respuesta principal
└── Country.cs                  # Entidad de dominio
```

### 2.2 Capa de Aplicación
```
Application/
├── Countries/
│   └── ICountryStore.cs        # Repository Pattern
├── Pricing/
│   ├── IPriceComponent.cs      # Interfaz base para decorators
│   ├── PriceDecorator.cs       # Clase base abstracta
│   ├── BasePriceComponent.cs   # Adapter/Wrapper
│   ├── DiscountDecorator.cs    # Concrete Decorator
│   ├── PriceCalculatorFactory.cs # Factory Method
│   └── ConfigurableTaxStrategy.cs # Strategy Pattern
└── Receipts/
    ├── ReceiptGenerator.cs     # Template Method (abstract)
    └── RetailReceiptGenerator.cs # Concrete Template
```

### 2.3 Capa de Dominio
```
Domain/
├── Country.cs                  # Entidad de dominio
└── Taxes/
    └── ITaxStrategy.cs         # Strategy Pattern interface
```

### 2.4 Capa de Infraestructura
```
Infrastructure/
└── CountryMemoryStore.cs       # Repository implementation
```

### 2.5 Capa de Endpoints
```
Endpoints/
├── PricingEndpoints.cs         # Minimal API endpoints
└── CountryEndpoints.cs         # CRUD endpoints
```

## 3. Patrones de Diseño Implementados

### 3.1 Strategy Pattern
**Ubicación**: `ITaxStrategy` → `ConfigurableTaxStrategy`

**Propósito**: Permite cambiar algoritmos de cálculo de impuestos dinámicamente

**Implementación**:
```csharp
public interface ITaxStrategy
{
    decimal Apply(decimal amount);
}

public sealed class ConfigurableTaxStrategy(string countryId, decimal taxRate) : ITaxStrategy
{
    public decimal Apply(decimal baseAmount) => baseAmount + (baseAmount * TaxRate);
}
```

**Ventajas**:
- Flexibilidad para diferentes países
- Fácil testing con mocks
- Extensible para nuevos tipos de impuestos

### 3.2 Decorator Pattern
**Ubicación**: `IPriceComponent` → `PriceDecorator` → `DiscountDecorator`

**Propósito**: Agregar funcionalidades (descuentos) sin modificar código existente

**Jerarquía**:
```
IPriceComponent (interface)
    ↑
PriceDecorator (abstract base)
    ↑
DiscountDecorator (concrete)
```

**Implementación**:
```csharp
public abstract class PriceDecorator(IPriceComponent inner) : IPriceComponent
{
    protected readonly IPriceComponent Inner = inner;
    public abstract decimal Compute(decimal baseAmount);
    public abstract string CountryId { get; }
}

public sealed class DiscountDecorator(IPriceComponent inner, decimal discountRate) : PriceDecorator(inner)
{
    public override decimal Compute(decimal baseAmount)
    {
        var taxed = Inner.Compute(baseAmount);
        var discount = decimal.Round(taxed * discountRate, 2);
        return taxed - discount;
    }
}
```

**Ventajas**:
- Composición flexible de funcionalidades
- Fácil agregar nuevos decorators (comisiones, redondeos, etc.)
- Mantiene la misma interfaz

### 3.3 Factory Method Pattern
**Ubicación**: `PriceCalculatorFactory`

**Propósito**: Crear componentes de precio complejos combinando Strategy + Decorator

**Implementación**:
```csharp
public sealed class PriceCalculatorFactory
{
    public bool TryCreate(string countryId, out IPriceComponent? component)
    {
        // 1. Validar país existe
        // 2. Crear/obtener Strategy desde cache
        // 3. Crear BasePriceComponent
        // 4. Aplicar Decorator si hay descuento
        // 5. Retornar componente final
    }
}
```

**Ventajas**:
- Encapsula lógica compleja de creación
- Centraliza configuración
- Integra cache automáticamente

### 3.4 Template Method Pattern
**Ubicación**: `ReceiptGenerator` → `RetailReceiptGenerator`

**Propósito**: Define estructura fija de algoritmo con pasos personalizables

**Implementación**:
```csharp
public abstract class ReceiptGenerator
{
    // Template Method - NO se overridea
    public string Generate(string countryId, decimal baseAmount, decimal finalAmount)
    {
        var header = BuildHeader();
        var body = BuildBody(countryId, baseAmount, finalAmount);
        var footer = BuildFooter();
        return $"{header}\n{body}\n{footer}";
    }

    // Abstract Hooks - DEBEN implementarse
    protected abstract string BuildHeader();
    protected abstract string BuildBody(string countryId, decimal baseAmount, decimal finalAmount);
    
    // Virtual Hook - PUEDE personalizarse
    protected virtual string BuildFooter() => $"Generated at {DateTimeOffset.UtcNow:u}";
}
```

**Ventajas**:
- Estructura consistente de recibos
- Fácil crear nuevos tipos (WholesaleReceiptGenerator, etc.)
- Reutilización de lógica común

### 3.5 Repository Pattern
**Ubicación**: `ICountryStore` → `CountryMemoryStore`

**Propósito**: Abstraer acceso a datos de países

**Implementación**:
```csharp
public interface ICountryStore
{
    bool Upsert(Country c);
    bool TryGet(string id, out Country? c);
    IReadOnlyCollection<Country> All();
    void Clear();
}
```

**Ventajas**:
- Intercambiable entre implementaciones (memoria, DB, cache)
- Fácil testing con mocks
- Separación de responsabilidades

### 3.6 Adapter Pattern
**Ubicación**: `BasePriceComponent`

**Propósito**: Adaptar `ITaxStrategy` para que implemente `IPriceComponent`

**Implementación**:
```csharp
public sealed class BasePriceComponent : IPriceComponent
{
    private readonly ITaxStrategy _strategy;
    
    public BasePriceComponent(ITaxStrategy strategy) => _strategy = strategy;
    public decimal Compute(decimal baseAmount) => _strategy.Apply(baseAmount);
}
```

**Ventajas**:
- Permite usar Strategy dentro del sistema de Decorators
- Mantiene interfaces separadas
- Facilita composición

## 4. Flujo de Ejecución Completo

### 4.1 Request de Pricing
```
GET /pricing/UY/100
```

### 4.2 Procesamiento
1. **Endpoint** (`PricingEndpoints`): Recibe request
2. **Validación**: Verifica país existe en `ICountryStore`
3. **Factory**: `PriceCalculatorFactory.TryCreate()` crea componente
4. **Strategy**: `ConfigurableTaxStrategy` calcula impuestos
5. **Adapter**: `BasePriceComponent` adapta Strategy a IPriceComponent
6. **Decorator**: `DiscountDecorator` aplica descuento si está configurado
7. **Template**: `RetailReceiptGenerator` genera recibo
8. **Response**: `PricingResponse` serializa resultado

### 4.3 Cadena de Responsabilidad
```
BaseAmount (100) 
    → ConfigurableTaxStrategy.Apply() (100 + 22% = 122)
    → DiscountDecorator.Compute() (122 - 5% = 115.9)
    → RetailReceiptGenerator.Generate()
    → PricingResponse
```

## 5. Ventajas de la Arquitectura

### 5.1 Extensibilidad
- **Nuevos países**: Solo agregar a `ICountryStore`
- **Nuevos impuestos**: Implementar `ITaxStrategy`
- **Nuevos descuentos**: Crear `Decorator` adicional
- **Nuevos recibos**: Extender `ReceiptGenerator`

### 5.2 Testabilidad
- Cada componente es testeable independientemente
- Interfaces permiten mocking fácil
- Factory centraliza configuración de tests

### 5.3 Mantenibilidad
- Separación clara de responsabilidades
- Patrones conocidos y documentados
- Código autodocumentado con comentarios

### 5.4 Performance
- Cache integrado en Factory
- Operaciones Try* evitan excepciones
- Composición eficiente de componentes

## 6. Configuración y Dependencias

### 6.1 Dependency Injection
```csharp
// Program.cs
builder.Services.AddSingleton<ICountryStore, CountryMemoryStore>();
builder.Services.AddSingleton<PriceCalculatorFactory>();
builder.Services.AddSingleton<ReceiptGenerator, RetailReceiptGenerator>();
builder.Services.AddMemoryCache();
```

### 6.2 Configuración
```json
{
  "Pricing": {
    "DiscountRate": 0.05  // 5% descuento global
  }
}
```

## 7. Casos de Uso y Extensiones

### 7.1 Agregar Nuevo Tipo de Recibo
```csharp
public sealed class WholesaleReceiptGenerator : ReceiptGenerator
{
    protected override string BuildHeader() => "== Wholesale Receipt ==";
    protected override string BuildBody(string countryId, decimal baseAmount, decimal finalAmount)
        => $"Bulk pricing for {countryId}: {baseAmount} → {finalAmount}";
}
```

### 7.2 Agregar Nuevo Decorator
```csharp
public sealed class CommissionDecorator : PriceDecorator
{
    public override decimal Compute(decimal baseAmount)
    {
        var result = Inner.Compute(baseAmount);
        return result + (result * 0.02m); // 2% comisión
    }
}
```

### 7.3 Agregar Nueva Strategy
```csharp
public sealed class ProgressiveTaxStrategy : ITaxStrategy
{
    public decimal Apply(decimal amount)
    {
        // Lógica de impuestos progresivos
        return amount * 1.15m; // 15% base
    }
}
```

## 8. Conclusiones

Esta arquitectura demuestra cómo los patrones de diseño trabajan juntos para crear un sistema:

- **Modular**: Cada patrón tiene responsabilidad específica
- **Extensible**: Fácil agregar nuevas funcionalidades
- **Mantenible**: Código claro y bien estructurado
- **Testeable**: Interfaces y composición facilitan testing
- **Performante**: Cache y operaciones eficientes

La jerarquía de response es simple pero poderosa, permitiendo que la complejidad del negocio se maneje en las capas inferiores mientras mantiene una API limpia y predecible.
