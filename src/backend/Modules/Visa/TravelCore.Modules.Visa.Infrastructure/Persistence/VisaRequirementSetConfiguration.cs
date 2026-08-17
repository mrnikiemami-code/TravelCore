using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure.Persistence;

internal sealed class VisaRequirementSetConfiguration : IEntityTypeConfiguration<VisaRequirementSet>
{
    public void Configure(EntityTypeBuilder<VisaRequirementSet> builder)
    {
        builder.ToTable("visa_requirement_sets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => VisaRequirementSetId.From(value));

        builder.Property(x => x.VisaDefinitionId)
            .HasColumnName("visa_definition_id")
            .HasConversion(id => id.Value, value => VisaDefinitionId.From(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.VisaDefinitionId)
            .HasDatabaseName("ix_visa_requirement_sets_visa_definition_id");

        builder.HasOne(x => x.Applicability)
            .WithOne()
            .HasForeignKey<VisaApplicability>(x => x.VisaRequirementSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Applicability)
            .HasField("_applicability")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .IsRequired()
            .AutoInclude();
    }
}
