namespace GateKeep.Api.Application.Pricing;

/// <summary>
/// ** CONCRETE DECORATOR **
/// Implementación específica que añade descuentos al cálculo de precios.
/// Demuestra cómo extender funcionalidad sin modificar el código existente.
/// Fácilmente combinable con otros decorators (comisiones, redondeos, etc.)
/// </summary>
public sealed class DiscountDecorator(IPriceComponent inner, decimal discountRate) : PriceDecorator(inner)
{
    /// <summary>
    /// ** DECORATOR LOGIC **
    /// 1. Delega el cálculo base al componente interno
    /// 2. Aplica la lógica de descuento adicional
    /// 3. Retorna el resultado modificado
    /// </summary>
    public override decimal Compute(decimal baseAmount)
    {
        // Primero calcula con el componente decorado (puede ser base o múltiples decorators)
        var taxed = Inner.Compute(baseAmount);

        // Aplica el descuento específico de este decorator
        var discount = decimal.Round(taxed * discountRate, 2);
        return taxed - discount;
    }

    /// <summary>
    /// ** DECORATOR DELEGATION **
    /// Delega la información del país al componente interno
    /// Mantiene la transparencia en la cadena de decorators
    /// </summary>
    public override string CountryId => Inner.CountryId;
}
