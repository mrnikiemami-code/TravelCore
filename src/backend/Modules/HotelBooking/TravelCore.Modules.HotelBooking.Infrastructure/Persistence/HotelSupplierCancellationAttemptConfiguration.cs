using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelSupplierCancellationAttemptConfiguration
    : IEntityTypeConfiguration<HotelSupplierCancellationAttempt>
{
    public void Configure(EntityTypeBuilder<HotelSupplierCancellationAttempt> builder)
    {
        builder.ToTable("hotel_supplier_cancellation_attempts", table =>
        {
            table.HasCheckConstraint(
                "ck_hotel_supplier_cancellation_attempts_status",
                "status IN (1, 2, 3, 4)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => HotelSupplierCancellationAttemptId.From(value));

        builder.Property(x => x.CancellationId)
            .HasColumnName("hotel_booking_cancellation_id")
            .HasConversion(id => id.Value, value => HotelBookingCancellationId.From(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.InitiatedAt).HasColumnName("initiated_at");
        builder.Property(x => x.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(x => x.FailedAt).HasColumnName("failed_at");

        builder.Ignore(x => x.IsUnresolved);
        builder.Ignore(x => x.IsTerminal);

        builder.HasIndex(x => x.CancellationId)
            .HasFilter("status IN (1, 2)")
            .IsUnique()
            .HasDatabaseName("ux_hotel_supplier_cancellation_attempts_one_unresolved");
    }
}
