using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Services;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class PaymentFoundationHostTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public PaymentFoundationHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Host_Registers_PaymentDbContext_Without_Public_Payment_Api()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
            Assert.Equal("payment", PaymentDbContext.SchemaName);
            Assert.Equal("payment", db.Model.GetDefaultSchema());
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<PaymentSuccessOutboxDispatcher>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<RefundSucceededOutboxDispatcher>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IPaymentSucceededIntegrationHandler>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IRefundSucceededIntegrationHandler>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IPublicBookingPaymentService>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<TravelCore.Modules.Booking.Infrastructure.Services.BookingCompensationOutboxDispatcher>());
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var list = await client.GetAsync("/api/payment", ct);
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var post = await client.PostAsync("/api/payment", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
        using var refund = await client.PostAsync("/api/payment/refund", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, refund.StatusCode);
        using var paymentRefund = await client.PostAsync(
            "/api/payment/" + Guid.CreateVersion7().ToString("D") + "/refund",
            content: null,
            ct);
        Assert.Equal(HttpStatusCode.NotFound, paymentRefund.StatusCode);
        using var callback = await client.PostAsync("/api/payment/callback", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, callback.StatusCode);
        using var webhook = await client.PostAsync("/api/payment/webhook", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, webhook.StatusCode);

        using var unknownProvider = await client.PostAsync(
            "/api/payment/providers/unknown/callback?success=true",
            new StringContent("""{"success":true}""", System.Text.Encoding.UTF8, "application/json"),
            ct);
        Assert.Equal(HttpStatusCode.NotFound, unknownProvider.StatusCode);

        using var unverified = await client.PostAsync(
            "/api/payment/providers/test/callback?success=true",
            new StringContent("""{"success":true}""", System.Text.Encoding.UTF8, "application/json"),
            ct);
        Assert.Equal(HttpStatusCode.NotFound, unverified.StatusCode);
        using var operational = await client.GetAsync("/api/payment/operational/" + Guid.CreateVersion7().ToString("D"), ct);
        Assert.Equal(HttpStatusCode.NotFound, operational.StatusCode);
        using var adminPayment = await client.GetAsync("/api/admin/payments/" + Guid.CreateVersion7().ToString("D"), ct);
        Assert.Equal(HttpStatusCode.NotFound, adminPayment.StatusCode);
    }

    [Fact]
    public async Task Host_Starts_With_Zero_Production_Providers_And_Internal_Operational_Query()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);
        using var scope = factory.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IPaymentProviderResolver>();
        var operational = scope.ServiceProvider.GetRequiredService<IPaymentOperationalQuery>();
        Assert.Empty(resolver.ListDescriptors());
        Assert.Null(await operational.GetByPaymentIdAsync(Guid.CreateVersion7(), ct));
        Assert.Equal("NONE", PaymentProviderTrustBoundary.NamedProviderSelected);
        Assert.False(PaymentProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.False(PaymentOperationalBoundary.PublicOperationalEndpointImplemented);
        Assert.False(PaymentOperationalBoundary.ManualPaymentMutationImplemented);
    }
}
