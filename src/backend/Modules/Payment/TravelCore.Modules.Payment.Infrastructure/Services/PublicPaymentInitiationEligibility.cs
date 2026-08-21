using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Shared public initiation eligibility for Tour / Hotel / Flight (TC-P34-T003).
/// Allows labeled sandbox without flipping NamedProductionAdapterImplemented.
/// </summary>
internal static class PublicPaymentInitiationEligibility
{
    public static bool IsAvailable(
        IOptions<PaymentProviderOptions> options,
        IPaymentProviderResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resolver);

        if (!ProviderKey.TryParse(options.Value.DefaultProviderKey, out var key))
        {
            return false;
        }

        if (string.Equals(key.Value, "test", StringComparison.Ordinal))
        {
            return false;
        }

        if (Providers.PaymentSandboxGate.IsSandboxProviderKey(key.Value)
            || Providers.PaymentStripeGate.IsStripeProviderKey(key.Value))
        {
            // Non-production labeled adapters: availability = DI registration (fail-closed in Production).
            return resolver.Check(key, PaymentProviderCapability.RedirectInitiation)
                == ProviderCapabilityStatus.Available;
        }

        // Production path — requires NamedProductionAdapterImplemented (remains false until real adapter).
        return PaymentProviderTrustBoundary.NamedProductionAdapterImplemented
            && resolver.Check(key, PaymentProviderCapability.RedirectInitiation)
                == ProviderCapabilityStatus.Available;
    }
}
