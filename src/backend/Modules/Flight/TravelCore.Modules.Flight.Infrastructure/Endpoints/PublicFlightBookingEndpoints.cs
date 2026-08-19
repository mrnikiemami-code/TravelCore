using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure.Endpoints;

/// <summary>
/// Public FlightBooking transactional journey (TC-P22-T008 / P22-R8).
/// Group: /api/flight-booking/public
/// Header: X-TravelCore-Flight-Booking-Access-Token
/// No generic list, CRUD, Refund command, or operational HTTP surface.
/// </summary>
internal static class PublicFlightBookingEndpoints
{
    public static IEndpointRouteBuilder MapPublicFlightBookingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(PublicFlightBookingCompositionBoundary.PublicApiGroup)
            .WithTags("FlightBookingPublic")
            .AllowAnonymous();

        group.MapPost("/search", async Task<IResult> (
            PublicFlightSearchRequest? request,
            IPublicFlightBookingSearchService search,
            CancellationToken cancellationToken) =>
        {
            if (request is null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["request"] = ["Request body is required."],
                });
            }

            try
            {
                var result = await search.SearchAsync(request, cancellationToken);
                return result.SourceConfigured
                    ? Results.Ok(result)
                    : Results.Problem(
                        detail: result.SafeMessage ?? "Flight search is not currently available.",
                        statusCode: StatusCodes.Status503ServiceUnavailable,
                        title: "Flight search source is not configured.",
                        extensions: new Dictionary<string, object?> { ["search"] = result });
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        group.MapPost("/initiations", async Task<IResult> (
            PublicFlightBookingInitiationRequest? request,
            HttpContext httpContext,
            IPublicFlightBookingInitiationService initiation,
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
                    PublicFlightBookingActorClaims.TryReadActorId(httpContext.User),
                    cancellationToken);
                return Results.Created(
                    $"{PublicFlightBookingCompositionBoundary.PublicApiGroup}/{created.FlightBookingId:D}",
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
                    title: "Public FlightBooking initiation rejected.");
            }
        });

        group.MapGet("/{flightBookingId:guid}", async Task<IResult> (
            Guid flightBookingId,
            HttpContext httpContext,
            IPublicFlightBookingReadService reads,
            CancellationToken cancellationToken) =>
        {
            var read = await reads.GetAuthorizedAsync(
                flightBookingId,
                ReadAccessToken(httpContext),
                PublicFlightBookingActorClaims.TryReadActorId(httpContext.User),
                cancellationToken);
            return read is null ? Results.NotFound() : Results.Ok(read);
        });

        group.MapPost("/{flightBookingId:guid}/offers", async Task<IResult> (
            Guid flightBookingId,
            HttpContext httpContext,
            IPublicFlightBookingJourneyService journey,
            CancellationToken cancellationToken) =>
            await ProgressAsync(
                flightBookingId,
                httpContext,
                journey.AcceptOfferAsync,
                "Flight offer source is not configured.",
                cancellationToken));

        group.MapPost("/{flightBookingId:guid}/reservations", async Task<IResult> (
            Guid flightBookingId,
            HttpContext httpContext,
            IPublicFlightBookingJourneyService journey,
            CancellationToken cancellationToken) =>
            await ProgressAsync(
                flightBookingId,
                httpContext,
                journey.RequestReservationAsync,
                "Flight reservation source is not configured.",
                cancellationToken));

        group.MapGet("/{flightBookingId:guid}/payment", async Task<IResult> (
            Guid flightBookingId,
            HttpContext httpContext,
            IPublicFlightBookingReadService reads,
            IPublicFlightBookingPaymentService payments,
            CancellationToken cancellationToken) =>
        {
            var booking = await reads.GetAuthorizedAsync(
                flightBookingId,
                ReadAccessToken(httpContext),
                PublicFlightBookingActorClaims.TryReadActorId(httpContext.User),
                cancellationToken);
            if (booking is null)
            {
                return Results.NotFound();
            }

            var payment = await payments.GetByFlightBookingIdAsync(flightBookingId, cancellationToken);
            return Results.Ok(PublicFlightBookingMapper.ToPayment(booking, payment));
        });

        group.MapPost("/{flightBookingId:guid}/payment/initiation", async Task<IResult> (
            Guid flightBookingId,
            PublicPaymentInitiationRequest? request,
            HttpContext httpContext,
            IPublicFlightBookingReadService reads,
            IPublicFlightBookingPaymentService payments,
            CancellationToken cancellationToken) =>
        {
            var booking = await reads.GetAuthorizedAsync(
                flightBookingId,
                ReadAccessToken(httpContext),
                PublicFlightBookingActorClaims.TryReadActorId(httpContext.User),
                cancellationToken);
            if (booking is null)
            {
                return Results.NotFound();
            }

            var idempotency = request?.IdempotencyKey;
            if (string.IsNullOrWhiteSpace(idempotency)
                && httpContext.Request.Headers.TryGetValue(
                    PublicFlightBookingCompositionBoundary.IdempotencyHeader,
                    out var values)
                && !string.IsNullOrWhiteSpace(values.ToString()))
            {
                idempotency = values.ToString();
            }

            var result = await payments.InitiateForFlightBookingAsync(flightBookingId, idempotency, cancellationToken);
            var refreshed = await reads.GetAuthorizedAsync(
                flightBookingId,
                ReadAccessToken(httpContext),
                PublicFlightBookingActorClaims.TryReadActorId(httpContext.User),
                cancellationToken) ?? booking;
            var body = PublicFlightBookingMapper.ToPayment(refreshed, result.Payment);
            return result.Status switch
            {
                PublicPaymentCommandStatus.ProviderUnavailable => Results.Problem(
                    detail: "Online payment is not currently available.",
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Payment provider is not configured.",
                    extensions: new Dictionary<string, object?> { ["payment"] = body }),
                PublicPaymentCommandStatus.BookingIneligible => Results.Problem(
                    detail: "FlightBooking is not eligible for Payment initiation.",
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "Payment initiation rejected."),
                _ => Results.Ok(body),
            };
        });

        group.MapPost("/{flightBookingId:guid}/cancellation", async Task<IResult> (
            Guid flightBookingId,
            PublicPaymentInitiationRequest? request,
            HttpContext httpContext,
            IPublicFlightBookingJourneyService journey,
            CancellationToken cancellationToken) =>
        {
            var idempotency = request?.IdempotencyKey;
            if (string.IsNullOrWhiteSpace(idempotency)
                && httpContext.Request.Headers.TryGetValue(
                    PublicFlightBookingCompositionBoundary.IdempotencyHeader,
                    out var values)
                && !string.IsNullOrWhiteSpace(values.ToString()))
            {
                idempotency = values.ToString();
            }

            try
            {
                var result = await journey.RequestCancellationAsync(
                    flightBookingId,
                    ReadAccessToken(httpContext),
                    PublicFlightBookingActorClaims.TryReadActorId(httpContext.User),
                    idempotency,
                    cancellationToken);
                if (result is null)
                {
                    return Results.NotFound();
                }

                return result.Outcome switch
                {
                    nameof(FlightBookingCancellationRequestOutcome.PartialRefundRequiredButUnsupported) =>
                        Results.Problem(
                            detail: "This cancellation requires a partial refund, which is not currently executable.",
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "PartialRefundRequiredButUnsupported",
                            extensions: new Dictionary<string, object?> { ["booking"] = result.Booking }),
                    nameof(FlightBookingCancellationRequestOutcome.PendingCustomerCancellationUnsupported) =>
                        Results.Problem(
                            detail: "This FlightBooking cannot be cancelled in its current state.",
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "Cancellation rejected."),
                    nameof(FlightBookingCancellationRequestOutcome.MissingPaymentEvidence) =>
                        Results.Problem(
                            detail: "Cancellation cannot start without authoritative Payment evidence.",
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "Cancellation rejected."),
                    nameof(FlightBookingCancellationRequestOutcome.PolicyAmbiguous) =>
                        Results.Problem(
                            detail: "Cancellation terms cannot be evaluated for an executable outcome.",
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "Cancellation rejected."),
                    nameof(FlightBookingCancellationRequestOutcome.UnconfiguredCancellationSource) =>
                        Results.Problem(
                            detail: "Flight cancellation source is not currently available.",
                            statusCode: StatusCodes.Status503ServiceUnavailable,
                            title: "Cancellation source is not configured.",
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
        Guid flightBookingId,
        HttpContext httpContext,
        Func<Guid, string?, Guid?, string?, CancellationToken, Task<PublicFlightBookingProgressResult>> action,
        string unavailableDetail,
        CancellationToken cancellationToken)
    {
        try
        {
            var idempotency = httpContext.Request.Headers.TryGetValue(
                PublicFlightBookingCompositionBoundary.IdempotencyHeader,
                out var values)
                ? values.ToString()
                : null;
            var result = await action(
                flightBookingId,
                ReadAccessToken(httpContext),
                PublicFlightBookingActorClaims.TryReadActorId(httpContext.User),
                idempotency,
                cancellationToken);
            return result.Status switch
            {
                PublicFlightBookingJourneyStatus.SourceUnavailable => Results.Problem(
                    detail: unavailableDetail,
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Flight source is not configured.",
                    extensions: new Dictionary<string, object?> { ["booking"] = result.Booking }),
                PublicFlightBookingJourneyStatus.OfferExpired => Results.Problem(
                    detail: "This offer has expired. A new quote is required.",
                    statusCode: StatusCodes.Status409Conflict,
                    title: "OfferExpired",
                    extensions: new Dictionary<string, object?> { ["booking"] = result.Booking }),
                PublicFlightBookingJourneyStatus.OfferRequoteRequired => Results.Problem(
                    detail: "The fare changed. A new quote is required.",
                    statusCode: StatusCodes.Status409Conflict,
                    title: "OfferRequoteRequired",
                    extensions: new Dictionary<string, object?> { ["booking"] = result.Booking }),
                PublicFlightBookingJourneyStatus.OfferUnavailable => Results.Problem(
                    detail: "This offer is no longer available.",
                    statusCode: StatusCodes.Status409Conflict,
                    title: "OfferUnavailable",
                    extensions: new Dictionary<string, object?> { ["booking"] = result.Booking }),
                PublicFlightBookingJourneyStatus.Ineligible => Results.Problem(
                    detail: "FlightBooking is not eligible for this step.",
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "FlightBooking progression rejected."),
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
                title: "FlightBooking progression rejected.");
        }
    }

    private static string? ReadAccessToken(HttpContext httpContext)
    {
        httpContext.Request.Headers.TryGetValue(
            PublicFlightBookingCompositionBoundary.AccessTokenHeader,
            out var tokenValues);
        return tokenValues.ToString();
    }

    private static PublicFlightBookingInitiationRequest MergeIdempotency(
        PublicFlightBookingInitiationRequest request,
        HttpContext httpContext)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            return request;
        }

        if (httpContext.Request.Headers.TryGetValue(
                PublicFlightBookingCompositionBoundary.IdempotencyHeader,
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
