using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure.Persistence;

internal sealed class VisaApplicabilityConfiguration : IEntityTypeConfiguration<VisaApplicability>
{
    public void Configure(EntityTypeBuilder<VisaApplicability> builder)
    {
        builder.ToTable("visa_applicabilities");
        builder.HasKey(x => x.VisaRequirementSetId);

        builder.Property(x => x.VisaRequirementSetId)
            .HasColumnName("visa_requirement_set_id")
            .HasConversion(id => id.Value, value => VisaRequirementSetId.From(value));

        builder.Property(x => x.DestinationGeographicId)
            .HasColumnName("destination_geographic_id")
            .IsRequired();

        builder.Property(x => x.ApplicantNationalityCode)
            .HasColumnName("applicant_nationality_code")
            .HasMaxLength(VisaApplicability.CountryCodeMaxLength);

        builder.Property(x => x.ResidenceCountryCode)
            .HasColumnName("residence_country_code")
            .HasMaxLength(VisaApplicability.CountryCodeMaxLength);

        builder.Property(x => x.ApplicantCategory)
            .HasColumnName("applicant_category")
            .HasMaxLength(VisaApplicantCategory.MaxLength)
            .HasConversion(
                category => category == null ? null : category.Value,
                value => VisaApplicantCategory.ParseOptional(value));

        builder.HasIndex(x => x.DestinationGeographicId)
            .HasDatabaseName("ix_visa_applicabilities_destination_geographic_id");
    }
}
