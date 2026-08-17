namespace TravelCore.Modules.Visa.Contracts;

/// <summary>
/// P17-R8: Visa owns visa policy / structured facts. Applicant-specific case workflow is
/// explicitly deferred to a future VisaApplication capability outside P17.
/// </summary>
public static class VisaApplicationBoundary
{
    public const string VisaPolicyOwner = "Visa";
    public const string FutureCaseOwner = "VisaApplication";
    public const string BookingOwner = "Booking";
    public const string PaymentOwner = "Payment";
    public const string PricingOwner = "Pricing";
    public const string IdentityOwner = "Identity";
    public const string PartyOwner = "Party";
    public const string MediaOwner = "Media";
    public const string ApplicantReferencePosture = "OpaqueLogicalApplicantReference";
    public const bool VisaPolicyCompleteInP17 = true;
    public const bool VisaApplicationImplemented = false;
    public const bool DeferredToFutureCapability = true;
    public const bool VisaEqualsVisaApplication = false;
    public const bool VisaApplicationEqualsBooking = false;
    public const bool VisaApplicationEqualsPayment = false;
    public const bool RequiredDocumentEqualsApplicantSubmittedDocument = false;
    public const bool OfficialVisaFeeEqualsPaymentAmount = false;
    public const bool OfficialVisaFeeEqualsCommercialPrice = false;
    public const bool VisaPolicyDataContainsApplicantPii = false;
    public const bool PublicVisaApiExposesPrivateCaseData = false;
    public const bool PrivateApplicationApiImplemented = false;
    public const bool P17VisaIsGenericWorkflowEngine = false;
    public const bool DocumentUploadAllowed = false;
    public const bool OcrAllowed = false;
    public const bool AppointmentSchedulingAllowed = false;
    public const bool ExternalEmbassyIntegrationAllowed = false;
    public const bool CaseLifecycleStateMachineAllowed = false;
    public const bool ApplicantMasterEntityAllowed = false;
    public const bool CommercialPriceCalculationAllowed = false;
    public const bool ApplicantDocumentMediaRelationAllowed = false;
    public const bool AiEligibilityDecisionAllowed = false;
    public const bool AiApprovalPredictionAllowed = false;
    public const bool RagOverPrivateApplicationDataAllowed = false;
}
