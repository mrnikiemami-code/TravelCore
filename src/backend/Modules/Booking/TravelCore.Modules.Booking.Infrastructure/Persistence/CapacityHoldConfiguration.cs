using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Persistence;

internal sealed class CapacityHoldConfiguration : IEntityTypeConfiguration<CapacityHold>
{
    public void Configure(EntityTypeBuilder<CapacityHold> builder)
    {
        builder.ToTable("capacity_holds", table =>
        {
            table.HasCheckConstraint("ck_capacity_holds_seat_count_positive", "seat_count > 0");
            table.HasCheckConstraint("ck_capacity_holds_expires_after_created", "expires_at > created_at");
            table.HasCheckConstraint(
                "ck_capacity_holds_observed_capacity_positive",
                "observed_configured_capacity > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => CapacityHoldId.From(value));

        builder.Property(x => x.BookingId)
            .HasColumnName("booking_id")
            .HasConversion(id => id.Value, value => BookingId.From(value))
            .IsRequired();

        builder.Property(x => x.TourDeparture)
            .HasColumnName("tour_departure_id")
            .HasConversion(
                reference => reference.LogicalId,
                value => new TourDepartureReference(value))
            .IsRequired();

        builder.Property(x => x.SeatCount)
            .HasColumnName("seat_count")
            .IsRequired();

        builder.Property(x => x.ObservedConfiguredCapacity)
            .HasColumnName("observed_configured_capacity")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(x => x.StatusChangedAt)
            .HasColumnName("status_changed_at")
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(CapacityHold.IdempotencyKeyMaxLength)
            .IsRequired();

        builder.Ignore(x => x.IsActive);
        builder.Ignore(x => x.IsTerminal);

        builder.HasIndex(x => x.BookingId)
            .HasDatabaseName("ix_capacity_holds_booking_id");

        builder.HasIndex(x => x.TourDeparture)
            .HasDatabaseName("ix_capacity_holds_tour_departure_id");

        builder.HasIndex(x => x.BookingId)
            .HasDatabaseName("ux_capacity_holds_one_active_per_booking")
            .IsUnique()
            .HasFilter("status = 1");

        builder.HasIndex(x => x.IdempotencyKey)
            .HasDatabaseName("ux_capacity_holds_idempotency_key")
            .IsUnique();

        builder.HasOne<Domain.Booking>()
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
