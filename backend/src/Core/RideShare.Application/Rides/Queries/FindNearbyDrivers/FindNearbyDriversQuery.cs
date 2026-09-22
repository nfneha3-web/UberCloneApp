using MediatR;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;
using RideShare.Domain.Enums;

namespace RideShare.Application.Rides.Queries.FindNearbyDrivers;

public sealed record FindNearbyDriversQuery(double Latitude, double Longitude, double RadiusKm, VehicleType? VehicleType)
    : IRequest<IReadOnlyList<NearbyDriverDto>>;

public class FindNearbyDriversQueryHandler(IDriverSearchQueryService driverSearchQueryService)
    : IRequestHandler<FindNearbyDriversQuery, IReadOnlyList<NearbyDriverDto>>
{
    public Task<IReadOnlyList<NearbyDriverDto>> Handle(FindNearbyDriversQuery request, CancellationToken cancellationToken)
    {
        return driverSearchQueryService.FindNearbyAvailableDriversAsync(
            request.Latitude, request.Longitude, request.RadiusKm, request.VehicleType, cancellationToken);
    }
}
