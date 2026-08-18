using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Infrastructure.Providers;

/// <summary>
/// Resolves configured adapters only. Production registers none in T003 (P20-R3).
/// </summary>
internal sealed class PaymentProviderResolver : IPaymentProviderResolver
{
    private readonly IReadOnlyDictionary<string, IPaymentProviderGateway> _gateways;

    public PaymentProviderResolver(IEnumerable<IPaymentProviderGateway> gateways)
    {
        ArgumentNullException.ThrowIfNull(gateways);
        _gateways = gateways.ToDictionary(gateway => gateway.Key.Value, StringComparer.Ordinal);
    }

    public IPaymentProviderGateway? Resolve(ProviderKey providerKey)
    {
        return _gateways.TryGetValue(providerKey.Value, out var gateway) ? gateway : null;
    }
}
