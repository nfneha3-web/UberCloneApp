using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;
using RideShare.Domain.Entities;

namespace RideShare.Application.Rides.Commands.StartRide;

public class StartRideCommandHandler(
    IRideRepository rideRepository,
    IDriverProfileRepository driverProfileRepository,
    IRideQueryService rideQueryService,
    IRideRealtimeNotifier realtimeNotifier,
    IUnitOfWork unitOfWork) : IRequestHandler<StartRideCommand, RideDetailsDto>
{
    public async Task<RideDetailsDto> Handle(StartRideCommand request, CancellationToken cancellationToken)
    {
        var driverProfile = await driverProfileRepository.GetByApplicationUserIdAsync(request.DriverApplicationUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(DriverProfile), request.DriverApplicationUserId);

        var ride = await rideRepository.GetByIdAsync(request.RideId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ride), request.RideId);

        if (ride.DriverProfileId != driverProfile.Id)
            throw new ForbiddenAccessException("Only the assigned driver can start this ride.");

        ride.Start();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var rideDetails = await rideQueryService.GetByIdAsync(ride.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ride), ride.Id);

        await realtimeNotifier.NotifyRideStatusChangedAsync(rideDetails, cancellationToken);

        return rideDetails;
    }
}
