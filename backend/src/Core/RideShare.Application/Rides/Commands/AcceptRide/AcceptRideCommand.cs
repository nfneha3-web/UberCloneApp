using MediatR;
using RideShare.Application.Common.Models;

namespace RideShare.Application.Rides.Commands.AcceptRide;

public sealed record AcceptRideCommand(Guid RideId, Guid DriverApplicationUserId) : IRequest<RideDetailsDto>;
