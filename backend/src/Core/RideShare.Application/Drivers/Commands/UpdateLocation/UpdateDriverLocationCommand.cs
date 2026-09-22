using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;
using RideShare.Domain.ValueObjects;

namespace RideShare.Application.Drivers.Commands.UpdateLocation;

public sealed record UpdateDriverLocationCommand(Guid DriverApplicationUserId, double Latitude, double Longitude, Guid? ActiveRideId) : IRequest;

public class UpdateDriverLocationCommandHandler(
    IDriverProfileRepository driverProfileRepository,
    IRideRealtimeNotifier realtimeNotifier,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateDriverLocationCommand>
{
    public async Task Handle(UpdateDriverLocationCommand request, CancellationToken cancellationToken)
    {
        var driverProfile = await driverProfileRepository.GetByApplicationUserIdAsync(request.DriverApplicationUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(DriverProfile), request.DriverApplicationUserId);

        driverProfile.UpdateLocation(new GeoPoint(request.Latitude, request.Longitude));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.ActiveRideId is not null)
            await realtimeNotifier.NotifyDriverLocationAsync(request.ActiveRideId.Value, request.Latitude, request.Longitude, cancellationToken);
    }
}
