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

        builder.Navigation(x => x.Article).AutoInclude();
        builder.Navigation(x => x.LandingPage).AutoInclude();
        builder.Navigation(x => x.Guide).AutoInclude();
        builder.Navigation(x => x.Translations)
            .HasField("_translations")
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
