using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Application.Rides.Commands.CancelRide;

public class CancelRideCommandHandler(
    IRideRepository rideRepository,
    IRiderProfileRepository riderProfileRepository,
    IDriverProfileRepository driverProfileRepository,
    IRideQueryService rideQueryService,
    IRideRealtimeNotifier realtimeNotifier,
    IUnitOfWork unitOfWork) : IRequestHandler<CancelRideCommand, Common.Models.RideDetailsDto>
{
    public async Task<Common.Models.RideDetailsDto> Handle(CancelRideCommand request, CancellationToken cancellationToken)
    {
        var ride = await rideRepository.GetByIdAsync(request.RideId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ride), request.RideId);

        var riderProfile = await riderProfileRepository.GetByIdAsync(ride.RiderProfileId, cancellationToken)
            ?? throw new NotFoundException(nameof(RiderProfile), ride.RiderProfileId);

        var isRequestingRider = riderProfile.ApplicationUserId == request.RequestingApplicationUserId;

        var isRequestingDriver = false;
        if (!isRequestingRider && ride.DriverProfileId is not null)
        {
            var driverProfile = await driverProfileRepository.GetByIdAsync(ride.DriverProfileId.Value, cancellationToken);
            isRequestingDriver = driverProfile?.ApplicationUserId == request.RequestingApplicationUserId;

            if (isRequestingDriver && driverProfile is not null)
                driverProfile.MarkAvailableAfterTrip();
        }

        if (!isRequestingRider && !isRequestingDriver)
            throw new ForbiddenAccessException("You are not a participant of this ride.");

        ride.Cancel(request.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var rideDetails = await rideQueryService.GetByIdAsync(ride.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ride), ride.Id);

        await realtimeNotifier.NotifyRideStatusChangedAsync(rideDetails, cancellationToken);

        return rideDetails;
    }
}
