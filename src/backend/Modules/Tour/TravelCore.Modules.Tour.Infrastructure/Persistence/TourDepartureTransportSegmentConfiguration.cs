using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Persistence;

internal sealed class TourDepartureTransportSegmentConfiguration
    : IEntityTypeConfiguration<TourDepartureTransportSegment>
{
    public void Configure(EntityTypeBuilder<TourDepartureTransportSegment> builder)
    {
        builder.ToTable("tour_departure_transport_segments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => TourDepartureTransportSegmentId.From(value));

        builder.Property(x => x.TourDepartureId)
            .HasColumnName("tour_departure_id")
            .HasConversion(id => id.Value, value => TourDepartureId.From(value))
            .IsRequired();

        builder.Property(x => x.Sequence)
            .HasColumnName("sequence")
            .IsRequired();

        builder.Property(x => x.TransportMode)
            .HasColumnName("transport_mode")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Origin)
            .HasColumnName("origin")
            .HasMaxLength(TourDepartureTransportSegment.LabelMaxLength)
            .IsRequired();

        builder.Property(x => x.Destination)
            .HasColumnName("destination")
            .HasMaxLength(TourDepartureTransportSegment.LabelMaxLength)
            .IsRequired();

        builder.HasIndex(x => new { x.TourDepartureId, x.Sequence })
            .IsUnique()
            .HasDatabaseName("ux_tour_departure_transport_segments_departure_sequence");
    }
}
