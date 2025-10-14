namespace GateKeep.Api.Application.Pricing;

using GateKeep.Api.Domain.Taxes;

public sealed class ConfigurableTaxStrategy(string countryId, decimal taxRate) : ITaxStrategy
{
  public string CountryId { get; } = countryId;
  private decimal TaxRate { get; } = taxRate < 0 ? 0 : taxRate; // saneo básico

  public decimal Apply(decimal baseAmount)
    => baseAmount + (baseAmount * TaxRate);
}
