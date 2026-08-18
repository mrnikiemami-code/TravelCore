using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Persistence;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Domain.Booking>
{
    public void Configure(EntityTypeBuilder<Domain.Booking> builder)
    {
        builder.ToTable("bookings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => BookingId.From(value));

        builder.Property(x => x.TourDeparture)
            .HasColumnName("tour_departure_id")
            .HasConversion(
                reference => reference.LogicalId,
                value => new TourDepartureReference(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.StatusChangedAt)
            .HasColumnName("status_changed_at")
            .IsRequired();

        builder.HasIndex(x => x.TourDeparture)
            .HasDatabaseName("ix_bookings_tour_departure_id");
    }
}
