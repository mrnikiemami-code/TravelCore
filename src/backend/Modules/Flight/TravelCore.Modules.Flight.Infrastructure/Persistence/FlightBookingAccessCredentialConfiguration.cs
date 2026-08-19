using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightBookingAccessCredentialConfiguration
    : IEntityTypeConfiguration<FlightBookingAccessCredential>
{
    public void Configure(EntityTypeBuilder<FlightBookingAccessCredential> builder)
    {
        builder.ToTable("flight_booking_access_credentials");
        builder.HasKey(x => x.FlightBookingId);

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value));

        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(FlightBookingAccessCredential.TokenHashLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique()
            .HasDatabaseName("ux_flight_booking_access_credentials_token_hash");

        builder.HasOne<Domain.FlightBooking>()
            .WithMany()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
