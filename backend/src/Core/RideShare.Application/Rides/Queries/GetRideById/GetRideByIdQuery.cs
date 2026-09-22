using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;
using RideShare.Domain.Entities;

namespace RideShare.Application.Rides.Queries.GetRideById;

public sealed record GetRideByIdQuery(Guid RideId) : IRequest<RideDetailsDto>;

public class GetRideByIdQueryHandler(IRideQueryService rideQueryService) : IRequestHandler<GetRideByIdQuery, RideDetailsDto>
{
    public async Task<RideDetailsDto> Handle(GetRideByIdQuery request, CancellationToken cancellationToken)
    {
        return await rideQueryService.GetByIdAsync(request.RideId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ride), request.RideId);
    }
}
