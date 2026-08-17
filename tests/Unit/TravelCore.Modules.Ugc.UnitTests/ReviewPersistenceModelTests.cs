using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Ugc.Domain;
using TravelCore.Modules.Ugc.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// Persistence model shape for Review + child dimension ratings (TC-P16-T002). Schema ugc only; no peer FK.
/// </summary>
public sealed class ReviewPersistenceModelTests
{
    [Fact]
    public void UgcModel_Maps_Review_With_Child_DimensionRatings_No_Peer_Fk_Or_Hardcoded_Columns()
    {
        using var db = new UgcDbContext(
            new DbContextOptionsBuilder<UgcDbContext>()
                .UseTravelCorePostgreSql(
                    "Host=127.0.0.1;Database=travelcore_ugc_review_model_probe;Username=x;Password=x",
                    migrationsHistorySchema: UgcDbContext.SchemaName)
                .Options);

        var model = db.Model;
        var reviewType = model.FindEntityType(typeof(Review));
        Assert.NotNull(reviewType);
        Assert.Equal("reviews", reviewType.GetTableName());
        Assert.Equal(UgcDbContext.SchemaName, reviewType.GetSchema());
        Assert.Equal("overall_rating", reviewType.FindProperty(nameof(Review.OverallRating))!.GetColumnName());
        Assert.Equal("target_type", reviewType.FindProperty(nameof(Review.TargetType))!.GetColumnName());
        Assert.Equal("target_id", reviewType.FindProperty(nameof(Review.TargetId))!.GetColumnName());
        Assert.Contains(
            reviewType.GetIndexes(),
            i => i.GetDatabaseName() == "ix_reviews_target_type_target_id");

        var dimensionType = model.FindEntityType(typeof(ReviewDimensionRating));
        Assert.NotNull(dimensionType);
        Assert.Equal("review_dimension_ratings", dimensionType.GetTableName());
        Assert.Equal(UgcDbContext.SchemaName, dimensionType.GetSchema());
        Assert.Equal("dimension_code", dimensionType.FindProperty(nameof(ReviewDimensionRating.DimensionCode))!.GetColumnName());
        Assert.Equal("value", dimensionType.FindProperty(nameof(ReviewDimensionRating.Value))!.GetColumnName());

        var fk = dimensionType.GetForeignKeys().Single();
        Assert.Equal(typeof(Review), fk.PrincipalEntityType.ClrType);
        Assert.Equal(UgcDbContext.SchemaName, fk.PrincipalEntityType.GetSchema());
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);

        Assert.DoesNotContain(
            model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()),
            f =>
            {
                var schema = f.PrincipalEntityType.GetSchema();
                return schema is "identity" or "party" or "tour" or "place" or "destination"
                    or "content" or "media" or "seo" or "search" or "agency_marketplace";
            });

        var columns = model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Select(p => p.GetColumnName())
            .Where(n => n is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("hotel_rating", columns);
        Assert.DoesNotContain("guide_rating", columns);
        Assert.DoesNotContain("food_rating", columns);
        Assert.DoesNotContain("service_rating", columns);
        Assert.Contains("target_id", columns);
        Assert.Contains("target_type", columns);
        Assert.DoesNotContain(
            model.GetEntityTypes().SelectMany(e => e.GetNavigations()),
            n => n.TargetEntityType.Name.Contains(".Tour.", StringComparison.Ordinal)
                 || n.TargetEntityType.Name.Contains(".Place.", StringComparison.Ordinal)
                 || n.TargetEntityType.Name.Contains(".AgencyMarketplace.", StringComparison.Ordinal));
        var travelogueType = model.FindEntityType(typeof(Travelogue));
        Assert.NotNull(travelogueType);
        Assert.Equal("travelogues", travelogueType.GetTableName());
        Assert.Equal(UgcDbContext.SchemaName, travelogueType.GetSchema());
        Assert.Equal("locale_code", travelogueType.FindProperty(nameof(Travelogue.LocaleCode))!.GetColumnName());
        Assert.Null(travelogueType.FindProperty("ContentItemId"));
        Assert.Null(travelogueType.FindProperty("PublicationStatus"));
        Assert.Null(travelogueType.FindProperty("IsUserGenerated"));
        Assert.Contains(
            travelogueType.GetIndexes(),
            i => i.GetDatabaseName() == "ix_travelogues_actor_id");
        Assert.Contains(
            travelogueType.GetIndexes(),
            i => i.GetDatabaseName() == "ix_travelogues_locale_code");

        var userPhotoType = model.FindEntityType(typeof(UserPhoto));
        Assert.NotNull(userPhotoType);
        Assert.Equal("user_photos", userPhotoType.GetTableName());
        Assert.Equal(UgcDbContext.SchemaName, userPhotoType.GetSchema());
        Assert.Equal("media_asset_id", userPhotoType.FindProperty(nameof(UserPhoto.MediaAssetId))!.GetColumnName());
        Assert.Null(userPhotoType.FindProperty("StorageKey"));
        Assert.Null(userPhotoType.FindProperty("MimeType"));
        Assert.Null(userPhotoType.FindProperty("FileSize"));
        Assert.Null(userPhotoType.FindProperty("Width"));
        Assert.Null(userPhotoType.FindProperty("Height"));
        Assert.Null(userPhotoType.FindProperty("FocalPoint"));
        Assert.Contains(
            userPhotoType.GetIndexes(),
            i => i.GetDatabaseName() == "ux_user_photos_media_asset_id" && i.IsUnique);

        var commentType = model.FindEntityType(typeof(Comment));
        Assert.NotNull(commentType);
        Assert.Equal("comments", commentType.GetTableName());
        Assert.Equal(UgcDbContext.SchemaName, commentType.GetSchema());
        Assert.Equal("target_type", commentType.FindProperty(nameof(Comment.TargetType))!.GetColumnName());
        Assert.Equal("target_id", commentType.FindProperty(nameof(Comment.TargetId))!.GetColumnName());
        Assert.Null(commentType.FindProperty("ParentCommentId"));
        Assert.Null(commentType.FindProperty("LikeCount"));
        Assert.Null(commentType.FindProperty("PublicationStatus"));
        Assert.Contains(
            commentType.GetIndexes(),
            i => i.GetDatabaseName() == "ix_comments_target_type_target_id");
        Assert.Null(model.GetEntityTypes().FirstOrDefault(e =>
            string.Equals(e.GetTableName(), "likes", StringComparison.OrdinalIgnoreCase)));

        Assert.False(db.Database.HasPendingModelChanges());
    }
}
