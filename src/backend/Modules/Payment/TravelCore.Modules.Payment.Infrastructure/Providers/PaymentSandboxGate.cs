using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace TravelCore.Modules.Payment.Infrastructure.Providers;

/// <summary>
/// Fail-closed registration gate for the labeled sandbox adapter (TC-P34-T003).
/// Production can never register sandbox — even if config Enabled=true.
/// </summary>
public static class PaymentSandboxGate
{
    public const string ProviderKeyValue = "sandbox";
    public const string DisplayName = "Sandbox (non-production)";
    public const string SignatureHeaderName = "X-TravelCore-Sandbox-Signature";

    private static readonly HashSet<string> AllowedEnvironments = new(StringComparer.OrdinalIgnoreCase)
    {
        Environments.Development,
        "Local",
        Environments.Staging,
    };

    public static bool IsAllowed(IHostEnvironment environment, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(configuration);
        var enabled = configuration.GetValue($"{Options.PaymentSandboxOptions.SectionName}:Enabled", false);
        return IsAllowed(environment.EnvironmentName, enabled);
    }

    /// <summary>
    /// Pure gate used by architecture/unit tests and host composition.
    /// </summary>
    public static bool IsAllowed(string? environmentName, bool enabled)
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

        return AllowedEnvironments.Contains(environmentName);
    }

    public static bool IsSandboxProviderKey(string? providerKey) =>
        string.Equals(providerKey, ProviderKeyValue, StringComparison.Ordinal);
}
