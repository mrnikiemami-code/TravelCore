using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContentItemAggregate = TravelCore.Modules.Content.Domain.ContentItem;
using TravelCore.Modules.Content.Domain;

namespace TravelCore.Modules.Content.Infrastructure.Persistence;

internal sealed class ContentItemConfiguration : IEntityTypeConfiguration<ContentItemAggregate>
{
    public void Configure(EntityTypeBuilder<ContentItemAggregate> builder)
    {
        builder.ToTable("content_items");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ContentItemId.From(value));

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(ContentItemAggregate.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.EnglishName)
            .HasColumnName("english_name")
            .HasMaxLength(ContentItemAggregate.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("ux_content_items_code");

        builder.HasIndex(x => x.Kind)
            .HasDatabaseName("ix_content_items_kind");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_content_items_created_at");

        // Same-schema 1:1 specializations — never a cross-schema FK.
        builder.HasOne(x => x.Article)
            .WithOne()
            .HasForeignKey<Article>(x => x.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LandingPage)
            .WithOne()
            .HasForeignKey<LandingPage>(x => x.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Guide)
            .WithOne()
            .HasForeignKey<Guide>(x => x.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Categories)
            .WithOne()
            .HasForeignKey(x => x.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tags)
            .WithOne()
            .HasForeignKey(x => x.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Destinations)
            .WithOne()
            .HasForeignKey(x => x.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // ContentBlock relationship is owned on ContentBlockConfiguration (PlaceMediaLink pattern).

        builder.Navigation(x => x.Article).AutoInclude();
        builder.Navigation(x => x.LandingPage).AutoInclude();
        builder.Navigation(x => x.Guide).AutoInclude();
        builder.Navigation(x => x.Translations)
            .HasField("_translations")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
        builder.Navigation(x => x.Categories)
            .HasField("_categories")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
        builder.Navigation(x => x.Tags)
            .HasField("_tags")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
        builder.Navigation(x => x.Blocks)
            .HasField("_blocks")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
        builder.Navigation(x => x.Destinations)
            .HasField("_destinations")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}

internal sealed class ContentItemTranslationConfiguration : IEntityTypeConfiguration<ContentItemTranslation>
{
    public void Configure(EntityTypeBuilder<ContentItemTranslation> builder)
    {
        builder.ToTable("content_item_translations");
        builder.HasKey(x => new { x.ContentItemId, x.LocaleCode });

        builder.Property(x => x.ContentItemId)
            .HasColumnName("content_item_id")
            .HasConversion(id => id.Value, value => ContentItemId.From(value));

        builder.Property(x => x.LocaleCode)
            .HasColumnName("locale_code")
            .HasMaxLength(ContentItemTranslation.LocaleCodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(ContentItemTranslation.TitleMaxLength)
            .IsRequired();

        builder.Property(x => x.Body)
            .HasColumnName("body")
            .HasMaxLength(ContentItemTranslation.BodyMaxLength);

        builder.Property(x => x.Excerpt)
            .HasColumnName("excerpt")
            .HasMaxLength(ContentItemTranslation.ExcerptMaxLength);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.LocaleCode)
            .HasDatabaseName("ix_content_item_translations_locale_code");
    }
}

internal sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("articles");
        builder.HasKey(x => x.ContentItemId);

        builder.Property(x => x.ContentItemId)
            .HasColumnName("content_item_id")
            .HasConversion(id => id.Value, value => ContentItemId.From(value));
    }
}

internal sealed class LandingPageConfiguration : IEntityTypeConfiguration<LandingPage>
{
    public void Configure(EntityTypeBuilder<LandingPage> builder)
    {
        builder.ToTable("landing_pages");
        builder.HasKey(x => x.ContentItemId);

        builder.Property(x => x.ContentItemId)
            .HasColumnName("content_item_id")
            .HasConversion(id => id.Value, value => ContentItemId.From(value));
    }
}

internal sealed class GuideConfiguration : IEntityTypeConfiguration<Guide>
{
    public void Configure(EntityTypeBuilder<Guide> builder)
    {
        builder.ToTable("guides");
        builder.HasKey(x => x.ContentItemId);

        builder.Property(x => x.ContentItemId)
            .HasColumnName("content_item_id")
            .HasConversion(id => id.Value, value => ContentItemId.From(value));
    }
}

