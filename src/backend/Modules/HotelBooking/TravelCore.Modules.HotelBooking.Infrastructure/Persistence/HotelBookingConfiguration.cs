using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;
using HotelBookingAggregate = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingConfiguration : IEntityTypeConfiguration<HotelBookingAggregate>
{
    public void Configure(EntityTypeBuilder<HotelBookingAggregate> builder)
    {
        builder.ToTable("hotel_bookings", table =>
        {
            table.HasCheckConstraint(
                "ck_hotel_bookings_checkout_after_checkin",
                "check_out_date > check_in_date");
            table.HasCheckConstraint(
                "ck_hotel_bookings_status",
                "status IN (1, 2, 3)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value));

        builder.Property(x => x.Place)
            .HasColumnName("place_id")
            .HasConversion(
                reference => reference.PlaceId,
                value => new HotelPlaceReference(value))
            .IsRequired();

        builder.Property(x => x.CheckInDate)
            .HasColumnName("check_in_date")
            .IsRequired();

        builder.Property(x => x.CheckOutDate)
            .HasColumnName("check_out_date")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .HasDefaultValue(HotelBookingStatus.Pending)
            .IsRequired();

        builder.Property(x => x.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.Property(x => x.CancelledAt)
            .HasColumnName("cancelled_at");

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .HasDefaultValue(0L)
            .IsRequired();

        builder.Property(x => x.ActorAccountId)
            .HasColumnName("actor_account_id");

        builder.Ignore(x => x.Nights);
        builder.Ignore(x => x.RoomCount);
        builder.Ignore(x => x.GuestCount);
        builder.Ignore(x => x.AdultCount);
        builder.Ignore(x => x.ChildCount);
        builder.Ignore(x => x.LeadGuest);
        builder.Ignore(x => x.Guests);

        builder.OwnsOne(x => x.Contact, contact =>
        {
            contact.Property(c => c.Email)
                .HasColumnName("contact_email")
                .HasMaxLength(HotelBookingContactSnapshot.EmailMaxLength);
            contact.Property(c => c.NormalizedEmail)
                .HasColumnName("contact_normalized_email")
                .HasMaxLength(HotelBookingContactSnapshot.EmailMaxLength);
            contact.Property(c => c.Phone)
                .HasColumnName("contact_phone")
                .HasMaxLength(HotelBookingContactSnapshot.PhoneMaxLength);
        });
        builder.Navigation(x => x.Contact).IsRequired();

        builder.HasMany(x => x.Rooms)
            .WithOne()
            .HasForeignKey(x => x.HotelBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Rooms)
            .HasField("_rooms")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
