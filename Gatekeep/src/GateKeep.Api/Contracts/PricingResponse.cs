namespace GateKeep.Api.Contracts;

public sealed record PricingResponse(string Country, decimal BaseAmount, decimal FinalAmount, string Receipt);
