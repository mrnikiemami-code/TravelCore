using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.AgencyMarketplace.Domain;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Persistence;

internal sealed class AgencyProfileConfiguration : IEntityTypeConfiguration<AgencyProfile>
{
    public void Configure(EntityTypeBuilder<AgencyProfile> builder)
    {
        builder.ToTable("agency_profiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => AgencyProfileId.From(value));

        // Logical Party identity only — no FK to party schema (P13-R1 / P13-R2).
        builder.Property(x => x.PartyId)
            .HasColumnName("party_id")
            .HasConversion(id => id.Value, value => MarketplacePartyId.From(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.HasIndex(x => x.PartyId)
            .IsUnique()
            .HasDatabaseName("ux_agency_profiles_party_id");

        builder.OwnsOne(x => x.Display, display =>
        {
            display.Property(p => p.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(AgencyDisplayInfo.DisplayNameMaxLength)
                .IsRequired();
            display.Property(p => p.Description)
                .HasColumnName("description")
                .HasMaxLength(AgencyDisplayInfo.DescriptionMaxLength);
            display.Property(p => p.LogoMediaAssetId)
                .HasColumnName("logo_media_asset_id");
        });

        builder.Navigation(x => x.Display).IsRequired();

        builder.OwnsOne(x => x.Contact, contact =>
        {
            contact.Property(p => p.PublicEmail)
                .HasColumnName("public_email")
                .HasMaxLength(AgencyContactSettings.ContactMaxLength);
            contact.Property(p => p.PublicPhone)
                .HasColumnName("public_phone")
                .HasMaxLength(AgencyContactSettings.ContactMaxLength);
            contact.Property(p => p.WebsiteUrl)
                .HasColumnName("website_url")
                .HasMaxLength(AgencyContactSettings.WebsiteMaxLength);
        });

        builder.Navigation(x => x.Contact).IsRequired();

        builder.OwnsOne(x => x.Commercial, commercial =>
        {
            commercial.Property(p => p.PublicListingEnabled)
                .HasColumnName("public_listing_enabled")
                .IsRequired();
        });

        builder.Navigation(x => x.Commercial).IsRequired();
    }
}