internal sealed class ContentCategoryConfiguration : IEntityTypeConfiguration<ContentCategory>
{
    public void Configure(EntityTypeBuilder<ContentCategory> builder)
    {
        builder.ToTable("content_categories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ContentCategoryId.From(value));

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(ContentCategory.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.EnglishName)
            .HasColumnName("english_name")
            .HasMaxLength(ContentCategory.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("ux_content_categories_code");
    }
}

internal sealed class ContentTagConfiguration : IEntityTypeConfiguration<ContentTag>
{
    public void Configure(EntityTypeBuilder<ContentTag> builder)
    {
        builder.ToTable("content_tags");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ContentTagId.From(value));

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(ContentTag.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.EnglishName)
            .HasColumnName("english_name")
            .HasMaxLength(ContentTag.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("ux_content_tags_code");
    }
}

internal sealed class ContentItemCategoryConfiguration : IEntityTypeConfiguration<ContentItemCategory>
{
    public void Configure(EntityTypeBuilder<ContentItemCategory> builder)
    {
        builder.ToTable("content_item_categories");
        builder.HasKey(x => new { x.ContentItemId, x.CategoryId });

        builder.Property(x => x.ContentItemId)
            .HasColumnName("content_item_id")
            .HasConversion(id => id.Value, value => ContentItemId.From(value));

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id")
            .HasConversion(id => id.Value, value => ContentCategoryId.From(value));

        builder.HasOne<ContentCategory>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CategoryId)
            .HasDatabaseName("ix_content_item_categories_category_id");
    }
}

internal sealed class ContentItemTagConfiguration : IEntityTypeConfiguration<ContentItemTag>
{
    public void Configure(EntityTypeBuilder<ContentItemTag> builder)
    {
        builder.ToTable("content_item_tags");
        builder.HasKey(x => new { x.ContentItemId, x.TagId });

        builder.Property(x => x.ContentItemId)
            .HasColumnName("content_item_id")
            .HasConversion(id => id.Value, value => ContentItemId.From(value));

        builder.Property(x => x.TagId)
            .HasColumnName("tag_id")
            .HasConversion(id => id.Value, value => ContentTagId.From(value));

        builder.HasOne<ContentTag>()
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TagId)
            .HasDatabaseName("ix_content_item_tags_tag_id");
    }
}

internal sealed class ContentBlockConfiguration : IEntityTypeConfiguration<ContentBlock>
{
    public void Configure(EntityTypeBuilder<ContentBlock> builder)
    {
        builder.ToTable("content_blocks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ContentBlockId.From(value));

        builder.Property(x => x.ContentItemId)
            .HasColumnName("content_item_id")
            .HasConversion(id => id.Value, value => ContentItemId.From(value))
            .IsRequired();

        builder.HasOne<ContentItemAggregate>()
            .WithMany(x => x.Blocks)
            .HasForeignKey(x => x.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.Property(x => x.Text)
            .HasColumnName("text")
            .HasMaxLength(ContentBlock.TextMaxLength);

        builder.Property(x => x.HeadingLevel)
            .HasColumnName("heading_level");

        // Logical MediaAssetId only — deliberately no FK / navigation to Media.
        builder.Property(x => x.MediaAssetId)
            .HasColumnName("media_asset_id");

        builder.Property(x => x.Href)
            .HasColumnName("href")
            .HasMaxLength(ContentBlock.HrefMaxLength);

        builder.HasMany(x => x.GalleryItems)
            .WithOne()
            .HasForeignKey(x => x.BlockId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.FaqItems)
            .WithOne()
            .HasForeignKey(x => x.BlockId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.GalleryItems)
            .HasField("_galleryItems")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
        builder.Navigation(x => x.FaqItems)
            .HasField("_faqItems")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasIndex(x => new { x.ContentItemId, x.SortOrder })
            .HasDatabaseName("ix_content_blocks_item_sort");

        builder.HasIndex(x => x.Kind)
            .HasDatabaseName("ix_content_blocks_kind");

        builder.HasIndex(x => x.MediaAssetId)
            .HasDatabaseName("ix_content_blocks_media_asset_id");
    }
}

internal sealed class ContentBlockGalleryItemConfiguration : IEntityTypeConfiguration<ContentBlockGalleryItem>
{
    public void Configure(EntityTypeBuilder<ContentBlockGalleryItem> builder)
    {
        builder.ToTable("content_block_gallery_items");
        builder.HasKey(x => new { x.BlockId, x.MediaAssetId });

        builder.Property(x => x.BlockId)
            .HasColumnName("block_id")
            .HasConversion(id => id.Value, value => ContentBlockId.From(value));

        builder.Property(x => x.MediaAssetId)
            .HasColumnName("media_asset_id")
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.HasIndex(x => new { x.BlockId, x.SortOrder })
            .IsUnique()
            .HasDatabaseName("ux_content_block_gallery_items_sort");
    }
}

internal sealed class ContentBlockFaqItemConfiguration : IEntityTypeConfiguration<ContentBlockFaqItem>
{
    public void Configure(EntityTypeBuilder<ContentBlockFaqItem> builder)
    {
        builder.ToTable("content_block_faq_items");
        builder.HasKey(x => new { x.BlockId, x.SortOrder });

        builder.Property(x => x.BlockId)
            .HasColumnName("block_id")
            .HasConversion(id => id.Value, value => ContentBlockId.From(value));

        builder.Property(x => x.Question)
            .HasColumnName("question")
            .HasMaxLength(ContentBlockFaqItem.QuestionMaxLength)
            .IsRequired();

        builder.Property(x => x.Answer)
            .HasColumnName("answer")
            .HasMaxLength(ContentBlockFaqItem.AnswerMaxLength)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();
    }
}

internal sealed class ContentItemDestinationConfiguration : IEntityTypeConfiguration<ContentItemDestination>
{
    public void Configure(EntityTypeBuilder<ContentItemDestination> builder)
    {
        builder.ToTable("content_item_destinations");
        builder.HasKey(x => new { x.ContentItemId, x.DestinationId });

        builder.Property(x => x.ContentItemId)
            .HasColumnName("content_item_id")
            .HasConversion(id => id.Value, value => ContentItemId.From(value));

        // Logical DestinationId only — deliberately no FK / navigation to Destination.
        builder.Property(x => x.DestinationId)
            .HasColumnName("destination_id")
            .IsRequired();

        builder.HasIndex(x => x.DestinationId)
            .HasDatabaseName("ix_content_item_destinations_destination_id");
    }
}
