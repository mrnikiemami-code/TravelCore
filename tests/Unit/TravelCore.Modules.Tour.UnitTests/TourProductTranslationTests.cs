using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class TourProductTranslationTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 3, 0);

    [Fact]
    public void UpsertTranslation_CreatesLocaleRow()
    {
        var product = TourProduct.CreateExperience("EXP-TR-001", "Caspian Walk", Now);

        var row = product.UpsertTranslation("fa", " پیاده‌روی خزر ", "توضیح کوتاه", Now);

        Assert.Equal("fa", row.LocaleCode);
        Assert.Equal("پیاده‌روی خزر", row.Title);
        Assert.Equal("توضیح کوتاه", row.Description);
        Assert.Same(row, product.FindTranslation("FA"));
        Assert.Single(product.Translations);
    }

    [Fact]
    public void UpsertTranslation_UpdatesExistingLocale()
    {
        var product = TourProduct.CreatePackage("PKG-TR-001", "Istanbul Package", Now);
        product.UpsertTranslation("en", "Old", null, Now);
        var later = Now.Plus(Duration.FromMinutes(2));

        var row = product.UpsertTranslation("EN", "New Title", "Desc", later);

        Assert.Equal("en", row.LocaleCode);
        Assert.Equal("New Title", row.Title);
        Assert.Equal("Desc", row.Description);
        Assert.Equal(later, row.UpdatedAt);
        Assert.Single(product.Translations);
    }

    [Fact]
    public void UpsertTranslation_RejectsBlankTitle()
    {
        var product = TourProduct.CreateExperience("EXP-TR-002", "Name", Now);

        Assert.ThrowsAny<ArgumentException>(() =>
            product.UpsertTranslation("fa", "  ", null, Now));
    }

    [Fact]
    public void NormalizeLocaleCode_RejectsBlank()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            TourProductTranslation.NormalizeLocaleCode(" "));
    }
}
