using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Persistence;

internal sealed class TourProductConfiguration : IEntityTypeConfiguration<TourProduct>
{
    public void Configure(EntityTypeBuilder<TourProduct> builder)
    {
        builder.ToTable("tour_products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => TourProductId.From(value));

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(TourProduct.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.EnglishName)
            .HasColumnName("english_name")
            .HasMaxLength(TourProduct.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.ClassificationCode)
            .HasColumnName("classification_code")
            .HasMaxLength(TourProduct.ClassificationCodeMaxLength);

        // Logical Origin DestinationId only — deliberately no FK / navigation to Destination.
        builder.Property(x => x.OriginDestinationId)
            .HasColumnName("origin_destination_id");

        // Logical Agency PartyId only — deliberately no FK / navigation to Party.
        builder.Property(x => x.AgencyId)
            .HasColumnName("agency_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("ux_tour_products_code");

        builder.HasIndex(x => x.Kind)
            .HasDatabaseName("ix_tour_products_kind");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_tour_products_created_at");

        builder.HasIndex(x => x.ClassificationCode)
            .HasDatabaseName("ix_tour_products_classification_code");

        builder.HasIndex(x => x.OriginDestinationId)
            .HasDatabaseName("ix_tour_products_origin_destination_id");

        builder.HasIndex(x => x.AgencyId)
            .HasDatabaseName("ix_tour_products_agency_id");

        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Translations)
            .HasField("_translations")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(x => x.Destinations)
            .WithOne()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Destinations)
            .HasField("_destinations")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(x => x.Services)
            .WithOne()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Services)
            .HasField("_services")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(x => x.Policies)
            .WithOne()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Policies)
            .HasField("_policies")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(x => x.Requirements)
            .WithOne()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Requirements)
            .HasField("_requirements")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.Ignore(x => x.Cover);
        builder.Ignore(x => x.GalleryOrdered);

        builder.HasMany(x => x.MediaLinks)
            .WithOne()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.MediaLinks)
            .HasField("_mediaLinks")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}

internal sealed class TourProductTranslationConfiguration : IEntityTypeConfiguration<TourProductTranslation>
{
    public void Configure(EntityTypeBuilder<TourProductTranslation> builder)
    {
        builder.ToTable("tour_product_translations");
        builder.HasKey(x => new { x.TourProductId, x.LocaleCode });

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value));

        builder.Property(x => x.LocaleCode)
            .HasColumnName("locale_code")
            .HasMaxLength(TourProductTranslation.LocaleCodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(TourProductTranslation.TitleMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(TourProductTranslation.DescriptionMaxLength);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.LocaleCode)
            .HasDatabaseName("ix_tour_product_translations_locale_code");
    }
}

internal sealed class TourProductDestinationConfiguration : IEntityTypeConfiguration<TourProductDestination>
{
    public void Configure(EntityTypeBuilder<TourProductDestination> builder)
    {
        builder.ToTable("tour_product_destinations");
        builder.HasKey(x => new { x.TourProductId, x.DestinationId });

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value));

        // Logical DestinationId only — deliberately no FK / navigation to Destination.
        builder.Property(x => x.DestinationId)
            .HasColumnName("destination_id")
            .IsRequired();

        builder.HasIndex(x => x.DestinationId)
            .HasDatabaseName("ix_tour_product_destinations_destination_id");
    }
}

internal sealed class TourProductServiceConfiguration : IEntityTypeConfiguration<TourProductService>
{
    public void Configure(EntityTypeBuilder<TourProductService> builder)
    {
        builder.ToTable("tour_product_services");
        builder.HasKey(x => new { x.TourProductId, x.Code });

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value));

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(TourCatalogFactCode.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Detail)
            .HasColumnName("detail")
            .HasMaxLength(TourCatalogFactCode.DetailMaxLength);
    }
}

internal sealed class TourProductPolicyConfiguration : IEntityTypeConfiguration<TourProductPolicy>
{
    public void Configure(EntityTypeBuilder<TourProductPolicy> builder)
    {
        builder.ToTable("tour_product_policies");
        builder.HasKey(x => new { x.TourProductId, x.Code });

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value));

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(TourCatalogFactCode.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Detail)
            .HasColumnName("detail")
            .HasMaxLength(TourCatalogFactCode.DetailMaxLength);
    }
}

internal sealed class TourProductRequirementConfiguration : IEntityTypeConfiguration<TourProductRequirement>
{
    public void Configure(EntityTypeBuilder<TourProductRequirement> builder)
    {
        builder.ToTable("tour_product_requirements");
        builder.HasKey(x => new { x.TourProductId, x.Code });

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value));

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(TourCatalogFactCode.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Detail)
            .HasColumnName("detail")
            .HasMaxLength(TourCatalogFactCode.DetailMaxLength);
    }
}

internal sealed class TourProductMediaLinkConfiguration : IEntityTypeConfiguration<TourProductMediaLink>
{
    public void Configure(EntityTypeBuilder<TourProductMediaLink> builder)
    {
        builder.ToTable("tour_product_media_links");
        builder.HasKey(x => new { x.TourProductId, x.MediaAssetId });

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value));

        // Logical MediaAssetId only — deliberately no FK / navigation to Media.
        builder.Property(x => x.MediaAssetId)
            .HasColumnName("media_asset_id")
            .IsRequired();

        builder.Property(x => x.Role)
            .HasColumnName("role")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.HasIndex(x => x.MediaAssetId)
            .HasDatabaseName("ix_tour_product_media_links_media_asset_id");

        builder.HasIndex(x => x.TourProductId)
            .IsUnique()
            .HasFilter("role = 0")
            .HasDatabaseName("ux_tour_product_media_links_cover");

        builder.HasIndex(x => new { x.TourProductId, x.SortOrder })
            .IsUnique()
            .HasFilter("role = 1")
            .HasDatabaseName("ux_tour_product_media_links_gallery_sort");
    }
}
