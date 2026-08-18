using System.Security.Claims;
using TravelCore.Modules.HotelBooking.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

internal static class PublicHotelBookingActorClaims
{
    public static Guid? TryReadActorId(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var raw = user.FindFirst(PublicHotelBookingCompositionBoundary.ActorAccountIdClaimType)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) && id != Guid.Empty ? id : null;
    }
}
