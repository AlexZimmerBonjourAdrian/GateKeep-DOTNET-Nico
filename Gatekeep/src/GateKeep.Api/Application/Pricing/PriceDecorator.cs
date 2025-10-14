namespace GateKeep.Api.Application.Pricing;

/// <summary>
/// ** DECORATOR PATTERN ** (Clase base abstracta)
/// Permite agregar responsabilidades a objetos dinámicamente sin alterar su estructura.
/// Mantiene la misma interfaz (IPriceComponent) pero añade funcionalidad.
/// Ejemplo clásico: decorar cafés con leche, azúcar, etc.
/// </summary>
public abstract class PriceDecorator(IPriceComponent inner) : IPriceComponent
{
    /// <summary>
    /// ** COMPOSITION ** - Componente que está siendo decorado
    /// Cada decorator puede decorar tanto componentes base como otros decorators
    /// </summary>
    protected readonly IPriceComponent Inner = inner;

    /// <summary>
    /// Las clases derivadas implementan la lógica específica de decoración
    /// Típicamente llaman a Inner.Compute() y modifican el resultado
    /// </summary>
    public abstract decimal Compute(decimal baseAmount);

    /// <summary>
    /// Las clases derivadas deben decidir cómo exponer el CountryId
    /// Normalmente delegarán al componente interno
    /// </summary>
    public abstract string CountryId { get; }
}
