using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure.Persistence;

internal sealed class VisaRequiredDocumentConfiguration : IEntityTypeConfiguration<VisaRequiredDocument>
{
    public void Configure(EntityTypeBuilder<VisaRequiredDocument> builder)
    {
        builder.ToTable("visa_required_documents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => VisaRequiredDocumentId.From(value));

        builder.Property(x => x.VisaRequirementSetId)
            .HasColumnName("visa_requirement_set_id")
            .HasConversion(id => id.Value, value => VisaRequirementSetId.From(value))
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(VisaRequirementCode.MaxLength)
            .IsRequired();

        builder.Property(x => x.RequirementLevel)
            .HasColumnName("requirement_level")
            .HasMaxLength(VisaRequirementLevel.MaxLength)
            .HasConversion(level => level.Value, value => VisaRequirementLevel.Parse(value))
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => new { x.VisaRequirementSetId, x.Code })
            .IsUnique()
            .HasDatabaseName("ux_visa_required_documents_set_code");

        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.RequiredDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Translations)
            .HasField("_translations")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}
