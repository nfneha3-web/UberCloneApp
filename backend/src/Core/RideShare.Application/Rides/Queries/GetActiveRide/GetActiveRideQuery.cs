using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;

namespace RideShare.Application.Rides.Queries.GetActiveRide;

public sealed record GetActiveRideQuery(Guid ApplicationUserId, bool AsDriver) : IRequest<RideDetailsDto?>;

public class GetActiveRideQueryHandler(
    IRideQueryService rideQueryService,
    IRiderProfileRepository riderProfileRepository,
    IDriverProfileRepository driverProfileRepository) : IRequestHandler<GetActiveRideQuery, RideDetailsDto?>
{
    public async Task<RideDetailsDto?> Handle(GetActiveRideQuery request, CancellationToken cancellationToken)
    {
        if (request.AsDriver)
        {
            var driverProfile = await driverProfileRepository.GetByApplicationUserIdAsync(request.ApplicationUserId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Entities.DriverProfile), request.ApplicationUserId);

            return await rideQueryService.GetActiveRideForDriverAsync(driverProfile.Id, cancellationToken);
        }

        var riderProfile = await riderProfileRepository.GetByApplicationUserIdAsync(request.ApplicationUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.RiderProfile), request.ApplicationUserId);

        return await rideQueryService.GetActiveRideForRiderAsync(riderProfile.Id, cancellationToken);
    }
}
