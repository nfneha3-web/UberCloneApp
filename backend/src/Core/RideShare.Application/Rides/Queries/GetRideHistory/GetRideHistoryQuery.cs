using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;

namespace RideShare.Application.Rides.Queries.GetRideHistory;

public sealed record GetRideHistoryQuery(Guid ApplicationUserId, bool AsDriver) : IRequest<IReadOnlyList<RideHistoryItemDto>>;

public class GetRideHistoryQueryHandler(
    IRideQueryService rideQueryService,
    IRiderProfileRepository riderProfileRepository,
    IDriverProfileRepository driverProfileRepository) : IRequestHandler<GetRideHistoryQuery, IReadOnlyList<RideHistoryItemDto>>
{
    public async Task<IReadOnlyList<RideHistoryItemDto>> Handle(GetRideHistoryQuery request, CancellationToken cancellationToken)
    {
        if (request.AsDriver)
        {
            var driverProfile = await driverProfileRepository.GetByApplicationUserIdAsync(request.ApplicationUserId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Entities.DriverProfile), request.ApplicationUserId);

            return await rideQueryService.GetHistoryForDriverAsync(driverProfile.Id, cancellationToken);
        }

        var riderProfile = await riderProfileRepository.GetByApplicationUserIdAsync(request.ApplicationUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.RiderProfile), request.ApplicationUserId);

        return await rideQueryService.GetHistoryForRiderAsync(riderProfile.Id, cancellationToken);
    }
}
