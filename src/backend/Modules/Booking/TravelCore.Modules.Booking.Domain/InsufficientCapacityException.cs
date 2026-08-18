namespace TravelCore.Modules.Booking.Domain;

public sealed class InsufficientCapacityException : InvalidOperationException
{
    public InsufficientCapacityException(int requestedSeats, int availableSeats)
        : base($"Insufficient TourDeparture capacity: requested {requestedSeats}, available {availableSeats}.")
    {
        RequestedSeats = requestedSeats;
        AvailableSeats = availableSeats;
    }

    public int RequestedSeats { get; }

    public int AvailableSeats { get; }
}
