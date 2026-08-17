using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure.Persistence;

internal sealed class VisaEntryPolicyConfiguration : IEntityTypeConfiguration<VisaEntryPolicy>
{
    public void Configure(EntityTypeBuilder<VisaEntryPolicy> builder)
    {
        builder.ToTable("visa_entry_policies");
        builder.HasKey(x => x.VisaRequirementSetId);

        builder.Property(x => x.VisaRequirementSetId)
            .HasColumnName("visa_requirement_set_id")
            .HasConversion(id => id.Value, value => VisaRequirementSetId.From(value));

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasMaxLength(VisaEntryKind.MaxLength)
            .HasConversion(kind => kind.Value, value => VisaEntryKind.Parse(value))
            .IsRequired();
    }
}
