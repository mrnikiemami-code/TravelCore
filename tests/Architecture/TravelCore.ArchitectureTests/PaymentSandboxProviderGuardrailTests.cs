using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Infrastructure.Providers;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P34-T003: Sandbox Payment provider is non-production, fail-closed, and never flips production adapter flag.
/// </summary>
public sealed class PaymentSandboxProviderGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void NamedProductionAdapterImplemented_Remains_False_While_Only_Sandbox_Exists()
    {
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.False(PaymentProviderTrustBoundary.ProductionFakeProviderRegistered);
        Assert.Equal("NONE", PaymentProviderTrustBoundary.NamedProviderSelected);
        Assert.Equal("NOT CONFIGURED / NONE", PaymentProviderTrustBoundary.ProductionProviderPosture);
        Assert.Equal(
            "sandbox != production provider",
            PaymentProviderTrustBoundary.SandboxIsNotProductionProvider);
    }

    [Fact]
    public void Production_Cannot_Register_Sandbox()
    {
        Assert.False(PaymentSandboxGate.IsAllowed("Production", enabled: true));
        Assert.False(PaymentSandboxGate.IsAllowed("production", enabled: true));
        Assert.True(PaymentSandboxGate.IsAllowed("Development", enabled: true));
        Assert.False(PaymentSandboxGate.IsAllowed("Development", enabled: false));
    }

    [Fact]
    public void Sandbox_Provider_Is_Not_Named_Production_Selection()
    {
        Assert.Equal("sandbox", PaymentSandboxGate.ProviderKeyValue);
        Assert.NotEqual(PaymentSandboxGate.ProviderKeyValue, PaymentProviderTrustBoundary.NamedProviderSelected);
        Assert.Contains("non-production", PaymentSandboxGate.DisplayName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PaymentModule_Registers_Sandbox_Only_Behind_Gate()
    {
        var module = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Infrastructure",
            "PaymentModule.cs"));

        Assert.Contains("PaymentSandboxGate.IsAllowed", module, StringComparison.Ordinal);
        Assert.Contains("SandboxPaymentProviderGateway", module, StringComparison.Ordinal);
        Assert.Contains("TryAddEnumerable", module, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "services.AddSingleton<IPaymentProviderGateway",
            module,
            StringComparison.Ordinal);
        Assert.DoesNotContain("NamedProductionAdapterImplemented = true", module, StringComparison.Ordinal);
    }

    [Fact]
    public void TrustBoundary_Source_Keeps_NamedProductionAdapterImplemented_False()
    {
        var source = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Contracts",
            "PaymentProviderTrustBoundary.cs"));

        Assert.Contains(
            "NamedProductionAdapterImplemented = false",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "NamedProductionAdapterImplemented = true",
            source,
            StringComparison.Ordinal);
    }
}
