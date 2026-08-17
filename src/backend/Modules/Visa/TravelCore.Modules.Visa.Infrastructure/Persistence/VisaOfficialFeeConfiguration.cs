using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure.Persistence;

internal sealed class VisaOfficialFeeConfiguration : IEntityTypeConfiguration<VisaOfficialFee>
{
    public void Configure(EntityTypeBuilder<VisaOfficialFee> builder)
    {
        builder.ToTable("visa_official_fees");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => VisaOfficialFeeId.From(value));

        builder.Property(x => x.VisaRequirementSetId)
            .HasColumnName("visa_requirement_set_id")
            .HasConversion(id => id.Value, value => VisaRequirementSetId.From(value))
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasMaxLength(VisaOfficialFeeKind.MaxLength)
            .HasConversion(kind => kind.Value, value => VisaOfficialFeeKind.Parse(value))
            .IsRequired();

        builder.OwnsRequiredMoney(x => x.Money, "amount", "currency_code");

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.Property(x => x.Source)
            .HasColumnName("source")
            .HasMaxLength(VisaOfficialFee.SourceMaxLength);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => new { x.VisaRequirementSetId, x.Kind })
            .IsUnique()
            .HasDatabaseName("ux_visa_official_fees_set_kind");
    }
}
