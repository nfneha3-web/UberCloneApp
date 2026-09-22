using RideShare.Domain.Common;

namespace RideShare.Domain.Events;

public sealed record RideRequestedEvent(Guid RideId, Guid RiderProfileId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record RideAcceptedEvent(Guid RideId, Guid DriverProfileId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record RideCompletedEvent(Guid RideId, Guid RiderProfileId, Guid DriverProfileId, decimal FinalFareAmount) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record RideCancelledEvent(Guid RideId, string Reason) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
