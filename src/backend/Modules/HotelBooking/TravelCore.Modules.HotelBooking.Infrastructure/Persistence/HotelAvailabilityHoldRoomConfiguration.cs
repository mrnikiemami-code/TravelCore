using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelAvailabilityHoldRoomConfiguration : IEntityTypeConfiguration<HotelAvailabilityHoldRoom>
{
    public void Configure(EntityTypeBuilder<HotelAvailabilityHoldRoom> builder)
    {
        builder.ToTable("hotel_availability_hold_rooms");
        builder.HasKey(x => new { x.HoldId, x.RoomReservationId });

        builder.Property(x => x.HoldId)
            .HasColumnName("hotel_availability_hold_id")
            .HasConversion(id => id.Value, value => HotelAvailabilityHoldId.From(value));

        builder.Property(x => x.RoomReservationId)
            .HasColumnName("room_reservation_id")
            .HasConversion(id => id.Value, value => RoomReservationId.From(value));

        builder.Property(x => x.SelectionReference)
            .HasColumnName("selection_reference")
            .HasMaxLength(HotelAvailabilityHoldRoom.SelectionMaxLength);
    }
}
