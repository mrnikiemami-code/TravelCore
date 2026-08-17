using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.AgencyMarketplace.Domain;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Persistence;

internal sealed class AgencyOfferConfiguration : IEntityTypeConfiguration<AgencyOffer>
{
    public void Configure(EntityTypeBuilder<AgencyOffer> builder)
    {
        builder.ToTable("agency_offers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => AgencyOfferId.From(value));

        builder.Property(x => x.AgencyProfileId)
            .HasColumnName("agency_profile_id")
            .HasConversion(id => id.Value, value => AgencyProfileId.From(value))
            .IsRequired();

        // Logical TourProduct identity only — no FK to tour schema (P13-R3).
        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Visibility)
            .HasColumnName("visibility")
            .HasConversion<short>()
            .IsRequired();

        builder.HasIndex(x => new { x.AgencyProfileId, x.TourProductId })
            .IsUnique()
            .HasDatabaseName("ux_agency_offers_profile_tour_product");

        builder.HasIndex(x => x.TourProductId)
            .HasDatabaseName("ix_agency_offers_tour_product_id");

        builder.HasOne<AgencyProfile>()
            .WithMany()
            .HasForeignKey(x => x.AgencyProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(x => x.Display, display =>
        {
            display.Property(p => p.TitleOverride)
                .HasColumnName("title_override")
                .HasMaxLength(AgencyOfferDisplaySettings.TitleMaxLength);
            display.Property(p => p.Highlight)
                .HasColumnName("highlight")
                .HasMaxLength(AgencyOfferDisplaySettings.HighlightMaxLength);
        });
        builder.Navigation(x => x.Display).IsRequired();

        builder.OwnsOne(x => x.CommercialTerms, terms =>
        {
            terms.Property(p => p.Notes)
                .HasColumnName("commercial_notes")
                .HasMaxLength(AgencyOfferCommercialTerms.NotesMaxLength);
        });
        builder.Navigation(x => x.CommercialTerms).IsRequired();
    }
}
