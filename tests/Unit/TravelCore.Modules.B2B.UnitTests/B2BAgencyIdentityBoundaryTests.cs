using TravelCore.Modules.B2B.Domain;
using Xunit;

namespace TravelCore.Modules.B2B.UnitTests;

public sealed class B2BAgencyIdentityBoundaryTests
{
    [Fact]
    public void AgencyReference_Links_To_PartyOwned_Agency_Logically()
    {
        var partyAgencyId = AgencyReferenceId.New();
        var reference = AgencyReference.FromPartyAgency(partyAgencyId);
        Assert.Equal(partyAgencyId, reference.PartyAgencyId);
    }

    [Fact]
    public void AgencyMembershipBoundary_Uses_AccessSubjectReference()
    {
        var agency = AgencyReference.FromPartyAgency(AgencyReferenceId.New());
        var subjectId = AccessSubjectReferenceId.New();
        var boundary = AgencyMembershipBoundary.DescribeMembership(agency, subjectId);
        Assert.Equal(agency.PartyAgencyId, boundary.Agency.PartyAgencyId);
        Assert.Equal(subjectId, boundary.AccessSubjectId);
    }

    [Fact]
    public void AgencyRelationshipBoundary_Preserves_Ownership_Posture()
    {
        Assert.Equal("Agency is a business concept, not Identity", AgencyRelationshipBoundary.AgencyIsBusinessConcept);
        Assert.Equal("Agency users are Access subjects", AgencyRelationshipBoundary.AgencyUsersAreAccessSubjects);
        Assert.Equal("Agency organization relationship belongs to Party", AgencyRelationshipBoundary.AgencyOrganizationOwnedByParty);
        Assert.Equal("Party", AgencyRelationshipBoundary.PartyIdentityOwner);
        Assert.Equal("Access", AgencyRelationshipBoundary.AccessAuthorizationOwner);
        Assert.Equal("Identity", AgencyRelationshipBoundary.IdentityCredentialOwner);
        Assert.False(AgencyRelationshipBoundary.B2BOwnsPartyOrganizationData);
        Assert.False(AgencyRelationshipBoundary.B2BOwnsIdentityCredentials);
        Assert.False(AgencyRelationshipBoundary.B2BOwnsAccessAuthorization);
        Assert.False(AgencyRelationshipBoundary.B2BOwnsAgencyAggregate);
        Assert.False(AgencyRelationshipBoundary.AgencyPersistenceImplemented);
        Assert.False(AgencyRelationshipBoundary.BookingRelationImplemented);
        Assert.False(AgencyRelationshipBoundary.PaymentRelationImplemented);
    }

    [Fact]
    public void Domain_Does_Not_Define_Forbidden_Product_Types()
    {
        var domain = typeof(B2BDomainAssemblyMarker).Assembly;
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.Agency"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.AgencyUser"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.Contract"));
        Assert.Null(domain.GetType("TravelCore.Modules.B2B.Domain.Commission"));
    }
}
