using TravelCore.Identifiers;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>Strongly typed day-meal identity (UUID v7).</summary>
public readonly record struct ExperienceDayMealId(Guid Value) : IEquatable<ExperienceDayMealId>
{
    public static ExperienceDayMealId New() => new(Uuid7.New());

    public static ExperienceDayMealId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ExperienceDayMealId cannot be empty.", nameof(value));
        }

        return new ExperienceDayMealId(value);
    }

    public override string ToString() => Value.ToString("D");
}

/// <summary>Strongly typed accommodation-plan entry identity (UUID v7).</summary>
public readonly record struct ExperienceAccommodationPlanId(Guid Value) : IEquatable<ExperienceAccommodationPlanId>
{
    public static ExperienceAccommodationPlanId New() => new(Uuid7.New());

    public static ExperienceAccommodationPlanId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ExperienceAccommodationPlanId cannot be empty.", nameof(value));
        }

        return new ExperienceAccommodationPlanId(value);
    }

    public override string ToString() => Value.ToString("D");
}
