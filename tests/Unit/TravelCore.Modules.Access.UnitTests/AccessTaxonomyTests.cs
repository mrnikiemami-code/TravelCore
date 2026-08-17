using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using Xunit;

namespace TravelCore.Modules.Access.UnitTests;

public sealed class AccessTaxonomyTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 15, 16, 0);

    [Fact]
    public void Permission_NormalizesCode_ToLower()
    {
        var permission = Permission.Create("Identity.Accounts.Read", "Read accounts", Now);
        Assert.Equal("identity.accounts.read", permission.Code);
        Assert.Equal(7, permission.Id.Value.Version);
    }

    [Fact]
    public void Role_GrantPermission_IsIdempotent()
    {
        var role = Role.Create("admin", "Administrator", Now);
        var permissionId = PermissionId.New();

        role.GrantPermission(permissionId, Now);
        role.GrantPermission(permissionId, Now.Plus(Duration.FromSeconds(1)));

        Assert.Single(role.Permissions);
        Assert.Equal(permissionId, role.Permissions.First().PermissionId);
    }

    [Fact]
    public void Role_RevokePermission_RemovesMembership()
    {
        var role = Role.Create("ops", "Operations", Now);
        var permissionId = PermissionId.New();
        role.GrantPermission(permissionId, Now);
        role.RevokePermission(permissionId);
        Assert.Empty(role.Permissions);
    }

    [Fact]
    public void AdminBaselineCatalog_IsExplicit_AndNonEmpty()
    {
        Assert.NotEmpty(AccessPermissionCatalog.AdminBaseline);
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "access.roles.write");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "seo.destination-posture.write");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "media.assets.write");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "place.places.write");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "seo.place-posture.write");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "content.items.write");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "seo.content-posture.write");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "tour.products.write");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "tour.departures.read");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "tour.departures.write");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "seo.tour-posture.write");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "pricing.prices.read");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "pricing.prices.write");
        Assert.Contains(AccessPermissionCatalog.AdminBaseline, x => x.Code == "agency.marketplace.offers.moderate");
        Assert.DoesNotContain(AccessPermissionCatalog.AgencyPresentationBaseline, x => x.Code == "agency.marketplace.offers.moderate");
        Assert.Equal("admin", AccessPermissionCatalog.AdminRoleCode);
    }
}

public sealed class AccessEvaluationContractTests
{
    [Fact]
    public void EvaluateAccessResponse_Defaults_ToExplicitDecisionFields()
    {
        var deny = new EvaluateAccessResponse
        {
            Allowed = false,
            PermissionCode = "x",
            Decision = "Deny",
            Reason = "Deny-by-default"
        };
        Assert.False(deny.Allowed);
        Assert.Equal("Deny", deny.Decision);
    }
}
