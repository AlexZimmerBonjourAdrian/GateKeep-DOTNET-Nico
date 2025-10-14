namespace GateKeep.Api.Application.Pricing;

/// <summary>
/// Interfaz común para objetos que pueden ser decorados con funcionalidades adicionales.
/// Permite composición flexible de comportamientos de cálculo de precios.
/// </summary>
public interface IPriceComponent
{
    /// <summary>
    /// Calcula el precio final basado en el monto base
    /// Implementaciones pueden agregar impuestos, descuentos, comisiones, etc.
    /// </summary>
    decimal Compute(decimal baseAmount);

    /// <summary>
    /// Expone el país asociado al componente para trazabilidad
    /// Permite que el país fluya hasta el recibo sin perder información
    /// </summary>
    string CountryId { get; }
}
