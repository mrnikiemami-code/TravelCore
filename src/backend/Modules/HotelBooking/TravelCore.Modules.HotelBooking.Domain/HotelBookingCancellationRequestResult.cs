namespace TravelCore.Modules.HotelBooking.Domain;

public enum HotelBookingCancellationRequestOutcome
{
    Accepted = 1,
    PartialRefundRequiredButUnsupported = 2,
    MissingPaymentEvidence = 3,
    PendingCustomerCancellationUnsupported = 4,
    AlreadyCancelled = 5,
    PolicyAmbiguous = 6,
    UnconfiguredReservationSource = 7,
}

public sealed class HotelBookingCancellationRequestResult
{
    public HotelBookingCancellationRequestResult(
        HotelBookingCancellationRequestOutcome outcome,
        HotelBookingCancellation? cancellation = null,
        HotelCancellationPenaltyEvaluation? evaluation = null)
    {
        Outcome = outcome;
        Cancellation = cancellation;
        Evaluation = evaluation;
    }

    public HotelBookingCancellationRequestOutcome Outcome { get; }

    public HotelBookingCancellation? Cancellation { get; }

    public HotelCancellationPenaltyEvaluation? Evaluation { get; }
}
