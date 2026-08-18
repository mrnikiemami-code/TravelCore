using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Persistence;

internal sealed class BookingAccessCredentialConfiguration : IEntityTypeConfiguration<BookingAccessCredential>
{
    public void Configure(EntityTypeBuilder<BookingAccessCredential> builder)
    {
        builder.ToTable("booking_access_credentials");
        builder.HasKey(x => x.BookingId);

        builder.Property(x => x.BookingId)
            .HasColumnName("booking_id")
            .HasConversion(id => id.Value, value => BookingId.From(value));

        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(BookingAccessCredential.TokenHashLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique()
            .HasDatabaseName("ux_booking_access_credentials_token_hash");

        builder.HasOne<Domain.Booking>()
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
