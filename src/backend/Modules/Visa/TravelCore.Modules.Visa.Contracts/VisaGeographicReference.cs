namespace TravelCore.Modules.Visa.Contracts;

/// <summary>
/// Opaque logical geographic/reference id for future Visa applicability.
/// Visa does not own Country/Destination/ReferenceData entities (P17-R1).
/// </summary>
public readonly record struct VisaGeographicReference(Guid GeographicId);
