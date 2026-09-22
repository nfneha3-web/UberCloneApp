using RideShare.Domain.Entities;
using RideShare.Domain.Enums;
using RideShare.Domain.Exceptions;
using RideShare.Domain.ValueObjects;

namespace RideShare.Domain.Tests;

public class RideStateMachineTests
{
    private static Ride CreateRequestedRide() =>
        Ride.Request(
            riderProfileId: Guid.NewGuid(),
            pickup: new GeoPoint(40.7128, -74.0060),
            dropoff: new GeoPoint(40.7300, -73.9950),
            requestedVehicleType: VehicleType.Economy,
            estimatedFare: new Money(12.50m),
            estimatedDistanceKm: 2.5);

    [Fact]
    public void Request_SetsStatusToRequested_AndRaisesDomainEvent()
    {
        var ride = CreateRequestedRide();

        Assert.Equal(RideStatus.Requested, ride.Status);
        Assert.Single(ride.DomainEvents);
    }

    [Fact]
    public void Start_WithoutAnAssignedDriver_Throws()
    {
        var ride = CreateRequestedRide();

        Assert.Throws<DomainException>(() => ride.Start());
    }

    [Fact]
    public void Complete_WithoutStarting_Throws()
    {
        var ride = CreateRequestedRide();
        ride.AssignDriver(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<DomainException>(() => ride.Complete(new Money(12.50m)));
    }

    [Fact]
    public void FullHappyPath_TransitionsThroughEveryStatusInOrder()
    {
        var ride = CreateRequestedRide();

        ride.AssignDriver(Guid.NewGuid(), Guid.NewGuid());
        Assert.Equal(RideStatus.DriverAssigned, ride.Status);

        ride.Start();
        Assert.Equal(RideStatus.InProgress, ride.Status);

        ride.Complete(new Money(15.00m));
        Assert.Equal(RideStatus.Completed, ride.Status);
        Assert.NotNull(ride.CompletedAtUtc);
    }

    [Fact]
    public void Cancel_AfterCompletion_Throws()
    {
        var ride = CreateRequestedRide();
        ride.AssignDriver(Guid.NewGuid(), Guid.NewGuid());
        ride.Start();
        ride.Complete(new Money(15.00m));

        Assert.Throws<DomainException>(() => ride.Cancel("too late"));
    }

    [Fact]
    public void AssignDriver_ToAnAlreadyAssignedRide_Throws()
    {
        var ride = CreateRequestedRide();
        ride.AssignDriver(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<DomainException>(() => ride.AssignDriver(Guid.NewGuid(), Guid.NewGuid()));
    }
}

public class GeoPointTests
{
    [Theory]
    [InlineData(91, 0)]
    [InlineData(-91, 0)]
    [InlineData(0, 181)]
    [InlineData(0, -181)]
    public void Constructor_RejectsOutOfRangeCoordinates(double lat, double lng)
    {
        Assert.Throws<DomainException>(() => new GeoPoint(lat, lng));
    }

    [Fact]
    public void DistanceInKmTo_KnownTwoPointDistance_IsApproximatelyCorrect()
    {
        // Times Square to Central Park, roughly 3.2km apart.
        var timesSquare = new GeoPoint(40.7580, -73.9855);
        var centralPark = new GeoPoint(40.7829, -73.9654);

        var distance = timesSquare.DistanceInKmTo(centralPark);

        Assert.InRange(distance, 2.5, 4.0);
    }
}

public class MoneyTests
{
    [Fact]
    public void Constructor_RejectsNegativeAmount()
    {
        Assert.Throws<DomainException>(() => new Money(-1));
    }

    [Fact]
    public void Add_DifferentCurrencies_Throws()
    {
        var usd = new Money(10, "USD");
        var eur = new Money(10, "EUR");

        Assert.Throws<DomainException>(() => usd.Add(eur));
    }

    [Fact]
    public void Constructor_RoundsToTwoDecimalPlaces()
    {
        var money = new Money(10.126m);

        Assert.Equal(10.13m, money.Amount);
    }
}
