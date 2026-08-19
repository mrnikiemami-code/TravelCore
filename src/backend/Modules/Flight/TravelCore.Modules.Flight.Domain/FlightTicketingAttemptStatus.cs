namespace TravelCore.Modules.Flight.Domain;

public enum FlightTicketingAttemptStatus : short
{
    Created = 1,
    Initiated = 2,
    Succeeded = 3,
    Failed = 4,
}
