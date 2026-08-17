using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure.Persistence;

internal sealed class VisaDefinitionTranslationConfiguration : IEntityTypeConfiguration<VisaDefinitionTranslation>
{
    public void Configure(EntityTypeBuilder<VisaDefinitionTranslation> builder)
    {
        builder.ToTable("visa_definition_translations");
        builder.HasKey(x => new { x.VisaDefinitionId, x.LocaleCode });

        builder.Property(x => x.VisaDefinitionId)
            .HasColumnName("visa_definition_id")
            .HasConversion(id => id.Value, value => VisaDefinitionId.From(value));

        builder.Property(x => x.LocaleCode)
            .HasColumnName("locale_code")
            .HasMaxLength(VisaDefinitionTranslation.LocaleCodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(VisaDefinitionTranslation.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Summary)
            .HasColumnName("summary")
            .HasMaxLength(VisaDefinitionTranslation.SummaryMaxLength);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.LocaleCode)
            .HasDatabaseName("ix_visa_definition_translations_locale_code");
    }
}
