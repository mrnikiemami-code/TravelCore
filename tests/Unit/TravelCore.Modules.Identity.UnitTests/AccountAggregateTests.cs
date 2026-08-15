using NodaTime;
using TravelCore.Modules.Identity.Domain;
using TravelCore.Modules.Identity.Infrastructure.Security;
using AccountAggregate = TravelCore.Modules.Identity.Domain.Account;
using Xunit;

namespace TravelCore.Modules.Identity.UnitTests;

public sealed class AccountAggregateTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 15, 14, 0);

    [Fact]
    public void Create_AssignsUuidV7_AndNormalizesEmail()
    {
        var account = AccountAggregate.Create("Ada@Example.com", "HASH", Now);

        Assert.NotEqual(Guid.Empty, account.Id.Value);
        Assert.Equal(7, account.Id.Value.Version);
        Assert.Equal("Ada@Example.com", account.Email);
        Assert.Equal("ADA@EXAMPLE.COM", account.NormalizedEmail);
        Assert.Equal(AccountStatus.Active, account.Status);
        Assert.Null(account.AssociatedPartyId);
    }

    [Fact]
    public void Create_AllowsOpaqueAssociatedPartyId_WithoutOwningParty()
    {
        var partyId = Guid.CreateVersion7();
        var account = AccountAggregate.Create("user@example.com", "HASH", Now, partyId);

        Assert.Equal(partyId, account.AssociatedPartyId);
    }

    [Fact]
    public void Create_RejectsEmptyAssociatedPartyId()
    {
        Assert.Throws<ArgumentException>(() =>
            AccountAggregate.Create("user@example.com", "HASH", Now, Guid.Empty));
    }

    [Fact]
    public void LinkReplaceUnlink_AssociationRules()
    {
        var account = AccountAggregate.Create("user@example.com", "HASH", Now);
        var partyA = Guid.CreateVersion7();
        var partyB = Guid.CreateVersion7();
        var t1 = Now.Plus(Duration.FromSeconds(1));
        var t2 = Now.Plus(Duration.FromSeconds(2));
        var t3 = Now.Plus(Duration.FromSeconds(3));

        account.LinkAssociatedParty(partyA, t1);
        Assert.Equal(partyA, account.AssociatedPartyId);

        Assert.Throws<InvalidOperationException>(() => account.LinkAssociatedParty(partyB, t2));

        account.ReplaceAssociatedParty(partyB, t2);
        Assert.Equal(partyB, account.AssociatedPartyId);

        account.UnlinkAssociatedParty(t3);
        Assert.Null(account.AssociatedPartyId);
    }

    [Fact]
    public void Disable_SetsDisabledLifecycle()
    {
        var account = AccountAggregate.Create("user@example.com", "HASH", Now);
        var later = Now.Plus(Duration.FromMinutes(1));
        account.Disable(later);
        Assert.Equal(AccountStatus.Disabled, account.Status);
        Assert.Equal(later, account.UpdatedAt);
    }
}

public sealed class AspNetCoreIdentityPasswordHasherTests
{
    [Fact]
    public void Hash_IsNotPlaintext_AndVerifies()
    {
        var hasher = new AspNetCoreIdentityPasswordHasher();
        const string password = "Correct-Horse-Battery-Staple-9";

        var hash = hasher.HashPassword(password);

        Assert.False(string.Equals(hash, password, StringComparison.Ordinal));
        Assert.DoesNotContain(password, hash, StringComparison.Ordinal);
        Assert.True(hasher.VerifyHashedPassword(hash, password));
        Assert.False(hasher.VerifyHashedPassword(hash, "wrong-password"));
    }
}
