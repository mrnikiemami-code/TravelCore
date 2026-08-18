namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Resolves a trusted configured ProviderKey to a gateway adapter. No ranking or failover (P20-R3).
/// </summary>
public interface IPaymentProviderResolver
{
    IPaymentProviderGateway? Resolve(ProviderKey providerKey);
}
