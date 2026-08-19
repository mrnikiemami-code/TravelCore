using TravelCore.Modules.B2B.Domain;
using Xunit;

namespace TravelCore.Modules.B2B.UnitTests;

public sealed class B2BAgencyMembershipAccessBoundaryTests
{
    [Fact]
    public void AgencyMemberReference_Describes_Membership_Intent_Only()
    {
        var agency = AgencyReference.FromPartyAgency(AgencyReferenceId.New());
        var subjectId = AccessSubjectReferenceId.New();
        var member = AgencyMemberReference.DescribeIntent(agency, subjectId);

        Assert.Equal(agency.PartyAgencyId, member.Agency.PartyAgencyId);
        Assert.Equal(subjectId, member.AccessSubjectId);
    }

    [Fact]
    public void AgencyAccessRelationshipBoundary_Preserves_Access_Ownership()
    {
        Assert.Equal("B2B", AgencyAccessRelationshipBoundary.MembershipIntentOwner);
        Assert.Equal("Identity", AgencyAccessRelationshipBoundary.UserIdentityOwner);
        Assert.Equal("Access", AgencyAccessRelationshipBoundary.AuthorizationOwner);
        Assert.Equal("Party", AgencyAccessRelationshipBoundary.OrganizationRelationshipOwner);
        Assert.Equal("Agency users are Access subjects", AgencyAccessRelationshipBoundary.AgencyUsersAreAccessSubjects);
        Assert.False(AgencyAccessRelationshipBoundary.B2BOwnsUsers);
        Assert.False(AgencyAccessRelationshipBoundary.B2BOwnsAuthentication);
        Assert.False(AgencyAccessRelationshipBoundary.B2BOwnsAuthorization);
        Assert.False(AgencyAccessRelationshipBoundary.B2BOwnsAccessPolicies);
        Assert.False(AgencyAccessRelationshipBoundary.B2BOwnsRoles);
        Assert.False(AgencyAccessRelationshipBoundary.B2BOwnsPermissions);
        Assert.False(AgencyAccessRelationshipBoundary.B2BOwnsInvitationFlow);
        Assert.False(AgencyAccessRelationshipBoundary.AgencyMemberTableImplemented);
        Assert.False(AgencyAccessRelationshipBoundary.MembershipPersistenceImplemented);
    }

    [Fact]
    public void Domain_Does_Not_Define_Forbidden_Membership_Product()
    {
        var domain = typeof(B2BDomainAssemblyMarker).Assembly;
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.AgencyMember"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.AgencyUser"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.User"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.Role"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.Permission"));
    }
}
