namespace TravelCore.Evolution;

/// <summary>
/// Post-P29 ownership separation. Evolution abstractions must not become business-module SoR or replace domain ownership.
/// </summary>
public static class EvolutionOwnershipBoundary
{
    public const string OwnerPlatform = "Platform";
    public const string SearchOwner = "Search";
    public const string AnalyticsOwner = "Analytics";
    public const string PerformanceOwner = "Performance";
    public const string HardeningOwner = "Hardening";
    public const string PricingOwner = "Pricing";
    public const string BookingOwner = "Booking";

    public const string EvolutionIsNotSearchRanking = "Evolution != SearchRanking";
    public const string EvolutionIsNotProductAnalytics = "Evolution != ProductAnalytics";
    public const string EvolutionIsNotPerformanceOptimization = "Evolution != PerformanceOptimization";
    public const string EvolutionIsNotHardeningExecution = "Evolution != HardeningExecution";
    public const string EvolutionIsNotBookingExecution = "Evolution != BookingExecution";
    public const string EvolutionIsNotPricingSoR = "Evolution != PricingSoR";
    public const string EvolutionIsNotDomainModuleOwner = "Evolution != DomainModuleOwner";

    public const bool OwnsSearchRanking = false;
    public const bool OwnsProductAnalytics = false;
    public const bool OwnsPerformanceOptimization = false;
    public const bool OwnsHardeningExecution = false;
    public const bool OwnsBookingExecution = false;
    public const bool OwnsPricingFacts = false;
    public const bool OwnsDomainModules = false;
    public const bool FoundationBoundaryImplemented = true;
    public const bool EvolutionGuardrailsImplemented = true;
}
