using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Pricing.Domain;

namespace TravelCore.Modules.Pricing.Infrastructure.Persistence;

internal sealed class PriceConfiguration : IEntityTypeConfiguration<Price>
{
    public void Configure(EntityTypeBuilder<Price> builder)
    {
        builder.ToTable("prices");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => PriceId.From(value));

        builder.Property(x => x.TargetType)
            .HasColumnName("target_type")
            .HasMaxLength(PriceTargetType.MaxLength)
            .HasConversion(
                type => type.Value,
                value => PriceTargetType.Parse(value))
            .IsRequired();

        // Logical polymorphic target id only — no FK to tour (or any peer) schema (P12-R3).
        builder.Property(x => x.TargetId)
            .HasColumnName("target_id")
            .IsRequired();

        builder.Ignore(x => x.Currency);
        builder.Ignore(x => x.ComponentsOrdered);

        builder.HasMany(x => x.Components)
            .WithOne()
            .HasForeignKey(x => x.PriceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Components)
            .HasField("_components")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasIndex(x => new { x.TargetType, x.TargetId })
            .HasDatabaseName("ix_prices_target_type_target_id");
    }
}
