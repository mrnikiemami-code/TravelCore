using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure.Persistence;

internal sealed class FlightTicketingAttemptConfiguration : IEntityTypeConfiguration<FlightTicketingAttempt>
{
    public void Configure(EntityTypeBuilder<FlightTicketingAttempt> builder)
    {
        builder.ToTable("flight_ticketing_attempts", table =>
        {
            table.HasCheckConstraint("ck_flight_ticketing_attempts_status", "status IN (1, 2, 3, 4)");
        });
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FlightTicketingAttemptId.From(value));

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.InitiatedAt).HasColumnName("initiated_at");
        builder.Property(x => x.SucceededAt).HasColumnName("succeeded_at");
        builder.Property(x => x.FailedAt).HasColumnName("failed_at");

        builder.Ignore(x => x.IsUnresolved);
        builder.Ignore(x => x.IsTerminal);

        builder.HasOne<Domain.FlightBooking>()
            .WithMany()
            .HasForeignKey(x => x.FlightBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.FlightBookingId)
            .HasFilter("status IN (1, 2)")
            .IsUnique()
            .HasDatabaseName("ux_flight_ticketing_attempts_one_unresolved");
    }
}
