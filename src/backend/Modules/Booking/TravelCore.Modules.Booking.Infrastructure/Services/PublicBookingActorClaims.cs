using System.Security.Claims;
using TravelCore.Modules.Booking.Contracts;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

internal static class PublicBookingActorClaims
{
    public static Guid? TryReadActorId(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var raw = user.FindFirst(PublicBookingCompositionBoundary.ActorAccountIdClaimType)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) && id != Guid.Empty ? id : null;
    }
}
