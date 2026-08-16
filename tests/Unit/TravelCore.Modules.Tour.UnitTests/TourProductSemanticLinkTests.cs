using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourProductSemanticLinkTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 4, 0);

    [Fact]
    public void SetClassificationCode_NormalizesAndClears()
    {
        var product = TourProduct.CreateExperience("EXP-CLS-001", "Name", Now);

        product.SetClassificationCode(" Cultural-Walk ", Now);
        Assert.Equal("cultural-walk", product.ClassificationCode);

        product.SetClassificationCode("  ", Now.Plus(Duration.FromMinutes(1)));
        Assert.Null(product.ClassificationCode);
    }

    [Fact]
    public void SetClassificationCode_RejectsInvalidChars()
    {
        var product = TourProduct.CreatePackage("PKG-CLS-001", "Name", Now);

        Assert.ThrowsAny<ArgumentException>(() =>
            product.SetClassificationCode("bad code!", Now));
    }

    [Fact]
    public void SetOriginLink_AcceptsNullAndRejectsEmpty()
    {
        var product = TourProduct.CreateExperience("EXP-ORG-001", "Name", Now);
        var origin = Guid.Parse("01900000-0000-7000-8000-000000000001");

        product.SetOriginLink(origin, Now);
        Assert.Equal(origin, product.OriginDestinationId);

        product.SetOriginLink(null, Now.Plus(Duration.FromMinutes(1)));
        Assert.Null(product.OriginDestinationId);

        Assert.ThrowsAny<ArgumentException>(() =>
            product.SetOriginLink(Guid.Empty, Now));
    }

    [Fact]
    public void AssignDestination_IsIdempotentAndCapsAtMax()
    {
        var product = TourProduct.CreatePackage("PKG-DST-001", "Name", Now);
        var dest = Guid.Parse("01900000-0000-7000-8000-0000000000aa");

        var first = product.AssignDestination(dest, Now);
        var second = product.AssignDestination(dest, Now.Plus(Duration.FromSeconds(1)));
        Assert.Same(first, second);
        Assert.Single(product.Destinations);

        for (var i = 0; i < TourProductDestination.MaxLinksPerTourProduct - 1; i++)
        {
            product.AssignDestination(Guid.Parse($"01900000-0000-7000-8000-{i:D12}"), Now);
        }

        Assert.Equal(TourProductDestination.MaxLinksPerTourProduct, product.Destinations.Count);
        Assert.Throws<InvalidOperationException>(() =>
            product.AssignDestination(Guid.Parse("01900000-0000-7000-8000-00000000ffff"), Now));
    }

    [Fact]
    public void AssignDestination_RejectsEmptyGuid()
    {
        var product = TourProduct.CreateExperience("EXP-DST-001", "Name", Now);

        Assert.ThrowsAny<ArgumentException>(() =>
            product.AssignDestination(Guid.Empty, Now));
    }

    [Fact]
    public void RemoveDestination_RemovesExistingLink()
    {
        var product = TourProduct.CreateExperience("EXP-DST-002", "Name", Now);
        var dest = Guid.Parse("01900000-0000-7000-8000-0000000000bb");
        product.AssignDestination(dest, Now);

        Assert.True(product.RemoveDestination(dest, Now.Plus(Duration.FromMinutes(1))));
        Assert.Empty(product.Destinations);
        Assert.False(product.RemoveDestination(dest, Now.Plus(Duration.FromMinutes(2))));
    }

    [Fact]
    public void SetAgencyLink_AcceptsNullAndRejectsEmpty()
    {
        var product = TourProduct.CreatePackage("PKG-AGY-001", "Name", Now);
        var agency = Guid.Parse("01900000-0000-7000-8000-000000000301");

        product.SetAgencyLink(agency, Now);
        Assert.Equal(agency, product.AgencyId);

        product.SetAgencyLink(null, Now.Plus(Duration.FromMinutes(1)));
        Assert.Null(product.AgencyId);

        Assert.ThrowsAny<ArgumentException>(() =>
            product.SetAgencyLink(Guid.Empty, Now));
    }
}
