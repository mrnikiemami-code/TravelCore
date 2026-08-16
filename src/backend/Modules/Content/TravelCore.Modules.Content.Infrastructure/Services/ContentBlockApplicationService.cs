using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Content.Contracts;
using TravelCore.Modules.Content.Domain;
using ContentItemAggregate = TravelCore.Modules.Content.Domain.ContentItem;

namespace TravelCore.Modules.Content.Infrastructure.Services;

/// <summary>
/// Relational Content Blocks application service (TC-P08-T005 / P08-R2).
/// </summary>
public sealed class ContentBlockApplicationService : IContentBlockService
{
    private readonly ContentDbContext _db;
    private readonly IClock _clock;

    public ContentBlockApplicationService(ContentDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<ContentBlockResponse> AddHeadingAsync(
        Guid contentItemId,
        AddContentHeadingBlockRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        var block = item.AddHeadingBlock(request.Text, request.Level, _clock.GetCurrentInstant(), request.SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(block);
    }

    public async Task<ContentBlockResponse> AddParagraphAsync(
        Guid contentItemId,
        AddContentParagraphBlockRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        var block = item.AddParagraphBlock(request.Text, _clock.GetCurrentInstant(), request.SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(block);
    }

    public async Task<ContentBlockResponse> AddImageAsync(
        Guid contentItemId,
        AddContentImageBlockRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        var block = item.AddImageBlock(
            request.MediaAssetId,
            _clock.GetCurrentInstant(),
            request.Caption,
            request.SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(block);
    }

    public async Task<ContentBlockResponse> AddGalleryAsync(
        Guid contentItemId,
        AddContentGalleryBlockRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        var block = item.AddGalleryBlock(request.MediaAssetIds, _clock.GetCurrentInstant(), request.SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(block);
    }

    public async Task<ContentBlockResponse> AddFaqAsync(
        Guid contentItemId,
        AddContentFaqBlockRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var items = request.Items
            .Select(x => (x.Question, x.Answer))
            .ToList();
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        var block = item.AddFaqBlock(items, _clock.GetCurrentInstant(), request.SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(block);
    }

    public async Task<ContentBlockResponse> AddTableAsync(
        Guid contentItemId,
        AddContentTableBlockRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        var block = item.AddTableBlock(request.Text, _clock.GetCurrentInstant(), request.SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(block);
    }

    public async Task<ContentBlockResponse> AddVideoAsync(
        Guid contentItemId,
        AddContentVideoBlockRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        var block = item.AddVideoBlock(
            request.MediaAssetId,
            _clock.GetCurrentInstant(),
            request.Caption,
            request.SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(block);
    }

    public async Task<ContentBlockResponse> AddCtaAsync(
        Guid contentItemId,
        AddContentCtaBlockRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        var block = item.AddCtaBlock(request.Label, request.Href, _clock.GetCurrentInstant(), request.SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(block);
    }

    public async Task<IReadOnlyList<ContentBlockResponse>> ListAsync(
        Guid contentItemId,
        CancellationToken cancellationToken = default)
    {
        var id = ContentItemId.From(contentItemId);
        var item = await _db.ContentItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return [];
        }

        return item.BlocksOrdered.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ContentBlockResponse>> ReorderAsync(
        Guid contentItemId,
        ReorderContentBlocksRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        var ordered = item.ReorderBlocks(
            request.OrderedBlockIds.Select(ContentBlockId.From).ToList(),
            _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return ordered.Select(Map).ToList();
    }

    public async Task RemoveAsync(
        Guid contentItemId,
        Guid blockId,
        CancellationToken cancellationToken = default)
    {
        var item = await LoadTrackedAsync(contentItemId, cancellationToken);
        if (!item.RemoveBlock(ContentBlockId.From(blockId), _clock.GetCurrentInstant()))
        {
            throw new KeyNotFoundException($"ContentBlock '{blockId}' was not found on ContentItem '{contentItemId}'.");
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<ContentItemAggregate> LoadTrackedAsync(
        Guid contentItemId,
        CancellationToken cancellationToken)
    {
        var id = ContentItemId.From(contentItemId);
        var item = await _db.ContentItems
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return item
            ?? throw new KeyNotFoundException($"ContentItem '{contentItemId}' was not found.");
    }

    private static ContentBlockResponse Map(ContentBlock block) =>
        new(
            block.Id.Value,
            block.ContentItemId.Value,
            block.Kind.ToString(),
            block.SortOrder,
            block.Text,
            block.HeadingLevel,
            block.MediaAssetId,
            block.Href,
            block.GalleryItems
                .OrderBy(x => x.SortOrder)
                .Select(x => new ContentBlockGalleryItemResponse(x.MediaAssetId, x.SortOrder))
                .ToList(),
            block.FaqItems
                .OrderBy(x => x.SortOrder)
                .Select(x => new ContentBlockFaqItemResponse(x.Question, x.Answer, x.SortOrder))
                .ToList());
}
