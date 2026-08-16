using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlaceAggregate = TravelCore.Modules.Place.Domain.Place;
using TravelCore.Modules.Place.Domain;

namespace TravelCore.Modules.Place.Infrastructure.Persistence;

internal sealed class PlaceConfiguration : IEntityTypeConfiguration<PlaceAggregate>
{
    public void Configure(EntityTypeBuilder<PlaceAggregate> builder)
    {
        builder.ToTable("places");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => PlaceId.From(value));

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(PlaceAggregate.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.EnglishName)
            .HasColumnName("english_name")
            .HasMaxLength(PlaceAggregate.NameMaxLength)
            .IsRequired();

        // Logical Destination identity only — deliberately no FK / navigation to Destination.
        builder.Property(x => x.DestinationId)
            .HasColumnName("destination_id");

        builder.Property(x => x.Latitude)
            .HasColumnName("latitude")
            .HasPrecision(9, 6);

        builder.Property(x => x.Longitude)
            .HasColumnName("longitude")
            .HasPrecision(9, 6);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(a => a.Line1)
                .HasColumnName("address_line1")
                .HasMaxLength(PlaceAddress.LineMaxLength);
            address.Property(a => a.Line2)
                .HasColumnName("address_line2")
                .HasMaxLength(PlaceAddress.LineMaxLength);
            address.Property(a => a.Locality)
                .HasColumnName("address_locality")
                .HasMaxLength(PlaceAddress.LocalityMaxLength);
            address.Property(a => a.AdministrativeArea)
                .HasColumnName("address_administrative_area")
                .HasMaxLength(PlaceAddress.AdministrativeAreaMaxLength);
            address.Property(a => a.PostalCode)
                .HasColumnName("address_postal_code")
                .HasMaxLength(PlaceAddress.PostalCodeMaxLength);
            address.Property(a => a.CountryCode)
                .HasColumnName("address_country_code")
                .HasMaxLength(PlaceAddress.CountryCodeMaxLength);
        });

        builder.Navigation(x => x.Address).IsRequired(false);

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("ux_places_code");

        builder.HasIndex(x => x.Kind)
            .HasDatabaseName("ix_places_kind");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_places_created_at");

        builder.HasIndex(x => x.DestinationId)
            .HasDatabaseName("ix_places_destination_id");

        // Same-schema 1:1 specializations — never a cross-schema FK.
        builder.HasOne(x => x.Hotel)
            .WithOne()
            .HasForeignKey<Hotel>(x => x.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Restaurant)
            .WithOne()
            .HasForeignKey<Restaurant>(x => x.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Attraction)
            .WithOne()
            .HasForeignKey<Attraction>(x => x.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Hotel).AutoInclude();
        builder.Navigation(x => x.Restaurant).AutoInclude();
        builder.Navigation(x => x.Attraction).AutoInclude();
        builder.Navigation(x => x.Translations)
            .HasField("_translations")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}

internal sealed class PlaceTranslationConfiguration : IEntityTypeConfiguration<PlaceTranslation>
{
    public void Configure(EntityTypeBuilder<PlaceTranslation> builder)
    {
        builder.ToTable("place_translations");
        builder.HasKey(x => new { x.PlaceId, x.LocaleCode });

        builder.Property(x => x.PlaceId)
            .HasColumnName("place_id")
            .HasConversion(id => id.Value, value => PlaceId.From(value));

        builder.Property(x => x.LocaleCode)
            .HasColumnName("locale_code")
            .HasMaxLength(PlaceTranslation.LocaleCodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(PlaceTranslation.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(PlaceTranslation.DescriptionMaxLength);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.LocaleCode)
            .HasDatabaseName("ix_place_translations_locale_code");
    }
}

internal sealed class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("hotels");
        builder.HasKey(x => x.PlaceId);

        builder.Property(x => x.PlaceId)
            .HasColumnName("place_id")
            .HasConversion(id => id.Value, value => PlaceId.From(value));

        builder.Property(x => x.StarRating)
            .HasColumnName("star_rating");
    }
}

internal sealed class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("restaurants");
        builder.HasKey(x => x.PlaceId);

        builder.Property(x => x.PlaceId)
            .HasColumnName("place_id")
            .HasConversion(id => id.Value, value => PlaceId.From(value));

        builder.Property(x => x.CuisineType)
            .HasColumnName("cuisine_type")
            .HasMaxLength(Restaurant.CuisineTypeMaxLength);
    }
}

internal sealed class AttractionConfiguration : IEntityTypeConfiguration<Attraction>
{
    public void Configure(EntityTypeBuilder<Attraction> builder)
    {
        builder.ToTable("attractions");
        builder.HasKey(x => x.PlaceId);

        builder.Property(x => x.PlaceId)
            .HasColumnName("place_id")
            .HasConversion(id => id.Value, value => PlaceId.From(value));

        builder.Property(x => x.CategoryCode)
            .HasColumnName("category_code")
            .HasMaxLength(Attraction.CategoryCodeMaxLength);
    }
}
