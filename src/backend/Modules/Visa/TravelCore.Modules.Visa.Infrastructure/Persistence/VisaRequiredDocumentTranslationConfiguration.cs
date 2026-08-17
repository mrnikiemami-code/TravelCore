using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure.Persistence;

internal sealed class VisaRequiredDocumentTranslationConfiguration : IEntityTypeConfiguration<VisaRequiredDocumentTranslation>
{
    public void Configure(EntityTypeBuilder<VisaRequiredDocumentTranslation> builder)
    {
        builder.ToTable("visa_required_document_translations");
        builder.HasKey(x => new { x.RequiredDocumentId, x.LocaleCode });

        builder.Property(x => x.RequiredDocumentId)
            .HasColumnName("required_document_id")
            .HasConversion(id => id.Value, value => VisaRequiredDocumentId.From(value));

        builder.Property(x => x.LocaleCode)
            .HasColumnName("locale_code")
            .HasMaxLength(VisaRequiredDocumentTranslation.LocaleCodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(VisaRequiredDocumentTranslation.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(VisaRequiredDocumentTranslation.NotesMaxLength);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
