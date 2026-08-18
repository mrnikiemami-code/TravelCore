using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingGuestConfiguration : IEntityTypeConfiguration<HotelBookingGuest>
{
    public void Configure(EntityTypeBuilder<HotelBookingGuest> builder)
    {
        builder.ToTable("hotel_booking_guests", table =>
        {
            table.HasCheckConstraint(
                "ck_hotel_booking_guests_age_by_category",
                "(category = 1 AND age_at_check_in IS NULL) OR (category = 2 AND age_at_check_in IS NOT NULL)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => HotelBookingGuestId.From(value));

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.RoomReservationId)
            .HasColumnName("room_reservation_id")
            .HasConversion(id => id.Value, value => RoomReservationId.From(value))
            .IsRequired();

        builder.Property(x => x.GivenName)
            .HasColumnName("given_name")
            .HasMaxLength(HotelBookingGuest.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.FamilyName)
            .HasColumnName("family_name")
            .HasMaxLength(HotelBookingGuest.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.AgeAtCheckIn)
            .HasColumnName("age_at_check_in")
            .HasConversion(
                age => age.HasValue ? age.Value.Years : (int?)null,
                value => value.HasValue ? new HotelGuestAgeAtCheckIn(value.Value) : null);

        builder.Property(x => x.IsLeadGuest)
            .HasColumnName("is_lead_guest")
            .IsRequired();

        builder.HasIndex(x => x.HotelBookingId)
            .HasFilter("is_lead_guest = TRUE")
            .IsUnique()
            .HasDatabaseName("ux_hotel_booking_guests_one_lead");
    }
}
