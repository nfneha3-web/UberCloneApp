using MediatR;
using RideShare.Application.Common.Models;

namespace RideShare.Application.Rides.Commands.StartRide;

public sealed record StartRideCommand(Guid RideId, Guid DriverApplicationUserId) : IRequest<RideDetailsDto>;
