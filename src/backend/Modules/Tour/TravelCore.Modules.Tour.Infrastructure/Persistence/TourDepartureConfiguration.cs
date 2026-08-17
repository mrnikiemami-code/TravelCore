using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Persistence;

internal sealed class TourDepartureConfiguration : IEntityTypeConfiguration<TourDeparture>
{
    public void Configure(EntityTypeBuilder<TourDeparture> builder)
    {
        builder.ToTable("tour_departures");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => TourDepartureId.From(value));

        builder.Property(x => x.TourProductId)
            .HasColumnName("tour_product_id")
            .HasConversion(id => id.Value, value => TourProductId.From(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.OwnsOne(x => x.Schedule, schedule =>
        {
            schedule.Property(s => s.StartDate)
                .HasColumnName("start_date")
                .IsRequired();

            schedule.Property(s => s.EndDate)
                .HasColumnName("end_date")
                .IsRequired();

            schedule.Property(s => s.TimeZoneId)
                .HasColumnName("time_zone_id")
                .HasMaxLength(TourDepartureSchedule.TimeZoneIdMaxLength)
                .IsRequired();
        });

        builder.Navigation(x => x.Schedule).IsRequired(false);

        builder.OwnsOne(x => x.Capacity, capacity =>
        {
            capacity.Property(c => c.MinimumPax)
                .HasColumnName("minimum_pax")
                .IsRequired();

            capacity.Property(c => c.MaximumPax)
                .HasColumnName("maximum_pax")
                .IsRequired();
        });

        builder.Navigation(x => x.Capacity).IsRequired(false);

        builder.HasOne<TourProduct>()
            .WithMany()
            .HasForeignKey(x => x.TourProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TourProductId)
            .HasDatabaseName("ix_tour_departures_tour_product_id");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_tour_departures_created_at");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_tour_departures_status");
    }
}
