using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.TripPlanner.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class TripPlannerPublicHostTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public TripPlannerPublicHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Public_TripPlanner_Anonymous_Draft_And_Submit_Are_Honest_And_Not_Booking()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var create = await client.PostAsJsonAsync(
            "/api/trip-planner/public/intents",
            new { localeCode = "en" },
            ct);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var createJson = await create.Content.ReadAsStringAsync(ct);
        Assert.DoesNotContain("Book Now", createJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Checkout", createJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Payment", createJson, StringComparison.OrdinalIgnoreCase);

        using var createDoc = JsonDocument.Parse(createJson);
        var intentId = createDoc.RootElement.GetProperty("intentId").GetGuid();
        var draftToken = createDoc.RootElement.GetProperty("draftAccessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(draftToken));

        var missingToken = await client.GetAsync(
            $"/api/trip-planner/public/intents/{intentId}",
            ct);
        Assert.Equal(HttpStatusCode.BadRequest, missingToken.StatusCode);

        using var readRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/trip-planner/public/intents/{intentId}");
        readRequest.Headers.Add(TripPlannerPublicCompositionBoundary.DraftTokenHeader, draftToken);
        var read = await client.SendAsync(readRequest, ct);
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);

        using var patchRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"/api/trip-planner/public/intents/{intentId}")
        {
            Content = JsonContent.Create(new
            {
                timing = new { kind = "Undecided" },
                travelers = new { adultCount = 2, childCount = 0, infantCount = 0 },
                destination = new { undecided = true, logicalDestinationIds = (Guid[]?)null },
            }),
        };
        patchRequest.Headers.Add(TripPlannerPublicCompositionBoundary.DraftTokenHeader, draftToken);
        var patch = await client.SendAsync(patchRequest, ct);
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);

        using var submitRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/trip-planner/public/intents/{intentId}/submit")
        {
            Content = JsonContent.Create(new
            {
                displayName = "Alex Planner",
                email = "alex@example.com",
                followUpContactAllowed = true,
                marketingAllowed = false,
                privacyNoticeVersion = "P18-PRIVACY-V1",
                preferredContactChannel = "Email",
            }),
        };
        submitRequest.Headers.Add(TripPlannerPublicCompositionBoundary.DraftTokenHeader, draftToken);
        var submit = await client.SendAsync(submitRequest, ct);
        Assert.Equal(HttpStatusCode.OK, submit.StatusCode);

        var submitJson = await submit.Content.ReadAsStringAsync(ct);
        Assert.Contains("Submitted", submitJson, StringComparison.Ordinal);

        using var resubmitRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/trip-planner/public/intents/{intentId}/submit")
        {
            Content = JsonContent.Create(new { email = "alex@example.com" }),
        };
        resubmitRequest.Headers.Add(TripPlannerPublicCompositionBoundary.DraftTokenHeader, draftToken);
        var resubmit = await client.SendAsync(resubmitRequest, ct);
        Assert.Equal(HttpStatusCode.OK, resubmit.StatusCode);

        using var resubmitDoc = JsonDocument.Parse(await resubmit.Content.ReadAsStringAsync(ct));
        Assert.True(resubmitDoc.RootElement.GetProperty("alreadySubmitted").GetBoolean());
    }
}
