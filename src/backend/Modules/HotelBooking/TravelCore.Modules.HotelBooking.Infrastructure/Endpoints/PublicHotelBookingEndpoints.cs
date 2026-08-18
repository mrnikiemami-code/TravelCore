using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.HotelBooking.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Endpoints;

/// <summary>
/// Public HotelBooking transactional journey (TC-P21-T008 / P21-R8).
/// Group: /api/hotel-booking/public
/// Header: X-TravelCore-Hotel-Booking-Access-Token
/// No generic list, CRUD, Refund command, or operational HTTP surface.
/// </summary>
internal static class PublicHotelBookingEndpoints
{
    public static IEndpointRouteBuilder MapPublicHotelBookingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(PublicHotelBookingCompositionBoundary.PublicApiGroup)
            .WithTags("HotelBookingPublic")
            .AllowAnonymous();

        group.MapPost("/initiations", async Task<IResult> (
            PublicHotelBookingInitiationRequest? request,
            HttpContext httpContext,
            IPublicHotelBookingInitiationService initiation,
            CancellationToken cancellationToken) =>
        {
            if (request is null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["request"] = ["Request body is required."],
                });
            }

            var merged = MergeIdempotency(request, httpContext);
            try
            {
                var created = await initiation.InitiateAsync(
                    merged,
                    PublicHotelBookingActorClaims.TryReadActorId(httpContext.User),
                    cancellationToken);
                return Results.Created(
                    $"{PublicHotelBookingCompositionBoundary.PublicApiGroup}/{created.HotelBookingId:D}",
                    created);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "Public HotelBooking initiation rejected.");
            }
        });

        group.MapGet("/{hotelBookingId:guid}", async Task<IResult> (
            Guid hotelBookingId,
            HttpContext httpContext,
            IPublicHotelBookingReadService reads,
            CancellationToken cancellationToken) =>
        {
            var read = await reads.GetAuthorizedAsync(
                hotelBookingId,
                ReadAccessToken(httpContext),
                PublicHotelBookingActorClaims.TryReadActorId(httpContext.User),
                cancellationToken);
            return read is null ? Results.NotFound() : Results.Ok(read);
        });

        group.MapPost("/{hotelBookingId:guid}/availability", async Task<IResult> (
            Guid hotelBookingId,
            HttpContext httpContext,
            IPublicHotelBookingJourneyService journey,
            CancellationToken cancellationToken) =>
            await ProgressAsync(
                hotelBookingId,
                httpContext,
                journey.RequestAvailabilityAsync,
                "Availability source is not configured.",
                cancellationToken));

        group.MapPost("/{hotelBookingId:guid}/rate-offers", async Task<IResult> (
            Guid hotelBookingId,
            HttpContext httpContext,
            IPublicHotelBookingJourneyService journey,
            CancellationToken cancellationToken) =>
            await ProgressAsync(
                hotelBookingId,
                httpContext,
                journey.RequestRateOfferAsync,
                "Rate source is not configured.",
                cancellationToken));

        group.MapGet("/{hotelBookingId:guid}/payment", async Task<IResult> (
            Guid hotelBookingId,
            HttpContext httpContext,
            IPublicHotelBookingReadService reads,
            IPublicHotelBookingPaymentService payments,
            CancellationToken cancellationToken) =>
        {
            var booking = await reads.GetAuthorizedAsync(
                hotelBookingId,
                ReadAccessToken(httpContext),
                PublicHotelBookingActorClaims.TryReadActorId(httpContext.User),
                cancellationToken);
            if (booking is null)
            {
                return Results.NotFound();
            }

            var payment = await payments.GetByHotelBookingIdAsync(hotelBookingId, cancellationToken);
            return Results.Ok(PublicHotelBookingMapper.ToPayment(booking, payment));
        });

        group.MapPost("/{hotelBookingId:guid}/payment/initiation", async Task<IResult> (
            Guid hotelBookingId,
            PublicPaymentInitiationRequest? request,
            HttpContext httpContext,
            IPublicHotelBookingReadService reads,
            IPublicHotelBookingPaymentService payments,
            CancellationToken cancellationToken) =>
        {
            var booking = await reads.GetAuthorizedAsync(
                hotelBookingId,
                ReadAccessToken(httpContext),
                PublicHotelBookingActorClaims.TryReadActorId(httpContext.User),
                cancellationToken);
            if (booking is null)
            {
                return Results.NotFound();
            }

            var idempotency = request?.IdempotencyKey;
            if (string.IsNullOrWhiteSpace(idempotency)
                && httpContext.Request.Headers.TryGetValue(
                    PublicHotelBookingCompositionBoundary.IdempotencyHeader,
                    out var values)
                && !string.IsNullOrWhiteSpace(values.ToString()))
            {
                idempotency = values.ToString();
            }

            var result = await payments.InitiateForHotelBookingAsync(hotelBookingId, idempotency, cancellationToken);
            var refreshed = await reads.GetAuthorizedAsync(
                hotelBookingId,
                ReadAccessToken(httpContext),
                PublicHotelBookingActorClaims.TryReadActorId(httpContext.User),
                cancellationToken) ?? booking;
            var body = PublicHotelBookingMapper.ToPayment(refreshed, result.Payment);
            return result.Status switch
            {
                PublicPaymentCommandStatus.ProviderUnavailable => Results.Problem(
                    detail: "Online payment is not currently available.",
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Payment provider is not configured.",
                    extensions: new Dictionary<string, object?> { ["payment"] = body }),
                PublicPaymentCommandStatus.BookingIneligible => Results.Problem(
                    detail: "HotelBooking is not eligible for Payment initiation.",
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "Payment initiation rejected."),
                _ => Results.Ok(body),
            };
        });

        group.MapPost("/{hotelBookingId:guid}/cancellation", async Task<IResult> (
            Guid hotelBookingId,
            PublicPaymentInitiationRequest? request,
            HttpContext httpContext,
            IPublicHotelBookingJourneyService journey,
            CancellationToken cancellationToken) =>
        {
            var idempotency = request?.IdempotencyKey;
            if (string.IsNullOrWhiteSpace(idempotency)
                && httpContext.Request.Headers.TryGetValue(
                    PublicHotelBookingCompositionBoundary.IdempotencyHeader,
                    out var values)
                && !string.IsNullOrWhiteSpace(values.ToString()))
            {
                idempotency = values.ToString();
            }

            try
            {
                var result = await journey.RequestCancellationAsync(
                    hotelBookingId,
                    ReadAccessToken(httpContext),
                    PublicHotelBookingActorClaims.TryReadActorId(httpContext.User),
                    idempotency,
                    cancellationToken);
                if (result is null)
                {
                    return Results.NotFound();
                }

                return result.Outcome switch
                {
                    nameof(HotelBookingCancellationRequestOutcome.PartialRefundRequiredButUnsupported) =>
                        Results.Problem(
                            detail: "This cancellation requires a partial refund, which is not currently executable.",
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "PartialRefundRequiredButUnsupported",
                            extensions: new Dictionary<string, object?> { ["booking"] = result.Booking }),
                    nameof(HotelBookingCancellationRequestOutcome.PendingCustomerCancellationUnsupported) =>
                        Results.Problem(
                            detail: "This HotelBooking cannot be cancelled in its current state.",
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "Cancellation rejected."),
                    nameof(HotelBookingCancellationRequestOutcome.MissingPaymentEvidence) =>
                        Results.Problem(
                            detail: "Cancellation cannot start without authoritative Payment evidence.",
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "Cancellation rejected."),
                    nameof(HotelBookingCancellationRequestOutcome.PolicyAmbiguous) =>
                        Results.Problem(
                            detail: "Cancellation terms cannot be evaluated for an executable outcome.",
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "Cancellation rejected."),
                    nameof(HotelBookingCancellationRequestOutcome.UnconfiguredReservationSource) =>
                        Results.Problem(
                            detail: "Hotel reservation source is not currently available.",
                            statusCode: StatusCodes.Status503ServiceUnavailable,
                            title: "Reservation source is not configured.",
                            extensions: new Dictionary<string, object?> { ["booking"] = result.Booking }),
                    _ => Results.Ok(result),
                };
            }
            catch (UnauthorizedAccessException)
            {
                return Results.NotFound();
            }
        });

        return endpoints;
    }

    private static async Task<IResult> ProgressAsync(
        Guid hotelBookingId,
        HttpContext httpContext,
        Func<Guid, string?, Guid?, string?, CancellationToken, Task<PublicHotelBookingProgressResult>> action,
        string unavailableDetail,
        CancellationToken cancellationToken)
    {
        try
        {
            var idempotency = httpContext.Request.Headers.TryGetValue(
                PublicHotelBookingCompositionBoundary.IdempotencyHeader,
                out var values)
                ? values.ToString()
                : null;
            var result = await action(
                hotelBookingId,
                ReadAccessToken(httpContext),
                PublicHotelBookingActorClaims.TryReadActorId(httpContext.User),
                idempotency,
                cancellationToken);
            return result.Status switch
            {
                PublicHotelBookingJourneyStatus.SourceUnavailable => Results.Problem(
                    detail: unavailableDetail,
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Hotel source is not configured.",
                    extensions: new Dictionary<string, object?> { ["booking"] = result.Booking }),
                PublicHotelBookingJourneyStatus.Ineligible => Results.Problem(
                    detail: "HotelBooking is not eligible for this step.",
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "HotelBooking progression rejected."),
                _ => Results.Ok(result.Booking),
            };
        }
        catch (UnauthorizedAccessException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "HotelBooking progression rejected.");
        }
    }

    private static string? ReadAccessToken(HttpContext httpContext)
    {
        httpContext.Request.Headers.TryGetValue(
            PublicHotelBookingCompositionBoundary.AccessTokenHeader,
            out var tokenValues);
        return tokenValues.ToString();
    }

    private static PublicHotelBookingInitiationRequest MergeIdempotency(
        PublicHotelBookingInitiationRequest request,
        HttpContext httpContext)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            return request;
        }

        if (httpContext.Request.Headers.TryGetValue(
                PublicHotelBookingCompositionBoundary.IdempotencyHeader,
                out var values)
            && !string.IsNullOrWhiteSpace(values.ToString()))
        {
            return request with { IdempotencyKey = values.ToString() };
        }

        return request;
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "request"] = [ex.Message],
        });
}
