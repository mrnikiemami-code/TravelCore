using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightTicketConfiguration : IEntityTypeConfiguration<FlightTicket>
{
    public void Configure(EntityTypeBuilder<FlightTicket> builder)
    {
        builder.ToTable("flight_tickets", table =>
        {
            table.HasCheckConstraint("ck_flight_tickets_status", "status IN (1, 2)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FlightTicketId.From(value));

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.PassengerId)
            .HasColumnName("flight_passenger_id")
            .HasConversion(id => id.Value, value => FlightPassengerId.From(value))
            .IsRequired();

        builder.Property(x => x.SourceKey)
            .HasColumnName("source_key")
            .HasMaxLength(FlightTicket.SourceKeyMaxLength)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.SourceTicketNumber)
            .HasColumnName("source_ticket_number")
            .HasMaxLength(FlightTicket.SourceTicketNumberMaxLength);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.IssuedAt).HasColumnName("issued_at");

        builder.HasOne<Domain.FlightBooking>()
            .WithMany()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.FlightBookingId, x.PassengerId })
            .IsUnique()
            .HasDatabaseName("ux_flight_tickets_booking_passenger");

        builder.HasIndex(x => x.SourceTicketNumber)
            .IsUnique()
            .HasFilter("source_ticket_number IS NOT NULL")
            .HasDatabaseName("ux_flight_tickets_source_ticket_number");
    }
}
