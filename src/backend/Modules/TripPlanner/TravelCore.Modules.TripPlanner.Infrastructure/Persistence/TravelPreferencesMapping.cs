using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.TripPlanner.Domain;
using TravelCore.Money;

namespace TravelCore.Modules.TripPlanner.Infrastructure.Persistence;

internal static class TravelPreferencesMapping
{
    internal static void OwnsTravelPreferences<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        System.Linq.Expressions.Expression<Func<TEntity, TravelPreferences>> navigation,
        string tablePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigation, preferences =>
            ConfigureTravelPreferencesOwned(preferences, tablePrefix));

        builder.Navigation(navigation).IsRequired();
    }

    internal static void ConfigureTravelPreferenceSnapshotOwned(
        OwnedNavigationBuilder<LeadSubmissionSnapshot, TravelPreferenceSnapshot> preferences,
        string tablePrefix)
    {
        preferences.Property(p => p.TravelerNote)
            .HasColumnName($"{tablePrefix}_traveler_note")
            .HasMaxLength(TravelPreferences.TravelerNoteMaxLength);

        preferences.Property(p => p.Accommodation)
            .HasColumnName($"{tablePrefix}_accommodation")
            .HasConversion<string>()
            .HasMaxLength(32);

        preferences.Property(p => p.Transport)
            .HasColumnName($"{tablePrefix}_transport")
            .HasConversion<string>()
            .HasMaxLength(32);

        preferences.Property(p => p.TripStyle)
            .HasColumnName($"{tablePrefix}_trip_style")
            .HasConversion<string>()
            .HasMaxLength(32);

        preferences.OwnsOne(p => p.Timing, timing =>
        {
            timing.Property(t => t.Kind)
                .HasColumnName($"{tablePrefix}_timing_kind")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            timing.Property(t => t.ExactStartDate).HasColumnName($"{tablePrefix}_exact_start_date");
            timing.Property(t => t.ExactEndDate).HasColumnName($"{tablePrefix}_exact_end_date");
            timing.Property(t => t.FlexibleEarliestStart).HasColumnName($"{tablePrefix}_flexible_earliest_start");
            timing.Property(t => t.FlexibleLatestStart).HasColumnName($"{tablePrefix}_flexible_latest_start");
            timing.Property(t => t.FlexibleMaxTripDurationDays).HasColumnName($"{tablePrefix}_flexible_max_trip_duration_days");
            timing.Property(t => t.ApproximateYear).HasColumnName($"{tablePrefix}_approximate_year");
            timing.Property(t => t.ApproximateMonth).HasColumnName($"{tablePrefix}_approximate_month");
            timing.Property(t => t.ApproximateSeason)
                .HasColumnName($"{tablePrefix}_approximate_season")
                .HasConversion<string>()
                .HasMaxLength(16);
        });

        preferences.OwnsOne(p => p.Travelers, travelers =>
        {
            travelers.Property(t => t.AdultCount).HasColumnName($"{tablePrefix}_adult_count");
            travelers.Property(t => t.ChildCount).HasColumnName($"{tablePrefix}_child_count");
            travelers.Property(t => t.InfantCount).HasColumnName($"{tablePrefix}_infant_count");
        });

        preferences.OwnsOne(p => p.Budget, budget =>
        {
            budget.Property(b => b.MinimumAmount)
                .HasColumnName($"{tablePrefix}_budget_min_amount")
                .HasColumnType("numeric(24,8)");
            budget.Property(b => b.MaximumAmount)
                .HasColumnName($"{tablePrefix}_budget_max_amount")
                .HasColumnType("numeric(24,8)");
            budget.Property(b => b.CurrencyCode)
                .HasColumnName($"{tablePrefix}_budget_currency_code")
                .HasConversion(code => code.Value, value => CurrencyCode.Parse(value))
                .HasMaxLength(CurrencyCode.MaxLength)
                .IsRequired();
        });

        preferences.OwnsMany(p => p.Destinations, destination =>
        {
            destination.ToTable($"{tablePrefix}_destination_preferences");
            destination.WithOwner().HasForeignKey("OwnerId");
            destination.Property<int>("Id");
            destination.HasKey("Id");
            destination.Property(d => d.SortOrder).HasColumnName("sort_order").IsRequired();
            destination.Property(d => d.LogicalDestinationId).HasColumnName("logical_destination_id");
            destination.Property(d => d.IsUndecided).HasColumnName("is_undecided").IsRequired();
        });

        preferences.Navigation(p => p.Destinations)
            .HasField("_destinations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        preferences.OwnsMany(p => p.Interests, interest =>
        {
            interest.ToTable($"{tablePrefix}_interest_preferences");
            interest.WithOwner().HasForeignKey("OwnerId");
            interest.Property<int>("Id");
            interest.HasKey("Id");
            interest.Property(i => i.Code)
                .HasColumnName("interest_code")
                .HasMaxLength(InterestPreference.CodeMaxLength)
                .IsRequired();
        });

        preferences.Navigation(p => p.Interests)
            .HasField("_interests")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        preferences.Navigation(p => p.Timing).IsRequired();
    }

    private static void ConfigureTravelPreferencesOwned<TEntity>(
        OwnedNavigationBuilder<TEntity, TravelPreferences> preferences,
        string tablePrefix)
        where TEntity : class
    {
        preferences.Property(p => p.TravelerNote)
            .HasColumnName($"{tablePrefix}_traveler_note")
            .HasMaxLength(TravelPreferences.TravelerNoteMaxLength);

        preferences.Property(p => p.Accommodation)
            .HasColumnName($"{tablePrefix}_accommodation")
            .HasConversion<string>()
            .HasMaxLength(32);

        preferences.Property(p => p.Transport)
            .HasColumnName($"{tablePrefix}_transport")
            .HasConversion<string>()
            .HasMaxLength(32);

        preferences.Property(p => p.TripStyle)
            .HasColumnName($"{tablePrefix}_trip_style")
            .HasConversion<string>()
            .HasMaxLength(32);

        preferences.OwnsOne(p => p.Timing, timing =>
        {
            timing.Property(t => t.Kind)
                .HasColumnName($"{tablePrefix}_timing_kind")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            timing.Property(t => t.ExactStartDate).HasColumnName($"{tablePrefix}_exact_start_date");
            timing.Property(t => t.ExactEndDate).HasColumnName($"{tablePrefix}_exact_end_date");
            timing.Property(t => t.FlexibleEarliestStart).HasColumnName($"{tablePrefix}_flexible_earliest_start");
            timing.Property(t => t.FlexibleLatestStart).HasColumnName($"{tablePrefix}_flexible_latest_start");
            timing.Property(t => t.FlexibleMaxTripDurationDays).HasColumnName($"{tablePrefix}_flexible_max_trip_duration_days");
            timing.Property(t => t.ApproximateYear).HasColumnName($"{tablePrefix}_approximate_year");
            timing.Property(t => t.ApproximateMonth).HasColumnName($"{tablePrefix}_approximate_month");
            timing.Property(t => t.ApproximateSeason)
                .HasColumnName($"{tablePrefix}_approximate_season")
                .HasConversion<string>()
                .HasMaxLength(16);
        });

        preferences.OwnsOne(p => p.Travelers, travelers =>
        {
            travelers.Property(t => t.AdultCount).HasColumnName($"{tablePrefix}_adult_count");
            travelers.Property(t => t.ChildCount).HasColumnName($"{tablePrefix}_child_count");
            travelers.Property(t => t.InfantCount).HasColumnName($"{tablePrefix}_infant_count");
        });

        preferences.OwnsOne(p => p.Budget, budget =>
        {
            budget.Property(b => b.MinimumAmount)
                .HasColumnName($"{tablePrefix}_budget_min_amount")
                .HasColumnType("numeric(24,8)");
            budget.Property(b => b.MaximumAmount)
                .HasColumnName($"{tablePrefix}_budget_max_amount")
                .HasColumnType("numeric(24,8)");
            budget.Property(b => b.CurrencyCode)
                .HasColumnName($"{tablePrefix}_budget_currency_code")
                .HasConversion(code => code.Value, value => CurrencyCode.Parse(value))
                .HasMaxLength(CurrencyCode.MaxLength)
                .IsRequired();
        });

        preferences.OwnsMany(p => p.Destinations, destination =>
        {
            destination.ToTable($"{tablePrefix}_destination_preferences");
            destination.WithOwner().HasForeignKey("OwnerId");
            destination.Property<int>("Id");
            destination.HasKey("Id");
            destination.Property(d => d.SortOrder).HasColumnName("sort_order").IsRequired();
            destination.Property(d => d.LogicalDestinationId).HasColumnName("logical_destination_id");
            destination.Property(d => d.IsUndecided).HasColumnName("is_undecided").IsRequired();
        });

        preferences.Navigation(p => p.Destinations)
            .HasField("_destinations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        preferences.OwnsMany(p => p.Interests, interest =>
        {
            interest.ToTable($"{tablePrefix}_interest_preferences");
            interest.WithOwner().HasForeignKey("OwnerId");
            interest.Property<int>("Id");
            interest.HasKey("Id");
            interest.Property(i => i.Code)
                .HasColumnName("interest_code")
                .HasMaxLength(InterestPreference.CodeMaxLength)
                .IsRequired();
        });

        preferences.Navigation(p => p.Interests)
            .HasField("_interests")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        preferences.Navigation(p => p.Timing).IsRequired();
    }
}
