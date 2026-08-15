using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Infrastructure.Authorization;
using TravelCore.Modules.Access.Infrastructure.Services;

namespace TravelCore.Modules.Access.Infrastructure.Endpoints;

internal static class AccessEndpoints
{
    public static IEndpointRouteBuilder MapAccessEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var permissions = endpoints.MapGroup("/api/access/permissions").WithTags("Access");
        permissions.MapPost("/", async Task<IResult> (CreatePermissionRequest request, AccessTaxonomyService svc, CancellationToken ct) =>
        {
            try
            {
                var created = await svc.CreatePermissionAsync(request, ct);
                return Results.Created($"/api/access/permissions/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { [ex.ParamName ?? "request"] = [ex.Message] });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = "Conflict", detail = ex.Message });
            }
        });
        permissions.MapGet("/", async Task<IResult> (AccessTaxonomyService svc, CancellationToken ct)
            => Results.Ok(await svc.ListPermissionsAsync(ct)));

        var roles = endpoints.MapGroup("/api/access/roles").WithTags("Access");
        roles.MapPost("/", async Task<IResult> (CreateRoleRequest request, AccessTaxonomyService svc, CancellationToken ct) =>
        {
            try
            {
                var created = await svc.CreateRoleAsync(request, ct);
                return Results.Created($"/api/access/roles/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { [ex.ParamName ?? "request"] = [ex.Message] });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = "Conflict", detail = ex.Message });
            }
        });
        roles.MapGet("/", async Task<IResult> (AccessTaxonomyService svc, CancellationToken ct)
            => Results.Ok(await svc.ListRolesAsync(ct)));
        roles.MapGet("/{id:guid}", async Task<IResult> (Guid id, AccessTaxonomyService svc, CancellationToken ct) =>
        {
            var role = await svc.GetRoleAsync(id, ct);
            return role is null ? Results.NotFound() : Results.Ok(role);
        });
        roles.MapPost("/{id:guid}/permissions", async Task<IResult> (
            Guid id, GrantRolePermissionRequest request, AccessTaxonomyService svc, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await svc.GrantPermissionAsync(id, request.PermissionId, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = "Conflict", detail = ex.Message });
            }
        });
        roles.MapDelete("/{id:guid}/permissions/{permissionId:guid}", async Task<IResult> (
            Guid id, Guid permissionId, AccessTaxonomyService svc, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await svc.RevokePermissionAsync(id, permissionId, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        endpoints.MapPost("/api/access/evaluate", async Task<IResult> (
            EvaluateAccessRequest request,
            IAccessAuthorizationEvaluator evaluator,
            CancellationToken ct) =>
        {
            try
            {
                var decision = await evaluator.EvaluateAsync(request, ct);
                return Results.Ok(decision);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
        }).WithTags("Access");

        var assignments = endpoints.MapGroup("/api/access/subject-roles").WithTags("Access");
        assignments.MapPost("/", async Task<IResult> (
            AssignSubjectRoleRequest request,
            AccessSubjectAssignmentService svc,
            CancellationToken ct) =>
        {
            try
            {
                var created = await svc.AssignAsync(request, ct);
                return Results.Ok(created);
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
        assignments.MapGet("/", async Task<IResult> (
            string subjectType,
            Guid subjectId,
            AccessSubjectAssignmentService svc,
            CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await svc.ListAsync(subjectType, subjectId, ct));
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
        });
        assignments.MapDelete("/", async Task<IResult> (
            string subjectType,
            Guid subjectId,
            Guid roleId,
            AccessSubjectAssignmentService svc,
            CancellationToken ct) =>
        {
            try
            {
                await svc.RevokeAsync(subjectType, subjectId, roleId, ct);
                return Results.NoContent();
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
        });

        // Admin sample surface — authentication + Access-backed authorization (hide ≠ authz).
        var adminAccess = endpoints.MapGroup("/api/admin/access").WithTags("AdminAccess");
        adminAccess.MapGet("/roles", async Task<IResult> (AccessTaxonomyService svc, CancellationToken ct)
                => Results.Ok(await svc.ListRolesAsync(ct)))
            .RequireAuthorization(AccessAuthorizationPolicies.AdminRolesRead);

        return endpoints;
    }
}
