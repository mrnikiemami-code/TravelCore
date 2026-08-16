using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Persistence;

internal sealed class SeoPathReservationConfiguration : IEntityTypeConfiguration<SeoPathReservation>
{
    public void Configure(EntityTypeBuilder<SeoPathReservation> builder)
    {
        builder.ToTable("seo_path_reservations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => SeoPathReservationId.From(value));

        builder.Property(x => x.ResourceType)
            .HasColumnName("resource_type")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.ResourceId)
            .HasColumnName("resource_id")
            .IsRequired();

        builder.Property(x => x.Locale)
            .HasColumnName("locale")
            .HasMaxLength(SeoRoute.LocaleMaxLength)
            .IsRequired();

        // Reserved public path string — coordinates with SeoRoute namespace, not Destination content tables.
        builder.Property(x => x.Path)
            .HasColumnName("path")
            .HasMaxLength(SeoRoute.PathMaxLength)
            .IsRequired();

        builder.Property(x => x.ReservedAt)
            .HasColumnName("reserved_at")
            .IsRequired();

        // One active reservation per locale+path in the SEO public namespace.
        builder.HasIndex(x => new { x.Locale, x.Path })
            .IsUnique()
            .HasDatabaseName("ux_seo_path_reservations_locale_path");

        builder.HasIndex(x => new { x.ResourceType, x.ResourceId })
            .HasDatabaseName("ix_seo_path_reservations_resource");
    }
}
