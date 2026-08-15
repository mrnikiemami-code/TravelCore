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
    }
}
