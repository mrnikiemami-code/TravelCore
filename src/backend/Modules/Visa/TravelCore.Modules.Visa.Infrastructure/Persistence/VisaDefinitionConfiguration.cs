using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure.Persistence;

internal sealed class VisaDefinitionConfiguration : IEntityTypeConfiguration<VisaDefinition>
{
    public void Configure(EntityTypeBuilder<VisaDefinition> builder)
    {
        builder.ToTable("visa_definitions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => VisaDefinitionId.From(value));

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(VisaDefinition.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("ux_visa_definitions_code");

        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.VisaDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Translations)
            .HasField("_translations")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(x => x.RequirementSets)
            .WithOne()
            .HasForeignKey(x => x.VisaDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.RequirementSets)
            .HasField("_requirementSets")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}
