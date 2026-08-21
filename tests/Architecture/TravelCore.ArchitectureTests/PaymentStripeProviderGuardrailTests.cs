using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Infrastructure.Providers;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P35-T008: Stripe TEST-MODE adapter is non-production, fail-closed, and never flips production flag.
/// </summary>
public sealed class PaymentStripeProviderGuardrailTests
{
    [Fact]
    public void NamedProductionAdapterImplemented_Remains_False_With_Stripe_Code()
    {
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.Equal(
            "stripe test mode != production activation",
            PaymentProviderTrustBoundary.StripeTestModeIsNotProductionActivation);
    }

    [Fact]
    public void Production_Cannot_Register_Stripe_Test_Mode()
    {
        Assert.False(PaymentStripeGate.IsAllowed("Production", enabled: true, secretKey: "sk_test_x"));
        Assert.True(PaymentStripeGate.IsAllowed("Development", enabled: true, secretKey: "sk_test_x"));
        Assert.False(PaymentStripeGate.IsAllowed("Development", enabled: true, secretKey: "sk_live_x"));
    }

    [Fact]
    public void PaymentModule_Registers_Stripe_Only_Behind_Gate()
    {
        var module = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Infrastructure",
            "PaymentModule.cs"));

        Assert.Contains("TryRegisterStripe", module, StringComparison.Ordinal);
        Assert.Contains("PaymentStripeGate.IsAllowed", module, StringComparison.Ordinal);
        Assert.Contains("StripePaymentProviderGateway", module, StringComparison.Ordinal);
        Assert.DoesNotContain("NamedProductionAdapterImplemented = true", module, StringComparison.Ordinal);
    }

    [Fact]
    public void Stripe_Net_Dependency_Stays_In_Payment_Infrastructure()
    {
        var infraCsproj = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Infrastructure",
            "TravelCore.Modules.Payment.Infrastructure.csproj"));
        Assert.Contains("Stripe.net", infraCsproj, StringComparison.Ordinal);

        var contractsCsproj = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Contracts",
            "TravelCore.Modules.Payment.Contracts.csproj"));
        Assert.DoesNotContain("Stripe.net", contractsCsproj, StringComparison.Ordinal);

        var bookingContracts = Directory.GetFiles(
                Path.Combine(FindRepoRoot(), "src", "backend", "Modules", "Booking"),
                "*.csproj",
                SearchOption.AllDirectories)
            .Select(File.ReadAllText);
        Assert.All(bookingContracts, text => Assert.DoesNotContain("Stripe.net", text, StringComparison.Ordinal));
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "TravelCore.sln"))
                || File.Exists(Path.Combine(dir.FullName, "TravelCore.slnx")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Repository root not found.");
    }
}
