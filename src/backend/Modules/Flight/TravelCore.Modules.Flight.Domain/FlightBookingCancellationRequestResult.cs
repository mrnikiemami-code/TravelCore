namespace TravelCore.Modules.Flight.Domain;

public enum FlightBookingCancellationRequestOutcome
{
    Accepted = 1,
    PartialRefundRequiredButUnsupported = 2,
    MissingPaymentEvidence = 3,
    PendingCustomerCancellationUnsupported = 4,
    AlreadyCancelled = 5,
    PolicyAmbiguous = 6,
    UnconfiguredCancellationSource = 7,
    SupplierEconomicsMismatch = 8,
}

public sealed class FlightBookingCancellationRequestResult
{
    public FlightBookingCancellationRequestResult(
        FlightBookingCancellationRequestOutcome outcome,
        FlightBookingCancellation? cancellation = null,
        FlightCancellationPenaltyEvaluation? evaluation = null)
    {
        Outcome = outcome;
        Cancellation = cancellation;
        Evaluation = evaluation;
    }

    public FlightBookingCancellationRequestOutcome Outcome { get; }

    public FlightBookingCancellation? Cancellation { get; }

    public FlightCancellationPenaltyEvaluation? Evaluation { get; }
}
