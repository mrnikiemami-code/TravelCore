using Microsoft.Extensions.Options;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Infrastructure.Options;

namespace TravelCore.Modules.Payment.Infrastructure.Providers;

/// <summary>
/// Resolves configured adapters only. Duplicate ProviderKey is rejected. No ranking or failover (P20-R8).
/// </summary>
internal sealed class PaymentProviderResolver : IPaymentProviderResolver
{
    private readonly IReadOnlyDictionary<string, IPaymentProviderGateway> _gateways;
    private readonly HashSet<string> _disabled;

    public PaymentProviderResolver(IEnumerable<IPaymentProviderGateway> gateways)
        : this(gateways, Microsoft.Extensions.Options.Options.Create(new PaymentProviderOptions()))
    {
    }

    public PaymentProviderResolver(
        IEnumerable<IPaymentProviderGateway> gateways,
        IOptions<PaymentProviderOptions> options)
    {
        ArgumentNullException.ThrowIfNull(gateways);
        ArgumentNullException.ThrowIfNull(options);
        var list = gateways.ToList();
        var duplicates = list
            .GroupBy(gateway => gateway.Key.Value, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicates.Length > 0)
        {
            throw new InvalidOperationException(
                "Duplicate ProviderKey registration is forbidden: " + string.Join(", ", duplicates));
        }

        _gateways = list.ToDictionary(gateway => gateway.Key.Value, StringComparer.Ordinal);
        _disabled = new HashSet<string>(
            options.Value.DisabledProviderKeys ?? [],
            StringComparer.Ordinal);
    }

    public IPaymentProviderGateway? Resolve(ProviderKey providerKey)
    {
        if (Check(providerKey, PaymentProviderCapability.None) is
            ProviderCapabilityStatus.UnknownProvider or ProviderCapabilityStatus.DisabledProvider)
        {
            return null;
        }

        return _gateways.TryGetValue(providerKey.Value, out var gateway) ? gateway : null;
    }

    public PaymentProviderDescriptor? Describe(ProviderKey providerKey)
    {
        if (!_gateways.TryGetValue(providerKey.Value, out var gateway))
        {
            return null;
        }

        var enabled = !_disabled.Contains(providerKey.Value);
        var isSandbox = PaymentSandboxGate.IsSandboxProviderKey(providerKey.Value);
        var isStripeTest = PaymentStripeGate.IsStripeProviderKey(providerKey.Value);
        var publicInitiation = enabled
            && gateway.Capabilities.HasFlag(PaymentProviderCapability.RedirectInitiation)
            && (
                isSandbox
                || isStripeTest
                || (PaymentProviderTrustBoundary.NamedProductionAdapterImplemented
                    && !string.Equals(providerKey.Value, "test", StringComparison.Ordinal)));
        return new PaymentProviderDescriptor(
            providerKey.Value,
            isSandbox
                ? PaymentSandboxGate.DisplayName
                : isStripeTest
                    ? PaymentStripeGate.DisplayName
                    : providerKey.Value,
            gateway.Capabilities,
            enabled,
            publicInitiation);
    }

    public IReadOnlyList<PaymentProviderDescriptor> ListDescriptors() =>
        _gateways.Keys
            .Select(key => Describe(new ProviderKey(key))!)
            .ToArray();

    public ProviderCapabilityStatus Check(ProviderKey providerKey, PaymentProviderCapability required)
    {
        if (!_gateways.TryGetValue(providerKey.Value, out var gateway))
        {
            return ProviderCapabilityStatus.UnknownProvider;
        }

        if (_disabled.Contains(providerKey.Value))
        {
            return ProviderCapabilityStatus.DisabledProvider;
        }

        if (required != PaymentProviderCapability.None && !gateway.Capabilities.HasFlag(required))
        {
            return ProviderCapabilityStatus.UnsupportedCapability;
        }

        return ProviderCapabilityStatus.Available;
    }
}
