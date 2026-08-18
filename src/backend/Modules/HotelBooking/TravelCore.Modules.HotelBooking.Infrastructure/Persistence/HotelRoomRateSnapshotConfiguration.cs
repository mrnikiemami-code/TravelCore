using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelRoomRateSnapshotConfiguration : IEntityTypeConfiguration<HotelRoomRateSnapshot>
{
    public void Configure(EntityTypeBuilder<HotelRoomRateSnapshot> builder)
    {
        builder.ToTable("hotel_room_rate_snapshots");
        builder.HasKey(x => new { x.HotelRateOfferSnapshotId, x.RoomReservationId });

        builder.Property(x => x.HotelRateOfferSnapshotId)
            .HasColumnName("hotel_rate_offer_snapshot_id")
            .HasConversion(id => id.Value, value => HotelRateOfferSnapshotId.From(value));

        builder.Property(x => x.RoomReservationId)
            .HasColumnName("room_reservation_id")
            .HasConversion(id => id.Value, value => RoomReservationId.From(value));

        builder.OwnsOptionalMoney(x => x.Amount, "amount", "currency_code");

        builder.Property(x => x.AvailabilitySelectionReference)
            .HasColumnName("availability_selection_reference")
            .HasMaxLength(HotelRoomRateSnapshot.ReferenceMaxLength);

        builder.Property(x => x.SourceRateReference)
            .HasColumnName("source_rate_reference")
            .HasMaxLength(HotelRoomRateSnapshot.ReferenceMaxLength);

        builder.Property(x => x.BoardBasisCode)
            .HasColumnName("board_basis_code")
            .HasMaxLength(HotelRoomRateSnapshot.BoardBasisMaxLength);
    }
}
