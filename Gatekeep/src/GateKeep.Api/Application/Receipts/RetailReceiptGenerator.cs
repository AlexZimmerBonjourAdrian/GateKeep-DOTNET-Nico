using System.Globalization;

namespace GateKeep.Api.Application.Receipts;

/// <summary>
/// ** CONCRETE IMPLEMENTATION ** de Template Method
/// Personaliza los pasos específicos del algoritmo de generación de recibos
/// para el contexto retail/minorista.
/// Demuestra cómo diferentes tipos de recibo pueden coexistir.
/// </summary>
public sealed class RetailReceiptGenerator : ReceiptGenerator
{
    /// <summary>
    /// ** HOOK IMPLEMENTATION ** - Header específico para retail
    /// </summary>
    protected override string BuildHeader()
        => "== Retail Receipt ==";

    /// <summary>
    /// ** HOOK IMPLEMENTATION ** - Formato de body para retail
    /// Incluye el countryId para trazabilidad y usa formato invariant
    /// para consistencia en diferentes culturas.
    /// </summary>
    protected override string BuildBody(string countryId, decimal baseAmount, decimal finalAmount)
        => $"Country: {countryId}, " +
           $"Base: {baseAmount.ToString("0.##", CultureInfo.InvariantCulture)}, " +
           $"Final: {finalAmount.ToString("0.##", CultureInfo.InvariantCulture)}";

    // ** NOTA EDUCATIVA **
    // Footer usa implementación base (virtual hook)
    // Si quisieras personalizar: protected override string BuildFooter() => base.BuildFooter();
}
