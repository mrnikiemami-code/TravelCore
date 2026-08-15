using NodaTime;
using TravelCore.Modules.Destination.Domain;
using Xunit;
using DestinationAggregate = TravelCore.Modules.Destination.Domain.Destination;

namespace TravelCore.Modules.Destination.UnitTests;

public sealed class DestinationSlugTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 16, 0, 10);

    [Fact]
    public void Slug_NormalizesAndRejectsInvalidShapes()
    {
        var country = DestinationAggregate.Create(
            DestinationKind.Country,
            "IR",
            "Iran",
            Now,
            isoCountryCode: "IR");

        country.UpsertTranslation("fa", "ایران", null, Now);
        country.SetTranslationSlug("fa", "Iran-Tehran", Now);
        Assert.Equal("iran-tehran", country.FindTranslation("fa")!.Slug);

        Assert.Throws<ArgumentException>(() => country.SetTranslationSlug("fa", "bad slug", Now));
        Assert.Throws<ArgumentException>(() => country.SetTranslationSlug("fa", "-bad", Now));
        Assert.Throws<ArgumentException>(() =>
            DestinationTranslation.NormalizeSlug("has_underscore"));
    }
}
