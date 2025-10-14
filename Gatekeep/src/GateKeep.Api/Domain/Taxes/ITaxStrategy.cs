namespace GateKeep.Api.Domain.Taxes;

public interface ITaxStrategy
{
    decimal Apply(decimal amount);
}
