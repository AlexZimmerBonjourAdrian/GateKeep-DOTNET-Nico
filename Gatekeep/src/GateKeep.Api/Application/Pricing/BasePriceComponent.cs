namespace GateKeep.Api.Application.Pricing;

using GateKeep.Api.Domain.Taxes;

/// <summary>
/// ** ADAPTER/WRAPPER PATTERN **
/// Envuelve una ITaxStrategy para que implemente IPriceComponent.
/// Permite usar estrategias de impuestos dentro del sistema de decorators.
/// Base sólida para aplicar el patrón Decorator.
/// </summary>
public sealed class BasePriceComponent : IPriceComponent
{
    private readonly ITaxStrategy _strategy;

    /// <summary>
    /// ** COMPOSITION OVER INHERITANCE **
    /// Recibe la estrategia por constructor (Dependency Injection)
    /// </summary>
    public BasePriceComponent(ITaxStrategy strategy) => _strategy = strategy;

    /// <summary>
    /// Delega el cálculo a la estrategia de impuestos encapsulada
    /// </summary>
    public decimal Compute(decimal baseAmount) => _strategy.Apply(baseAmount);

    /// <summary>
    /// Extrae el CountryId de ConfigurableTaxStrategy si está disponible
    /// Para overrides hardcodeados, usa un valor por defecto
    /// </summary>
    public string CountryId => 
        _strategy is ConfigurableTaxStrategy configurable 
            ? configurable.CountryId 
            : "OVERRIDE"; // Para estrategias hardcodeadas
}
