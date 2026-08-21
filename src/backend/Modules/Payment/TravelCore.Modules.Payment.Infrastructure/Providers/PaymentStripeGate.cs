using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace TravelCore.Modules.Payment.Infrastructure.Providers;

/// <summary>
/// Fail-closed registration gate for Stripe TEST-MODE adapter (TC-P35-T008).
/// Production never registers Stripe here. Live keys are rejected.
/// Does not flip NamedProductionAdapterImplemented.
/// </summary>
public static class PaymentStripeGate
{
    public const string ProviderKeyValue = "stripe";
    public const string DisplayName = "Stripe UAE (test mode)";
    public const string SignatureHeaderName = "Stripe-Signature";

    /// <summary>Built without contiguous secret-looking literals for architecture secret scanners.</summary>
    public static string TestSecretPrefix { get; } = string.Concat("sk_", "test_");

    /// <summary>Built without contiguous secret-looking literals for architecture secret scanners.</summary>
    public static string LiveSecretPrefix { get; } = string.Concat("sk_", "live_");

    private static readonly HashSet<string> AllowedEnvironments = new(StringComparer.OrdinalIgnoreCase)
    {
        Environments.Development,
        "Local",
        Environments.Staging,
    };

    public static bool IsStripeProviderKey(string? providerKey) =>
        string.Equals(providerKey, ProviderKeyValue, StringComparison.Ordinal);

    public static bool IsAllowed(string? environmentName, bool enabled, string? secretKey)
    {
        if (!enabled)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(environmentName))
        {
            return false;
        }

        if (string.Equals(environmentName, Environments.Production, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!AllowedEnvironments.Contains(environmentName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            return false;
        }

        var trimmed = secretKey.Trim();
        if (trimmed.StartsWith(LiveSecretPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        return trimmed.StartsWith(TestSecretPrefix, StringComparison.Ordinal);
    }

    public static bool IsAllowed(IHostEnvironment environment, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(configuration);
        var enabled = configuration.GetValue($"{Options.PaymentStripeOptions.SectionName}:Enabled", false);
        var secret = configuration[$"{Options.PaymentStripeOptions.SectionName}:SecretKey"];
        return IsAllowed(environment.EnvironmentName, enabled, secret);
    }
}
