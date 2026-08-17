using TravelCore.Modules.Visa.Contracts;
using Xunit;

namespace TravelCore.Modules.Visa.UnitTests;

/// <summary>
/// TC-P17-T008: Visa policy ownership stays in Visa; applicant case workflow is deferred.
/// </summary>
public sealed class VisaApplicationBoundaryTests
{
    [Fact]
    public void VisaApplicationBoundary_Separates_Policy_From_Future_Case()
    {
        Assert.Equal("Visa", VisaApplicationBoundary.VisaPolicyOwner);
        Assert.Equal("VisaApplication", VisaApplicationBoundary.FutureCaseOwner);
        Assert.True(VisaApplicationBoundary.VisaPolicyCompleteInP17);
        Assert.False(VisaApplicationBoundary.VisaApplicationImplemented);
        Assert.True(VisaApplicationBoundary.DeferredToFutureCapability);
        Assert.False(VisaApplicationBoundary.VisaEqualsVisaApplication);
    }

    [Fact]
    public void VisaApplicationBoundary_Separates_Future_Case_From_Booking_And_Payment()
    {
        Assert.Equal("Booking", VisaApplicationBoundary.BookingOwner);
        Assert.Equal("Payment", VisaApplicationBoundary.PaymentOwner);
        Assert.Equal("Pricing", VisaApplicationBoundary.PricingOwner);
        Assert.False(VisaApplicationBoundary.VisaApplicationEqualsBooking);
        Assert.False(VisaApplicationBoundary.VisaApplicationEqualsPayment);
        Assert.False(VisaApplicationBoundary.OfficialVisaFeeEqualsPaymentAmount);
        Assert.False(VisaApplicationBoundary.OfficialVisaFeeEqualsCommercialPrice);
        Assert.False(VisaApplicationBoundary.CommercialPriceCalculationAllowed);
    }

    [Fact]
    public void VisaApplicationBoundary_Preserves_Document_And_Privacy_Posture()
    {
        Assert.False(VisaApplicationBoundary.RequiredDocumentEqualsApplicantSubmittedDocument);
        Assert.False(VisaApplicationBoundary.VisaPolicyDataContainsApplicantPii);
        Assert.False(VisaApplicationBoundary.PublicVisaApiExposesPrivateCaseData);
        Assert.False(VisaApplicationBoundary.PrivateApplicationApiImplemented);
        Assert.False(VisaApplicationBoundary.ApplicantMasterEntityAllowed);
        Assert.Equal("OpaqueLogicalApplicantReference", VisaApplicationBoundary.ApplicantReferencePosture);
    }

    [Fact]
    public void VisaApplicationBoundary_Forbids_P17_Transactional_Capabilities()
    {
        Assert.False(VisaApplicationBoundary.DocumentUploadAllowed);
        Assert.False(VisaApplicationBoundary.OcrAllowed);
        Assert.False(VisaApplicationBoundary.AppointmentSchedulingAllowed);
        Assert.False(VisaApplicationBoundary.ExternalEmbassyIntegrationAllowed);
        Assert.False(VisaApplicationBoundary.CaseLifecycleStateMachineAllowed);
        Assert.False(VisaApplicationBoundary.ApplicantDocumentMediaRelationAllowed);
        Assert.False(VisaApplicationBoundary.P17VisaIsGenericWorkflowEngine);
        Assert.False(VisaApplicationBoundary.AiEligibilityDecisionAllowed);
        Assert.False(VisaApplicationBoundary.AiApprovalPredictionAllowed);
        Assert.False(VisaApplicationBoundary.RagOverPrivateApplicationDataAllowed);
    }

    [Fact]
    public void VisaOwnershipBoundary_Records_R8_Deferred_Posture()
    {
        Assert.True(VisaOwnershipBoundary.VisaPolicyCapabilityCompleteInP17);
        Assert.False(VisaOwnershipBoundary.VisaApplicationCapabilityImplemented);
        Assert.False(VisaOwnershipBoundary.ApplicationWorkflowImplemented);
        Assert.False(VisaOwnershipBoundary.OwnsApplicantCase);
        Assert.False(VisaOwnershipBoundary.OwnsApplicantPii);
        Assert.True(VisaOwnershipBoundary.RequiredDocumentIsRequirementDefinitionOnly);
        Assert.False(VisaOwnershipBoundary.ApplicantSubmittedDocumentImplemented);
        Assert.False(VisaOwnershipBoundary.AppointmentSchedulingImplemented);
        Assert.False(VisaOwnershipBoundary.ExternalEmbassyIntegrationImplemented);
        Assert.False(VisaOwnershipBoundary.GenericWorkflowEngineImplemented);
        Assert.False(VisaOwnershipBoundary.OwnsBooking);
        Assert.False(VisaOwnershipBoundary.OwnsPayment);
        Assert.False(VisaOwnershipBoundary.OwnsPricing);
        Assert.False(VisaOwnershipBoundary.OwnsIdentityOrParty);
    }

    [Fact]
    public void VisaPublicCompositionBoundary_Remains_Informational_Only()
    {
        Assert.False(VisaPublicCompositionBoundary.ApplicationWorkflowAllowed);
        Assert.False(VisaPublicCompositionBoundary.DocumentUploadAllowed);
        Assert.False(VisaPublicCompositionBoundary.AppointmentBookingAllowed);
        Assert.False(VisaPublicCompositionBoundary.PaymentCtaAllowed);
        Assert.False(VisaPublicCompositionBoundary.PrivateCaseDataExposureAllowed);
    }
}
