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

        builder.Property(x => x.ActorReference)
            .HasColumnName("actor_reference_id")
            .HasConversion(
                reference => reference.HasValue ? reference.Value.ActorId : (Guid?)null,
                value => value.HasValue ? new BookingActorReference(value.Value) : null);

        builder.Property(x => x.PartyReference)
            .HasColumnName("party_reference_id")
            .HasConversion(
                reference => reference.HasValue ? reference.Value.PartyId : (Guid?)null,
                value => value.HasValue ? new BookingPartyReference(value.Value) : null);

        builder.Ignore(x => x.PassengerCount);

        builder.OwnsOne(x => x.Contact, contact =>
        {
            contact.Property(c => c.DisplayName)
                .HasColumnName("contact_display_name")
                .HasMaxLength(BookingContactSnapshot.DisplayNameMaxLength);
            contact.Property(c => c.Email)
                .HasColumnName("contact_email")
                .HasMaxLength(BookingContactSnapshot.EmailMaxLength);
            contact.Property(c => c.NormalizedEmail)
                .HasColumnName("contact_normalized_email")
                .HasMaxLength(BookingContactSnapshot.EmailMaxLength);
            contact.Property(c => c.Phone)
                .HasColumnName("contact_phone")
                .HasMaxLength(BookingContactSnapshot.PhoneMaxLength);
        });

        builder.Navigation(x => x.Contact).IsRequired(false);

        builder.HasMany(x => x.Passengers)
            .WithOne()
            .HasForeignKey("BookingId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Passengers)
            .HasField("_passengers")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(x => x.MonetarySnapshot)
            .WithOne()
            .HasForeignKey<BookingMonetarySnapshot>(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.MonetarySnapshot)
            .IsRequired(false);

        builder.HasIndex(x => x.TourDeparture)
            .HasDatabaseName("ix_bookings_tour_departure_id");
    }
}
