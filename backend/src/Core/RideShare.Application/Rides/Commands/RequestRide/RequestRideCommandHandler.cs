using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;
using RideShare.Domain.Entities;
using RideShare.Domain.ValueObjects;

namespace RideShare.Application.Rides.Commands.RequestRide;

public class RequestRideCommandHandler(
    IRiderProfileRepository riderProfileRepository,
    IRideRepository rideRepository,
    IRideQueryService rideQueryService,
    IDriverSearchQueryService driverSearchQueryService,
    IRideRealtimeNotifier realtimeNotifier,
    IFareCalculator fareCalculator,
    IUnitOfWork unitOfWork) : IRequestHandler<RequestRideCommand, RideDetailsDto>
{
    private const double DriverSearchRadiusKm = 8.0;

    public async Task<RideDetailsDto> Handle(RequestRideCommand request, CancellationToken cancellationToken)
    {
        var riderProfile = await riderProfileRepository.GetByApplicationUserIdAsync(request.RiderApplicationUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(RiderProfile), request.RiderApplicationUserId);

        var existingActiveRide = await rideRepository.GetActiveRideForRiderAsync(riderProfile.Id, cancellationToken);
        if (existingActiveRide is not null)
            throw new ConflictException("You already have an active ride in progress.");

        var pickup = new GeoPoint(request.PickupLatitude, request.PickupLongitude, request.PickupAddress);
        var dropoff = new GeoPoint(request.DropoffLatitude, request.DropoffLongitude, request.DropoffAddress);
        var distanceKm = pickup.DistanceInKmTo(dropoff);
        var estimatedFare = fareCalculator.EstimateFare(distanceKm, request.VehicleType);

        var ride = Ride.Request(riderProfile.Id, pickup, dropoff, request.VehicleType, estimatedFare, distanceKm);
        rideRepository.Add(ride);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var rideDetails = await rideQueryService.GetByIdAsync(ride.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ride), ride.Id);

        // Best-effort fan-out to nearby available drivers over SignalR; the ride itself
        // already exists and is safe to retry/re-broadcast if this step fails.
        var nearbyDrivers = await driverSearchQueryService.FindNearbyAvailableDriversAsync(
            pickup.Latitude, pickup.Longitude, DriverSearchRadiusKm, request.VehicleType, cancellationToken);

        foreach (var driver in nearbyDrivers)
            await realtimeNotifier.NotifyDriverOfferAsync(driver.DriverProfileId, rideDetails, cancellationToken);

        return rideDetails;
    }
}
