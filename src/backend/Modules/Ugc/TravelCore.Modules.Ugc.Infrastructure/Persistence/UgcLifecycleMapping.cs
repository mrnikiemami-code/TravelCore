using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Ugc.Domain;

namespace TravelCore.Modules.Ugc.Infrastructure.Persistence;

internal static class UgcLifecycleMapping
{
    public static void Map<T>(
        EntityTypeBuilder<T> builder,
        Expression<Func<T, ModerationStatus>> moderationStatus,
        Expression<Func<T, PublicationStatus>> publicationStatus,
        string moderationIndexName,
        string publicationIndexName)
        where T : class
    {
        builder.Ignore("IsPubliclyEligible");

        builder.Property(moderationStatus)
            .HasColumnName("moderation_status")
            .HasMaxLength(ModerationStatus.MaxLength)
            .HasConversion(status => status.Value, value => ModerationStatus.Parse(value))
            .IsRequired();

        builder.Property(publicationStatus)
            .HasColumnName("publication_status")
            .HasMaxLength(PublicationStatus.MaxLength)
            .HasConversion(status => status.Value, value => PublicationStatus.Parse(value))
            .IsRequired();

        builder.HasIndex("ModerationStatus")
            .HasDatabaseName(moderationIndexName);

        builder.HasIndex("PublicationStatus")
            .HasDatabaseName(publicationIndexName);
    }
}
