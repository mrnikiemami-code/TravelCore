using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Persistence;

internal sealed class HotelBookingAccessCredentialConfiguration
    : IEntityTypeConfiguration<HotelBookingAccessCredential>
{
    public void Configure(EntityTypeBuilder<HotelBookingAccessCredential> builder)
    {
        builder.ToTable("hotel_booking_access_credentials");
        builder.HasKey(x => x.HotelBookingId);

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value));

        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(HotelBookingAccessCredential.TokenHashLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique()
            .HasDatabaseName("ux_hotel_booking_access_credentials_token_hash");

        builder.HasOne<Domain.HotelBooking>()
            .WithMany()
            .HasForeignKey(x => x.HotelBookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
