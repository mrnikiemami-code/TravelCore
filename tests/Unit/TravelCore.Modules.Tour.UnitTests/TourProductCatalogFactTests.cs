using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourProductCatalogFactTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 5, 0);

    [Fact]
    public void ReplaceServices_DedupesAndOrdersByCode()
    {
        var product = TourProduct.CreateExperience("EXP-SVC-001", "Name", Now);

        product.ReplaceServices(
            [
                new TourCatalogFactInput(" Meals ", "Breakfast"),
                new TourCatalogFactInput("transfer", null),
                new TourCatalogFactInput("meals", "Full board")
            ],
            Now);

        Assert.Equal(2, product.Services.Count);
        Assert.Equal(["meals", "transfer"], product.Services.Select(x => x.Code).ToArray());
        Assert.Equal("Full board", product.Services.Single(x => x.Code == "meals").Detail);
    }

    [Fact]
    public void ReplacePolicies_RejectsInvalidCode()
    {
        var product = TourProduct.CreatePackage("PKG-POL-001", "Name", Now);

        Assert.ThrowsAny<ArgumentException>(() =>
            product.ReplacePolicies([new TourCatalogFactInput("bad code!")], Now));
    }

    [Fact]
    public void ReplaceRequirements_CapsAtMax()
    {
        var product = TourProduct.CreateExperience("EXP-REQ-001", "Name", Now);
        var tooMany = Enumerable.Range(0, TourCatalogFactCode.MaxEntriesPerKind + 1)
            .Select(i => new TourCatalogFactInput($"req-{i:D2}"))
            .ToArray();

        Assert.ThrowsAny<ArgumentException>(() =>
            product.ReplaceRequirements(tooMany, Now));
    }

    [Fact]
    public void ReplacePolicies_EmptyClears()
    {
        var product = TourProduct.CreatePackage("PKG-POL-002", "Name", Now);
        product.ReplacePolicies([new TourCatalogFactInput("cancellation", "24h")], Now);
        Assert.Single(product.Policies);

        product.ReplacePolicies([], Now.Plus(Duration.FromMinutes(1)));
        Assert.Empty(product.Policies);
    }
}
