using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Party.Domain;
using PartyAggregate = TravelCore.Modules.Party.Domain.Party;

namespace TravelCore.Modules.Party.Infrastructure.Persistence;

internal sealed class PartyConfiguration : IEntityTypeConfiguration<PartyAggregate>
{
    public void Configure(EntityTypeBuilder<PartyAggregate> builder)
    {
        builder.ToTable("parties");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => PartyId.From(value));

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(PartyAggregate.DisplayNameMaxLength)
            .IsRequired();

        builder.Property(x => x.PrimaryEmail)
            .HasColumnName("primary_email")
            .HasMaxLength(PartyAggregate.ContactMaxLength);

        builder.Property(x => x.PrimaryPhone)
            .HasColumnName("primary_phone")
            .HasMaxLength(PartyAggregate.ContactMaxLength);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.DisplayName).HasDatabaseName("ix_parties_display_name");
        builder.HasIndex(x => x.Kind).HasDatabaseName("ix_parties_kind");
        builder.HasIndex(x => x.Status).HasDatabaseName("ix_parties_status");

        builder.HasOne(x => x.Person)
            .WithOne()
            .HasForeignKey<PersonParty>(x => x.PartyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Organization)
            .WithOne()
            .HasForeignKey<OrganizationParty>(x => x.PartyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Agency)
            .WithOne()
            .HasForeignKey<AgencyParty>(x => x.PartyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Person).AutoInclude();
        builder.Navigation(x => x.Organization).AutoInclude();
        builder.Navigation(x => x.Agency).AutoInclude();
    }
}

internal sealed class PersonPartyConfiguration : IEntityTypeConfiguration<PersonParty>
{
    public void Configure(EntityTypeBuilder<PersonParty> builder)
    {
        builder.ToTable("party_persons");
        builder.HasKey(x => x.PartyId);
        builder.Property(x => x.PartyId)
            .HasColumnName("party_id")
            .HasConversion(id => id.Value, value => PartyId.From(value));
        builder.Property(x => x.GivenName).HasColumnName("given_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.FamilyName).HasColumnName("family_name").HasMaxLength(100).IsRequired();
    }
}

internal sealed class OrganizationPartyConfiguration : IEntityTypeConfiguration<OrganizationParty>
{
    public void Configure(EntityTypeBuilder<OrganizationParty> builder)
    {
        builder.ToTable("party_organizations");
        builder.HasKey(x => x.PartyId);
        builder.Property(x => x.PartyId)
            .HasColumnName("party_id")
            .HasConversion(id => id.Value, value => PartyId.From(value));
        builder.Property(x => x.LegalName).HasColumnName("legal_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.TradeName).HasColumnName("trade_name").HasMaxLength(200);
    }
}

internal sealed class AgencyPartyConfiguration : IEntityTypeConfiguration<AgencyParty>
{
    public void Configure(EntityTypeBuilder<AgencyParty> builder)
    {
        builder.ToTable("party_agencies");
        builder.HasKey(x => x.PartyId);
        builder.Property(x => x.PartyId)
            .HasColumnName("party_id")
            .HasConversion(id => id.Value, value => PartyId.From(value));
        builder.Property(x => x.TradingName).HasColumnName("trading_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.LicenseCode).HasColumnName("license_code").HasMaxLength(64);
    }
}
