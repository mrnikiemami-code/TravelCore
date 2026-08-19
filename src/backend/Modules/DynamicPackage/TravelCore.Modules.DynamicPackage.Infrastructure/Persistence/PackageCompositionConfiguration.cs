using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.DynamicPackage.Domain;

namespace TravelCore.Modules.DynamicPackage.Infrastructure.Persistence;

internal sealed class PackageCompositionConfiguration : IEntityTypeConfiguration<PackageComposition>
{
    public void Configure(EntityTypeBuilder<PackageComposition> builder)
    {
        builder.ToTable("package_compositions", table =>
        {
            table.HasCheckConstraint(
                "ck_package_compositions_refs_required",
                "flight_booking_id IS NOT NULL AND hotel_booking_id IS NOT NULL");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => PackageCompositionId.From(value));

        builder.Property(x => x.FlightBookingId)
            .HasColumnName("flight_booking_id")
            .HasConversion(id => id.Value, value => FlightBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.HotelBookingId)
            .HasColumnName("hotel_booking_id")
            .HasConversion(id => id.Value, value => HotelBookingId.From(value))
            .IsRequired();

        // No FK / no peer-schema references by design (composition boundary only).
    }
}

