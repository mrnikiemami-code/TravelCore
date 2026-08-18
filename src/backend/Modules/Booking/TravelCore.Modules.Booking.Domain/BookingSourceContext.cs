namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Booking-owned transaction-origin context (TC-P19-T007 / P19-R7).
/// Direct and Agency use the same Booking aggregate. AgencyOfferReference is optional:
/// an agency-mediated Booking may originate from an accepted AgencyProfile without a
/// specific marketplace Offer. References are logical only — no AgencyMarketplace clone.
/// </summary>
public sealed class BookingSourceContext
{
    private BookingSourceContext()
    {
    }

    private BookingSourceContext(
        BookingSourceKind kind,
        AgencyProfileReference? agencyProfile,
        AgencyOfferReference? agencyOffer)
    {
        Kind = kind;
        AgencyProfile = agencyProfile;
        AgencyOffer = agencyOffer;
        EnsureInvariants();
    }

    public BookingSourceKind Kind { get; private set; }

    public AgencyProfileReference? AgencyProfile { get; private set; }

    public AgencyOfferReference? AgencyOffer { get; private set; }

    public static BookingSourceContext Direct() =>
        new(BookingSourceKind.Direct, agencyProfile: null, agencyOffer: null);

    public static BookingSourceContext ForAgency(
        AgencyProfileReference agencyProfile,
        AgencyOfferReference? agencyOffer = null) =>
        new(BookingSourceKind.Agency, agencyProfile, agencyOffer);

    public static BookingSourceContext Create(
        BookingSourceKind kind,
        AgencyProfileReference? agencyProfile = null,
        AgencyOfferReference? agencyOffer = null) =>
        new(kind, agencyProfile, agencyOffer);

    private void EnsureInvariants()
    {
        if (!Enum.IsDefined(Kind))
        {
            throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "BookingSourceKind is not a controlled source kind.");
        }

        if (Kind == BookingSourceKind.Direct)
        {
            if (AgencyProfile is not null || AgencyOffer is not null)
            {
                throw new InvalidOperationException(
                    "Direct Booking cannot carry AgencyProfileReference or AgencyOfferReference.");
            }

            return;
        }

        if (Kind != BookingSourceKind.Agency)
        {
            throw new InvalidOperationException("BookingSourceKind must be Direct or Agency.");
        }

        if (AgencyProfile is not { AgencyProfileId: var profileId } || profileId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Agency-originated Booking requires AgencyProfileReference. AgencyOfferReference is optional.");
        }

        if (AgencyOffer is { AgencyOfferId: var offerId } && offerId == Guid.Empty)
        {
            throw new ArgumentException("AgencyOfferReference cannot be empty.");
        }
    }
}
