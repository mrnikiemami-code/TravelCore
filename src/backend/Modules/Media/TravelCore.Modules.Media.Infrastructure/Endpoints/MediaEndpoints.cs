using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Media.Contracts;

namespace TravelCore.Modules.Media.Infrastructure.Endpoints;

internal static class MediaEndpoints
{
    private const string MediaAssetsWritePolicy = "Access.Media.Assets.Write";

    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/media/assets")
            .WithTags("Media");

        // Minimal Access-protected list (take/status only — no query DSL).
        group.MapGet("/", async Task<IResult> (
            string? status,
            int? take,
            IMediaAssetService assets,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var list = await assets.ListAsync(status, take ?? 50, cancellationToken);
                return Results.Ok(list);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "take"] = [ex.Message]
                });
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "status"] = [ex.Message]
                });
            }
        }).RequireAuthorization(MediaAssetsWritePolicy);

        group.MapPost("/upload", async Task<IResult> (
            HttpRequest request,
            IMediaUploadService uploadService,
            CancellationToken cancellationToken) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["contentType"] = ["multipart/form-data is required for Media upload."]
                });
            }

            IFormFile? file;
            try
            {
                var form = await request.ReadFormAsync(cancellationToken);
                file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
            }
            catch (Exception ex) when (ex is InvalidDataException or BadHttpRequestException)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["file"] = [ex.Message]
                });
            }

            if (file is null || file.Length <= 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["file"] = ["A non-empty file form field is required."]
                });
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var created = await uploadService.UploadAsync(
                    stream,
                    file.ContentType,
                    file.FileName,
                    file.Length,
                    cancellationToken);
                return Results.Created($"/api/media/assets/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "file"] = [ex.Message]
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "Media upload failed");
            }
        }).DisableAntiforgery()
            .RequireAuthorization(MediaAssetsWritePolicy);

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            IMediaAssetService assets,
            CancellationToken cancellationToken) =>
        {
            var asset = await assets.GetByIdAsync(id, cancellationToken);
            return asset is null ? Results.NotFound() : Results.Ok(asset);
        }).RequireAuthorization(MediaAssetsWritePolicy);

        group.MapPost("/{id:guid}/variants/generate", async Task<IResult> (
            Guid id,
            IMediaVariantProcessingService variants,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await variants.GenerateForAssetAsync(id, cancellationToken);
                return Results.Created($"/api/media/assets/{id:D}/variants", created);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "mediaAssetId"] = [ex.Message]
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "Media variant generation failed");
            }
        }).RequireAuthorization(MediaAssetsWritePolicy);

        group.MapGet("/{id:guid}/variants", async Task<IResult> (
            Guid id,
            IMediaVariantProcessingService variants,
            CancellationToken cancellationToken) =>
        {
            var list = await variants.ListForAssetAsync(id, cancellationToken);
            return Results.Ok(list);
        }).RequireAuthorization(MediaAssetsWritePolicy);

        group.MapPut("/{id:guid}/focal-point", async Task<IResult> (
            Guid id,
            UpsertFocalPointRequest request,
            IMediaFocalPointService focalPoints,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await focalPoints.SetAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "focalPoint"] = [ex.Message]
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Media asset not found");
            }
        }).RequireAuthorization(MediaAssetsWritePolicy);

        group.MapGet("/{id:guid}/focal-point", async Task<IResult> (
            Guid id,
            IMediaFocalPointService focalPoints,
            CancellationToken cancellationToken) =>
        {
            var focal = await focalPoints.GetAsync(id, cancellationToken);
            return focal is null ? Results.NotFound() : Results.Ok(focal);
        }).RequireAuthorization(MediaAssetsWritePolicy);

        group.MapPut("/{id:guid}/translations/{localeCode}", async Task<IResult> (
            Guid id,
            string localeCode,
            UpsertMediaAssetTranslationRequest request,
            IMediaAssetTranslationService translations,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var upserted = await translations.UpsertAsync(id, localeCode, request, cancellationToken);
                return Results.Ok(upserted);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "translation"] = [ex.Message]
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Media asset not found");
            }
        }).RequireAuthorization(MediaAssetsWritePolicy);

        group.MapGet("/{id:guid}/translations", async Task<IResult> (
            Guid id,
            IMediaAssetTranslationService translations,
            CancellationToken cancellationToken) =>
        {
            var list = await translations.ListAsync(id, cancellationToken);
            return Results.Ok(list);
        }).RequireAuthorization(MediaAssetsWritePolicy);

        group.MapGet("/{id:guid}/translations/{localeCode}", async Task<IResult> (
            Guid id,
            string localeCode,
            IMediaAssetTranslationService translations,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var row = await translations.GetAsync(id, localeCode, cancellationToken);
                return row is null ? Results.NotFound() : Results.Ok(row);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "localeCode"] = [ex.Message]
                });
            }
        }).RequireAuthorization(MediaAssetsWritePolicy);

        group.MapGet("/{id:guid}/translations/{localeCode}/presentation", async Task<IResult> (
            Guid id,
            string localeCode,
            IMediaAssetTranslationService translations,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var published = await translations.GetPublishedForPresentationAsync(
                    id,
                    localeCode,
                    cancellationToken);
                return published is null ? Results.NotFound() : Results.Ok(published);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "localeCode"] = [ex.Message]
                });
            }
        }).RequireAuthorization(MediaAssetsWritePolicy);

        // --- Anonymous public app-proxy delivery (P06-R4) ---
        // Mutation endpoints above remain Access-protected. These GETs intentionally omit
        // RequireAuthorization (anonymous public read). StorageKey is never accepted from callers.

        group.MapGet("/{id:guid}/content", async Task<IResult> (
            Guid id,
            IMediaContentDeliveryService delivery,
            CancellationToken cancellationToken) =>
        {
            var opened = await delivery.OpenOriginalAsync(id, cancellationToken);
            return opened is null ? Results.NotFound() : ToStreamResult(opened);
        }).AllowAnonymous();

        group.MapGet("/{id:guid}/variants/{profile}/content", async Task<IResult> (
            Guid id,
            string profile,
            IMediaContentDeliveryService delivery,
            CancellationToken cancellationToken) =>
        {
            var opened = await delivery.OpenVariantAsync(id, profile, cancellationToken);
            return opened is null ? Results.NotFound() : ToStreamResult(opened);
        }).AllowAnonymous();

        group.MapGet("/{id:guid}/presentation", async Task<IResult> (
            Guid id,
            string? locale,
            IMediaPresentationService presentation,
            CancellationToken cancellationToken) =>
        {
            var dto = await presentation.GetPresentationAsync(id, locale, cancellationToken);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        }).AllowAnonymous();

        return endpoints;
    }

    private static IResult ToStreamResult(MediaContentDeliveryResult opened)
    {
        // Conservative cache: do not invent long-lived immutable Cache-Control.
        // Content-Type comes from trusted Media metadata (not caller / filename).
        return Results.Stream(
            opened.Content,
            contentType: opened.ContentType,
            fileDownloadName: null,
            lastModified: null,
            entityTag: null,
            enableRangeProcessing: false);
    }
}
