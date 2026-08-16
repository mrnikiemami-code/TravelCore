using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourProductTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 2, 0);

    [Fact]
    public void TourProductId_RejectsEmpty()
    {
        Assert.Throws<ArgumentException>(() => TourProductId.From(Guid.Empty));
    }

    [Fact]
    public void TourProductId_New_IsNonEmpty()
    {
        var id = TourProductId.New();
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void CreateExperience_SetsKindAndCoreFields()
    {
        var product = TourProduct.CreateExperience("EXP-001", " Caspian Walk ", Now);

        Assert.Equal(TourKind.Experience, product.Kind);
        Assert.Equal("EXP-001", product.Code);
        Assert.Equal("Caspian Walk", product.EnglishName);
        Assert.Equal(Now, product.CreatedAt);
        Assert.Equal(Now, product.UpdatedAt);
        Assert.NotEqual(Guid.Empty, product.Id.Value);
    }

    [Fact]
    public void CreatePackage_SetsKindAndCoreFields()
    {
        var product = TourProduct.CreatePackage("PKG-001", "Istanbul Package", Now);

        Assert.Equal(TourKind.Package, product.Kind);
        Assert.Equal("PKG-001", product.Code);
        Assert.Equal("Istanbul Package", product.EnglishName);
    }

    [Fact]
    public void CreateExperience_RejectsBlankCode()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            TourProduct.CreateExperience("  ", "Name", Now));
    }

    [Fact]
    public void CreatePackage_RejectsBlankName()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            TourProduct.CreatePackage("PKG-002", " ", Now));
    }

    [Fact]
    public void RenameEnglishName_UpdatesTimestamp()
    {
        var product = TourProduct.CreateExperience("EXP-002", "Old", Now);
        var later = Now.Plus(Duration.FromMinutes(5));

        product.RenameEnglishName(" New Name ", later);

        Assert.Equal("New Name", product.EnglishName);
        Assert.Equal(later, product.UpdatedAt);
        Assert.Equal(Now, product.CreatedAt);
    }

    [Fact]
    public void Reconstitute_PreservesIdentityAndKind()
    {
        var id = TourProductId.New();
        var product = TourProduct.Reconstitute(
            id,
            TourKind.Package,
            "PKG-003",
            "Reconstituted",
            Now,
            Now.Plus(Duration.FromHours(1)));

        Assert.Equal(id, product.Id);
        Assert.Equal(TourKind.Package, product.Kind);
        Assert.Equal("PKG-003", product.Code);
    }
}
