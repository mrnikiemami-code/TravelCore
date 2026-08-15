using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.ReferenceData.Domain;

namespace TravelCore.Modules.ReferenceData.Infrastructure.Persistence;

internal sealed class CurrencyCatalogEntryConfiguration : IEntityTypeConfiguration<CurrencyCatalogEntry>
{
    public void Configure(EntityTypeBuilder<CurrencyCatalogEntry> builder)
    {
        builder.ToTable("currencies");
        builder.HasKey(x => x.Code);
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(12).IsRequired();
        builder.Property(x => x.EnglishName).HasColumnName("english_name").HasMaxLength(CurrencyCatalogEntry.MaxNameLength).IsRequired();
        builder.Property(x => x.MinorUnits).HasColumnName("minor_units").IsRequired();
        builder.Property(x => x.Symbol).HasColumnName("symbol").HasMaxLength(CurrencyCatalogEntry.MaxSymbolLength);
    }
}

internal sealed class LocaleCatalogEntryConfiguration : IEntityTypeConfiguration<LocaleCatalogEntry>
{
    public void Configure(EntityTypeBuilder<LocaleCatalogEntry> builder)
    {
        builder.ToTable("locales");
        builder.HasKey(x => x.Code);
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(LocaleCatalogEntry.MaxCodeLength).IsRequired();
        builder.Property(x => x.EnglishName).HasColumnName("english_name").HasMaxLength(LocaleCatalogEntry.MaxNameLength).IsRequired();
    }
}

internal sealed class CountryCatalogEntryConfiguration : IEntityTypeConfiguration<CountryCatalogEntry>
{
    public void Configure(EntityTypeBuilder<CountryCatalogEntry> builder)
    {
        builder.ToTable("countries");
        builder.HasKey(x => x.Alpha2Code);
        builder.Property(x => x.Alpha2Code).HasColumnName("alpha2_code").HasMaxLength(2).IsRequired();
        builder.Property(x => x.Alpha3Code).HasColumnName("alpha3_code").HasMaxLength(3).IsRequired();
        builder.Property(x => x.NumericCode).HasColumnName("numeric_code").HasMaxLength(3);
        builder.Property(x => x.EnglishName).HasColumnName("english_name").HasMaxLength(CountryCatalogEntry.MaxNameLength).IsRequired();
        builder.HasIndex(x => x.Alpha3Code).IsUnique().HasDatabaseName("ix_countries_alpha3_code");
    }
}

internal sealed class TimeZoneCatalogEntryConfiguration : IEntityTypeConfiguration<TimeZoneCatalogEntry>
{
    public void Configure(EntityTypeBuilder<TimeZoneCatalogEntry> builder)
    {
        builder.ToTable("time_zones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasMaxLength(TimeZoneCatalogEntry.MaxIdLength).IsRequired();
        builder.Property(x => x.EnglishName).HasColumnName("english_name").HasMaxLength(TimeZoneCatalogEntry.MaxNameLength).IsRequired();
    }
}
