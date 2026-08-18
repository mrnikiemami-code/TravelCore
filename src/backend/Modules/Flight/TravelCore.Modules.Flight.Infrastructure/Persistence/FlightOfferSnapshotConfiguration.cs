using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightOfferSnapshotConfiguration : IEntityTypeConfiguration<FlightOfferSnapshot>
{
    public void Configure(EntityTypeBuilder<FlightOfferSnapshot> builder)
    {
        builder.ToTable("flight_offer_snapshots");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FlightOfferSnapshotId.From(value));

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.TripType)
            .HasColumnName("trip_type")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.SourceKey)
            .HasColumnName("source_key")
            .HasMaxLength(FlightOfferSnapshot.SourceKeyMaxLength)
            .IsRequired();

        builder.Property(x => x.SourceOfferReference)
            .HasColumnName("source_offer_reference")
            .HasMaxLength(FlightOfferSnapshot.SourceOfferReferenceMaxLength)
            .IsRequired();

        builder.Property(x => x.QuotedAt).HasColumnName("quoted_at").IsRequired();
        builder.Property(x => x.OfferExpiresAt).HasColumnName("offer_expires_at").IsRequired();
        builder.Property(x => x.AcceptedAt).HasColumnName("accepted_at").IsRequired();

        builder.Property(x => x.Cabin)
            .HasColumnName("cabin")
            .HasMaxLength(FlightOfferSnapshot.CabinMaxLength);

        builder.Property(x => x.BookingClass)
            .HasColumnName("booking_class")
            .HasMaxLength(FlightOfferSnapshot.BookingClassMaxLength);

        builder.Property(x => x.FareBasis)
            .HasColumnName("fare_basis")
            .HasMaxLength(FlightOfferSnapshot.FareBasisMaxLength);

        builder.Property(x => x.FareFamily)
            .HasColumnName("fare_family")
            .HasMaxLength(FlightOfferSnapshot.FareFamilyMaxLength);

        builder.HasOne<Domain.FlightBooking>()
            .WithMany()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Monetary)
            .WithOne()
            .HasForeignKey<FlightBookingMonetarySnapshot>(x => x.FlightOfferSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Monetary).IsRequired();

        builder.HasOne(x => x.FareRules)
            .WithOne()
            .HasForeignKey<FlightFareRulesSnapshot>(x => x.FlightOfferSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.FareRules).IsRequired();

        builder.HasIndex(x => x.FlightBookingId)
            .IsUnique()
            .HasDatabaseName("ux_flight_offer_snapshots_flight_booking_id");

        builder.HasIndex(x => new { x.SourceKey, x.SourceOfferReference })
            .IsUnique()
            .HasDatabaseName("ux_flight_offer_snapshots_source_offer");
    }
}
