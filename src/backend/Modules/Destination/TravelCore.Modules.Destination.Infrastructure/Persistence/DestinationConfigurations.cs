using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DestinationAggregate = TravelCore.Modules.Destination.Domain.Destination;
using TravelCore.Modules.Destination.Domain;

namespace TravelCore.Modules.Destination.Infrastructure.Persistence;

internal sealed class DestinationConfiguration : IEntityTypeConfiguration<DestinationAggregate>
{
    public void Configure(EntityTypeBuilder<DestinationAggregate> builder)
    {
        builder.ToTable("destinations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => DestinationId.From(value));

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(DestinationAggregate.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.EnglishName)
            .HasColumnName("english_name")
            .HasMaxLength(DestinationAggregate.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.ParentId)
            .HasColumnName("parent_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? DestinationId.From(value.Value) : null);

        builder.Property(x => x.IsoCountryCode)
            .HasColumnName("iso_country_code")
            .HasMaxLength(2);

        builder.Property(x => x.Latitude)
            .HasColumnName("latitude")
            .HasPrecision(9, 6);

        builder.Property(x => x.Longitude)
            .HasColumnName("longitude")
            .HasPrecision(9, 6);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("ux_destinations_code");

        builder.HasIndex(x => x.ParentId)
            .HasDatabaseName("ix_destinations_parent_id");

        builder.HasIndex(x => x.Kind)
            .HasDatabaseName("ix_destinations_kind");

        builder.HasIndex(x => x.IsoCountryCode)
            .HasDatabaseName("ix_destinations_iso_country_code");

        // Same-schema parent/child only — never a cross-schema FK.
        builder.HasOne<DestinationAggregate>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.DestinationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Translations)
            .HasField("_translations")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}

internal sealed class DestinationTranslationConfiguration : IEntityTypeConfiguration<DestinationTranslation>
{
    public void Configure(EntityTypeBuilder<DestinationTranslation> builder)
    {
        builder.ToTable("destination_translations");
        builder.HasKey(x => new { x.DestinationId, x.LocaleCode });

        builder.Property(x => x.DestinationId)
            .HasColumnName("destination_id")
            .HasConversion(id => id.Value, value => DestinationId.From(value));

        builder.Property(x => x.LocaleCode)
            .HasColumnName("locale_code")
            .HasMaxLength(DestinationTranslation.LocaleCodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(DestinationTranslation.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(DestinationTranslation.DescriptionMaxLength);

        builder.Property(x => x.Slug)
            .HasColumnName("slug")
            .HasMaxLength(DestinationTranslation.SlugMaxLength);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.LocaleCode)
            .HasDatabaseName("ix_destination_translations_locale_code");

        builder.HasIndex(x => new { x.LocaleCode, x.Slug })
            .IsUnique()
            .HasFilter("slug IS NOT NULL")
            .HasDatabaseName("ux_destination_translations_locale_slug");
    }
}
