using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.B2B.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P24-T008 / P24-R8: hardening guardrails for deferred B2B concerns.
/// </summary>
public sealed class B2BHardeningGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void B2B_Hardening_Guardrails_Are_Declared()
    {
        Assert.True(B2BOwnershipBoundary.B2BHardeningGuardrailsImplemented);
        Assert.False(B2BOwnershipBoundary.OwnsProviderExecution);
        Assert.False(B2BOwnershipBoundary.OwnsSettlementExecution);
        Assert.False(B2BOwnershipBoundary.OwnsAdvancedFinanceExecution);
    }

    [Fact]
    public void B2B_T008_Forbids_Provider_And_Advanced_Finance_Product_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "B2B");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(ProviderAdapter|SupplierAdapter|SettlementEngine|Wallet|CreditLimit|Invoice|CommissionPayout)\b",
            RegexOptions.Compiled);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return forbiddenType.IsMatch(x.line);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(hits.Count == 0, "B2B T008 forbids provider/advanced-finance types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P24_Evidence_Records_T008_And_R8()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P24-implementation-plan.md"));
        Assert.Contains("TC-P24-T008", plan, StringComparison.Ordinal);
        Assert.Contains("P24-R8", plan, StringComparison.Ordinal);
        Assert.Contains("hardening and guardrails", plan, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
