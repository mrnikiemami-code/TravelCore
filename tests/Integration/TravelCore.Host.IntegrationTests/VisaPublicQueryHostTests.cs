using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Visa.Contracts;
using TravelCore.Modules.Visa.Domain;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class VisaPublicQueryHostTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public VisaPublicQueryHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Public_Visa_Read_Is_Anonymous_Locale_Explicit_And_Not_Indexed_By_Default()
    {
        var ct = TestContext.Current.CancellationToken;
        var now = Instant.FromUtc(2026, 8, 18, 0, 30);
        var destinationId = Guid.Parse("0198b3e0-0000-7000-8000-000000000081");
        var code = $"PUB{Guid.NewGuid():N}"[..12].ToUpperInvariant();

        await using (var visaDb = _fixture.CreateVisaDb())
        {
            var definition = VisaDefinition.Create(
                code,
                "en",
                "Tourist visa",
                now,
                "Visitor entry");
            var set = definition.AddRequirementSet(destinationId, now, "IR", null, "Adult");
            set.AddRequiredDocument("PASSPORT", "Required", "en", "Passport", now);
            set.AddEligibilityRequirement(
                "MIN_PASSPORT_VALIDITY",
                "Required",
                "en",
                "Passport validity",
                now,
                "MinValidity",
                "6",
                "Months");
            set.SetProcessingTime(5, "BusinessDays", now, 10);
            set.SetValidity(90, "Days", now);
            set.SetAllowedStay(30, "Days", now);
            set.SetEntryPolicy("Single", now);
            set.AddOfficialFee("Embassy", 80m, "EUR", now, "consular schedule");
            visaDb.VisaDefinitions.Add(definition);
            await visaDb.SaveChangesAsync(ct);
        }

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var missingLocale = await client.GetAsync($"/api/visa/public/definitions/{code}", ct);
        Assert.Equal(HttpStatusCode.BadRequest, missingLocale.StatusCode);

        var unknown = await client.GetAsync(
            "/api/visa/public/definitions/UNKNOWN?localeCode=en",
            ct);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);

        var otherLocale = await client.GetAsync(
            $"/api/visa/public/definitions/{code}?localeCode=fa",
            ct);
        Assert.Equal(HttpStatusCode.NotFound, otherLocale.StatusCode);

        var ok = await client.GetAsync(
            $"/api/visa/public/definitions/{code}?localeCode=en",
            ct);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);

        var json = await ok.Content.ReadAsStringAsync(ct);
        Assert.DoesNotContain("Apply Now", json, StringComparison.Ordinal);
        Assert.DoesNotContain("IndexPolicy", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Quote", json, StringComparison.Ordinal);

        var body = JsonSerializer.Deserialize<PublicVisaDefinition>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(body);
        Assert.Equal(code, body.Code);
        Assert.Equal("en", body.LocaleCode);
        Assert.Equal($"visas/{code}", body.PublicPath);
        var setBody = Assert.Single(body.RequirementSets);
        Assert.Equal(destinationId, setBody.Applicability.DestinationGeographicId);
        Assert.Equal("passport", Assert.Single(setBody.RequiredDocuments).Code);
        Assert.Equal("min_passport_validity", Assert.Single(setBody.EligibilityRequirements).Code);
        Assert.Equal("BusinessDays", setBody.ProcessingTime?.Unit);
        Assert.Equal("Days", setBody.Validity?.Unit);
        Assert.Equal("Days", setBody.AllowedStay?.Unit);
        Assert.Equal("Single", setBody.EntryPolicy?.Kind);
        Assert.Equal("Embassy", Assert.Single(setBody.OfficialFees).Kind);
        Assert.Equal("EUR", Assert.Single(setBody.OfficialFees).Money.CurrencyCode);
        Assert.StartsWith("80", Assert.Single(setBody.OfficialFees).Money.Amount, StringComparison.Ordinal);
    }
}
