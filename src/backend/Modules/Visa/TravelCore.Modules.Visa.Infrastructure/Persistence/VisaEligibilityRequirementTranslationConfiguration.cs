using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure.Persistence;

internal sealed class VisaEligibilityRequirementTranslationConfiguration : IEntityTypeConfiguration<VisaEligibilityRequirementTranslation>
{
    public void Configure(EntityTypeBuilder<VisaEligibilityRequirementTranslation> builder)
    {
        builder.ToTable("visa_eligibility_requirement_translations");
        builder.HasKey(x => new { x.EligibilityRequirementId, x.LocaleCode });

        builder.Property(x => x.EligibilityRequirementId)
            .HasColumnName("eligibility_requirement_id")
            .HasConversion(id => id.Value, value => VisaEligibilityRequirementId.From(value));

        builder.Property(x => x.LocaleCode)
            .HasColumnName("locale_code")
            .HasMaxLength(VisaEligibilityRequirementTranslation.LocaleCodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(VisaEligibilityRequirementTranslation.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(VisaEligibilityRequirementTranslation.NotesMaxLength);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
