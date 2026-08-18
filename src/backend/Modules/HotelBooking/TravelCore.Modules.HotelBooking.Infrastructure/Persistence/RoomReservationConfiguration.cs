using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class RoomReservationConfiguration : IEntityTypeConfiguration<RoomReservation>
{
    public void Configure(EntityTypeBuilder<RoomReservation> builder)
    {
        builder.ToTable("room_reservations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => RoomReservationId.From(value));

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.Ordinal)
            .HasColumnName("ordinal")
            .IsRequired();

        builder.Ignore(x => x.GuestCount);
        builder.Ignore(x => x.AdultCount);
        builder.Ignore(x => x.ChildCount);

        builder.HasIndex(x => new { x.HotelBookingId, x.Ordinal })
            .IsUnique()
            .HasDatabaseName("ux_room_reservations_booking_ordinal");

        builder.HasMany(x => x.Guests)
            .WithOne()
            .HasForeignKey(x => x.RoomReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Guests)
            .HasField("_guests")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
