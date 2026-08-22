using TravelCore.Modules.AgencyMarketplace.Contracts;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Policies;

/// <summary>
/// Default commercial policy stub (P38-T010). Always allows — extension point only.
/// </summary>
internal sealed class AllowAgencyOfferCommercialPolicy : IAgencyOfferCommercialPolicy
{
    public Task EnsureAllowsAsync(AgencyOfferPolicyContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Default content policy stub (P38-T010). Always allows — extension point only.
/// </summary>
internal sealed class AllowAgencyOfferContentPolicy : IAgencyOfferContentPolicy
{
    public Task EnsureAllowsAsync(AgencyOfferPolicyContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Default channel policy stub (P38-T010). Always allows — extension point only.
/// </summary>
internal sealed class AllowAgencyOfferChannelPolicy : IAgencyOfferChannelPolicy
{
    public Task EnsureAllowsAsync(AgencyOfferPolicyContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Task.CompletedTask;
    }
}
