using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure.Persistence;

internal sealed class VisaValidityConfiguration : IEntityTypeConfiguration<VisaValidity>
{
    public void Configure(EntityTypeBuilder<VisaValidity> builder)
    {
        builder.ToTable("visa_validities");
        builder.HasKey(x => x.VisaRequirementSetId);

        builder.Property(x => x.VisaRequirementSetId)
            .HasColumnName("visa_requirement_set_id")
            .HasConversion(id => id.Value, value => VisaRequirementSetId.From(value));

        builder.Property(x => x.Value)
            .HasColumnName("value")
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasColumnName("unit")
            .HasMaxLength(VisaTimeUnit.MaxLength)
            .HasConversion(unit => unit.Value, value => VisaTimeUnit.Parse(value))
            .IsRequired();
    }
}
