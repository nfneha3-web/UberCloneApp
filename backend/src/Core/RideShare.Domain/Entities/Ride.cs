using RideShare.Domain.Common;
using RideShare.Domain.Enums;
using RideShare.Domain.Events;
using RideShare.Domain.Exceptions;
using RideShare.Domain.ValueObjects;

namespace RideShare.Domain.Entities;

/// <summary>
/// Aggregate root for the ride lifecycle. Every state transition is guarded here so that
/// invalid transitions (e.g. completing a ride that was never started) are impossible,
/// regardless of which layer initiates the change (a controller, or the AI copilot).
/// </summary>
public class Ride : BaseEntity
{
    public Guid RiderProfileId { get; private set; }
    public Guid? DriverProfileId { get; private set; }
    public Guid? VehicleId { get; private set; }

    public GeoPoint Pickup { get; private set; } = default!;
    public GeoPoint Dropoff { get; private set; } = default!;
    public VehicleType RequestedVehicleType { get; private set; }

    public RideStatus Status { get; private set; } = RideStatus.Requested;

    public Money EstimatedFare { get; private set; } = default!;
    public Money? FinalFare { get; private set; }
    public double EstimatedDistanceKm { get; private set; }

    public DateTime RequestedAtUtc { get; private set; }
    public DateTime? DriverAssignedAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }

    /// <summary>Concurrency token: prevents two drivers from accepting the same ride offer.</summary>
    public byte[] RowVersion { get; private set; } = default!;

    private Ride() { }

    public static Ride Request(Guid riderProfileId, GeoPoint pickup, GeoPoint dropoff, VehicleType requestedVehicleType, Money estimatedFare, double estimatedDistanceKm)
    {
        if (riderProfileId == Guid.Empty)
            throw new DomainException("A ride must be requested by a rider.");

        var ride = new Ride
        {
            RiderProfileId = riderProfileId,
            Pickup = pickup,
            Dropoff = dropoff,
            RequestedVehicleType = requestedVehicleType,
            EstimatedFare = estimatedFare,
            EstimatedDistanceKm = estimatedDistanceKm,
            Status = RideStatus.Requested,
            RequestedAtUtc = DateTime.UtcNow
        };

        ride.AddDomainEvent(new RideRequestedEvent(ride.Id, riderProfileId));
        return ride;
    }

    public void AssignDriver(Guid driverProfileId, Guid vehicleId)
    {
        if (Status != RideStatus.Requested)
            throw new DomainException($"Cannot assign a driver to a ride in status '{Status}'.");

        DriverProfileId = driverProfileId;
        VehicleId = vehicleId;
        Status = RideStatus.DriverAssigned;
        DriverAssignedAtUtc = DateTime.UtcNow;
        Touch();

        AddDomainEvent(new RideAcceptedEvent(Id, driverProfileId));
    }

    public void MarkDriverArriving()
    {
        if (Status != RideStatus.DriverAssigned)
            throw new DomainException($"Cannot mark driver arriving from status '{Status}'.");

        Status = RideStatus.DriverArriving;
        Touch();
    }

    public void Start()
    {
        if (Status is not (RideStatus.DriverAssigned or RideStatus.DriverArriving))
            throw new DomainException($"Cannot start a ride in status '{Status}'.");

        Status = RideStatus.InProgress;
        StartedAtUtc = DateTime.UtcNow;
        Touch();
    }

    public void Complete(Money finalFare)
    {
        if (Status != RideStatus.InProgress)
            throw new DomainException($"Cannot complete a ride in status '{Status}'.");
        if (DriverProfileId is null)
            throw new DomainException("Cannot complete a ride with no assigned driver.");

        FinalFare = finalFare;
        Status = RideStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        Touch();

        AddDomainEvent(new RideCompletedEvent(Id, RiderProfileId, DriverProfileId.Value, finalFare.Amount));
    }

    public void Cancel(string reason)
    {
        if (Status is RideStatus.Completed or RideStatus.Cancelled)
            throw new DomainException($"Cannot cancel a ride in status '{Status}'.");

        Status = RideStatus.Cancelled;
        CancellationReason = reason;
        CancelledAtUtc = DateTime.UtcNow;
        Touch();

        AddDomainEvent(new RideCancelledEvent(Id, reason));
    }
}
