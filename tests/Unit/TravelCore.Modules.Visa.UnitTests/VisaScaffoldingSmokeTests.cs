using TravelCore.Modules.Visa.Contracts;
using TravelCore.Modules.Visa.Domain;
using TravelCore.Modules.Visa.Infrastructure;
using Xunit;

namespace TravelCore.Modules.Visa.UnitTests;

public sealed class VisaScaffoldingSmokeTests
{
    [Fact]
    public void VisaContractsAssembly_IsLoadable()
    {
        var marker = typeof(VisaContractsAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Visa.Contracts", marker.Namespace);
        Assert.Equal("TravelCore.Modules.Visa.Contracts", marker.Assembly.GetName().Name);
    }

    [Fact]
    public void VisaDomainAssembly_IsLoadable()
    {
        var marker = typeof(VisaDomainAssemblyMarker);
        Assert.Equal("TravelCore.Modules.Visa.Domain", marker.Namespace);
    }

    [Fact]
    public void OwnershipBoundary_Keeps_Peer_SoT_Out_Of_Visa()
    {
        Assert.Equal("Visa", VisaOwnershipBoundary.OwnerModule);
        Assert.Equal("visa", VisaOwnershipBoundary.SchemaName);
        Assert.Equal("Destination", VisaOwnershipBoundary.GeographicOwner);
        Assert.Equal("ReferenceData", VisaOwnershipBoundary.ReferenceDataOwner);
        Assert.Equal("Content", VisaOwnershipBoundary.EditorialOwner);
        Assert.Equal("Media", VisaOwnershipBoundary.MediaAssetOwner);
        Assert.Equal("Pricing", VisaOwnershipBoundary.PriceOwner);
        Assert.Equal("Seo", VisaOwnershipBoundary.IndexPolicyOwner);
        Assert.Equal("Search", VisaOwnershipBoundary.SearchOwner);
        Assert.Equal("Booking", VisaOwnershipBoundary.BookingOwner);
        Assert.Equal("Payment", VisaOwnershipBoundary.PaymentOwner);
        Assert.Equal("OpaqueLogicalGeographicId", VisaOwnershipBoundary.GeographicReferencePosture);
        Assert.False(VisaOwnershipBoundary.OwnsDestinationFacts);
        Assert.False(VisaOwnershipBoundary.OwnsReferenceData);
        Assert.False(VisaOwnershipBoundary.OwnsContentCms);
        Assert.False(VisaOwnershipBoundary.OwnsMediaAssetTruth);
        Assert.False(VisaOwnershipBoundary.OwnsPricing);
        Assert.False(VisaOwnershipBoundary.OwnsQuote);
        Assert.False(VisaOwnershipBoundary.OwnsBooking);
        Assert.False(VisaOwnershipBoundary.OwnsPayment);
        Assert.False(VisaOwnershipBoundary.OwnsIndexPolicy);
        Assert.False(VisaOwnershipBoundary.OwnsSearch);
        Assert.False(VisaOwnershipBoundary.OwnsIdentityOrParty);
        Assert.True(VisaOwnershipBoundary.GeographicReferencesAreLogicalOnly);
        Assert.False(VisaOwnershipBoundary.GeographicReferencesAreSourceOfTruth);
        Assert.True(VisaOwnershipBoundary.FutureEffectivePeriodAllowed);
        Assert.True(VisaOwnershipBoundary.FutureProvenanceAllowed);
        Assert.True(VisaOwnershipBoundary.FutureVerificationTimestampAllowed);
        Assert.True(VisaOwnershipBoundary.FutureJurisdictionContextAllowed);
        Assert.False(VisaOwnershipBoundary.RegulatoryEngineImplemented);
        Assert.True(VisaOwnershipBoundary.VisaDefinitionImplemented);
        Assert.True(VisaOwnershipBoundary.VisaRequirementSetImplemented);
        Assert.False(VisaOwnershipBoundary.VisaRequirementImplemented);
        Assert.False(VisaOwnershipBoundary.RequiredDocumentImplemented);
        Assert.False(VisaOwnershipBoundary.EligibilityModelImplemented);
        Assert.False(VisaOwnershipBoundary.ProcessingValidityModelImplemented);
        Assert.False(VisaOwnershipBoundary.FeeModelImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
    }

    [Fact]
    public void GeographicReference_Is_Opaque_Logical_Id_Not_A_Country_Entity()
    {
        var geographicId = Guid.Parse("0198b3e0-0000-7000-8000-000000000031");
        var reference = new VisaGeographicReference(geographicId);
        Assert.Equal(geographicId, reference.GeographicId);
        Assert.Equal("VisaGeographicReference", nameof(VisaGeographicReference));
        Assert.False(typeof(VisaGeographicReference).IsClass);
    }

    [Fact]
    public void VisaDbContext_Owns_Schema_visa()
    {
        Assert.Equal("visa", VisaDbContext.SchemaName);
        Assert.Equal(VisaOwnershipBoundary.SchemaName, VisaDbContext.SchemaName);
    }
}
