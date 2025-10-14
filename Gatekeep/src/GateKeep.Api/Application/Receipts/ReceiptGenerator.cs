namespace GateKeep.Api.Application.Receipts;

/// <summary>
/// ** TEMPLATE METHOD PATTERN **
/// Define el esqueleto de un algoritmo (generar recibo) en la clase base,
/// permitiendo que las subclases redefinan pasos específicos sin cambiar la estructura.
/// Ejemplo clásico: algoritmo de cocina donde cada chef personaliza ingredientes.
/// </summary>
public abstract class ReceiptGenerator
{
    /// <summary>
    /// ** TEMPLATE METHOD ** - Algoritmo completo con pasos definidos
    /// Este método NO debe ser overrideado (estructura fija)
    /// Las subclases personalizan comportamiento via métodos Hook
    /// </summary>
    public string Generate(string countryId, decimal baseAmount, decimal finalAmount)
    {
        var header = BuildHeader();
        var body = BuildBody(countryId, baseAmount, finalAmount);
        var footer = BuildFooter();
        return $"{header}\n{body}\n{footer}";
    }

    /// <summary>
    /// ** ABSTRACT HOOK ** - Las subclases DEBEN implementar
    /// </summary>
    protected abstract string BuildHeader();

    /// <summary>
    /// ** ABSTRACT HOOK ** - Las subclases DEBEN implementar
    /// Incluye el countryId para trazabilidad en el recibo.
    /// </summary>
    protected abstract string BuildBody(string countryId, decimal baseAmount, decimal finalAmount);

    /// <summary>
    /// ** VIRTUAL HOOK ** - Las subclases PUEDEN personalizar (comportamiento default)
    /// </summary>
    protected virtual string BuildFooter() => $"Generated at {DateTimeOffset.UtcNow:u}";
}
