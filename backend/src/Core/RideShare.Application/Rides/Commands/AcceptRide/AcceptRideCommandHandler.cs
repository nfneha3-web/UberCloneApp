using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;
using RideShare.Domain.Entities;

namespace RideShare.Application.Rides.Commands.AcceptRide;

public class AcceptRideCommandHandler(
    IRideRepository rideRepository,
    IDriverProfileRepository driverProfileRepository,
    IVehicleRepository vehicleRepository,
    IRideQueryService rideQueryService,
    IRideRealtimeNotifier realtimeNotifier,
    IUnitOfWork unitOfWork) : IRequestHandler<AcceptRideCommand, RideDetailsDto>
{
    public async Task<RideDetailsDto> Handle(AcceptRideCommand request, CancellationToken cancellationToken)
    {
        var driverProfile = await driverProfileRepository.GetByApplicationUserIdAsync(request.DriverApplicationUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(DriverProfile), request.DriverApplicationUserId);

        if (driverProfile.AvailabilityStatus != Domain.Enums.DriverAvailabilityStatus.Online)
            throw new ConflictException("You must be online and available to accept a ride.");

        var vehicle = await vehicleRepository.GetActiveVehicleForDriverAsync(driverProfile.Id, cancellationToken)
            ?? throw new ConflictException("Register an active vehicle before accepting rides.");

        var ride = await rideRepository.GetByIdAsync(request.RideId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ride), request.RideId);

        ride.AssignDriver(driverProfile.Id, vehicle.Id);
        driverProfile.MarkOnTrip();

        // Optimistic concurrency (Ride.RowVersion) guarantees that if two drivers race to
        // accept the same ride, only the first SaveChanges wins; the loser gets a ConflictException
        // (translated from DbUpdateConcurrencyException in the Infrastructure UnitOfWork).
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var rideDetails = await rideQueryService.GetByIdAsync(ride.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ride), ride.Id);

        await realtimeNotifier.NotifyRideStatusChangedAsync(rideDetails, cancellationToken);

        return rideDetails;
    }
}
