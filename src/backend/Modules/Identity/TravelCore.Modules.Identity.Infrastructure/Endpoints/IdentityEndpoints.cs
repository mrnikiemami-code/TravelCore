using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NodaTime;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Identity.Infrastructure.Security;
using TravelCore.Modules.Identity.Infrastructure.Services;

namespace TravelCore.Modules.Identity.Infrastructure.Endpoints;

internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var accounts = endpoints.MapGroup("/api/identity/accounts")
            .WithTags("Identity");

        accounts.MapPost("/", async Task<IResult> (
            CreateAccountRequest request,
            IdentityApplicationService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/identity/accounts/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = "Conflict", detail = ex.Message });
            }
        });

        accounts.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            IdentityApplicationService service,
            CancellationToken cancellationToken) =>
        {
            var account = await service.GetStatusByIdAsync(id, cancellationToken);
            return account is null ? Results.NotFound() : Results.Ok(account);
        });

        accounts.MapPost("/{id:guid}/party-association", async Task<IResult> (
            Guid id,
            SetAccountPartyAssociationRequest request,
            IdentityApplicationService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.LinkPartyAsync(id, request.PartyId, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = "Conflict", detail = ex.Message });
            }
        });

        accounts.MapPut("/{id:guid}/party-association", async Task<IResult> (
            Guid id,
            SetAccountPartyAssociationRequest request,
            IdentityApplicationService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.ReplacePartyAsync(id, request.PartyId, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = "Conflict", detail = ex.Message });
            }
        });

        accounts.MapDelete("/{id:guid}/party-association", async Task<IResult> (
            Guid id,
            IdentityApplicationService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.UnlinkPartyAsync(id, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        var auth = endpoints.MapGroup("/api/identity")
            .WithTags("Identity");

        auth.MapPost("/login", async Task<IResult> (
            LoginRequest request,
            IdentityApplicationService service,
            HttpContext httpContext,
            IClock clock,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var principal = await service.AuthenticateAsync(request.Email, request.Password, cancellationToken);
                if (principal is null)
                {
                    // Uniform failure — do not disclose account existence.
                    return Results.Unauthorized();
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, principal.AccountId.ToString("D")),
                    new(IdentityCookieAuthenticationDefaults.AccountIdClaimType, principal.AccountId.ToString("D")),
                    new(ClaimTypes.Email, principal.Email)
                };

                var identity = new ClaimsIdentity(claims, IdentityCookieAuthenticationDefaults.AuthenticationScheme);
                await httpContext.SignInAsync(
                    IdentityCookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));

                return Results.Ok(new AuthenticatedPrincipalResponse
                {
                    AccountId = principal.AccountId,
                    Email = principal.Email,
                    Status = principal.Status,
                    AssociatedPartyId = principal.AssociatedPartyId,
                    AuthenticatedAt = clock.GetCurrentInstant()
                });
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
        });

        auth.MapPost("/logout", async Task<IResult> (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(IdentityCookieAuthenticationDefaults.AuthenticationScheme);
            return Results.NoContent();
        });

        auth.MapGet("/me", async Task<IResult> (
            HttpContext httpContext,
            IdentityApplicationService service,
            IClock clock,
            CancellationToken cancellationToken) =>
        {
            if (httpContext.User.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var idValue = httpContext.User.FindFirstValue(IdentityCookieAuthenticationDefaults.AccountIdClaimType)
                ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(idValue, out var accountId))
            {
                return Results.Unauthorized();
            }

            var status = await service.GetStatusByIdAsync(accountId, cancellationToken);
            if (status is null || !string.Equals(status.Status, "Active", StringComparison.Ordinal))
            {
                await httpContext.SignOutAsync(IdentityCookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Unauthorized();
            }

            return Results.Ok(new AuthenticatedPrincipalResponse
            {
                AccountId = status.Id,
                Email = status.Email,
                Status = status.Status,
                AssociatedPartyId = status.AssociatedPartyId,
                AuthenticatedAt = clock.GetCurrentInstant()
            });
        }).RequireAuthorization();

        return endpoints;
    }
}
