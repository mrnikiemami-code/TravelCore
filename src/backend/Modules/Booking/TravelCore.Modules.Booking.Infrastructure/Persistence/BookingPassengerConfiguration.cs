using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Persistence;

internal sealed class BookingPassengerConfiguration : IEntityTypeConfiguration<BookingPassenger>
{
    public void Configure(EntityTypeBuilder<BookingPassenger> builder)
    {
        builder.ToTable("booking_passengers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => BookingPassengerId.From(value));

        builder.Property(x => x.GivenName)
            .HasColumnName("given_name")
            .HasMaxLength(BookingPassenger.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.FamilyName)
            .HasColumnName("family_name")
            .HasMaxLength(BookingPassenger.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasColumnName("traveler_category")
            .IsRequired();

        builder.Property(x => x.Sequence)
            .HasColumnName("sequence")
            .IsRequired();

        builder.Property<BookingId>("BookingId")
            .HasColumnName("booking_id")
            .HasConversion(id => id.Value, value => BookingId.From(value));

        builder.HasIndex("BookingId")
            .HasDatabaseName("ix_booking_passengers_booking_id");
    }
}
