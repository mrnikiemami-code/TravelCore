using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Ugc.Domain;

namespace TravelCore.Modules.Ugc.Infrastructure.Persistence;

internal sealed class UgcReportConfiguration : IEntityTypeConfiguration<UgcReport>
{
    public void Configure(EntityTypeBuilder<UgcReport> builder)
    {
        builder.ToTable("reports");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => UgcReportId.From(value));

        builder.Property(x => x.ReporterActorId)
            .HasColumnName("reporter_actor_id")
            .IsRequired();

        builder.Property(x => x.TargetType)
            .HasColumnName("target_type")
            .HasMaxLength(UgcReportTargetType.MaxLength)
            .HasConversion(type => type.Value, value => UgcReportTargetType.Parse(value))
            .IsRequired();

        builder.Property(x => x.TargetId)
            .HasColumnName("target_id")
            .IsRequired();

        builder.Ignore(x => x.Target);

        builder.Property(x => x.ReasonCode)
            .HasColumnName("reason_code")
            .HasMaxLength(UgcReportReasonCode.MaxLength)
            .HasConversion(code => code.Value, value => UgcReportReasonCode.Parse(value))
            .IsRequired();

        builder.Property(x => x.OptionalDetail)
            .HasColumnName("optional_detail")
            .HasMaxLength(UgcReport.OptionalDetailMaxLength);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(UgcReportStatus.MaxLength)
            .HasConversion(status => status.Value, value => UgcReportStatus.Parse(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.ReporterActorId)
            .HasDatabaseName("ix_reports_reporter_actor_id");

        builder.HasIndex(x => new { x.TargetType, x.TargetId })
            .HasDatabaseName("ix_reports_target_type_target_id");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_reports_status");
    }
}
