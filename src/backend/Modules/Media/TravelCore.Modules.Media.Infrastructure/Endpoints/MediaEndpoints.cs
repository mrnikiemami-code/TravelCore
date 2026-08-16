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

        return endpoints;
    }
}
