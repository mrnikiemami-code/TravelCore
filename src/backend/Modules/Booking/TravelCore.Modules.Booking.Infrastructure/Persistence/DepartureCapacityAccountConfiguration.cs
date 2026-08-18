using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Persistence;

internal sealed class DepartureCapacityAccountConfiguration : IEntityTypeConfiguration<DepartureCapacityAccount>
{
    public void Configure(EntityTypeBuilder<DepartureCapacityAccount> builder)
    {
        builder.ToTable("departure_capacity_accounts", table =>
        {
            table.HasCheckConstraint("ck_departure_capacity_accounts_active_seats_nonnegative", "active_seats >= 0");
            table.HasCheckConstraint(
                "ck_departure_capacity_accounts_consumed_seats_nonnegative",
                "consumed_seats >= 0");
        });

        builder.HasKey(x => x.TourDeparture);

        builder.Property(x => x.TourDeparture)
            .HasColumnName("tour_departure_id")
            .HasConversion(
                reference => reference.LogicalId,
                value => new TourDepartureReference(value));

        builder.Property(x => x.ActiveSeats)
            .HasColumnName("active_seats")
            .IsRequired();

        builder.Property(x => x.ConsumedSeats)
            .HasColumnName("consumed_seats")
            .IsRequired();

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .IsRequired();

        builder.Ignore(x => x.EffectiveSeats);
    }
}
