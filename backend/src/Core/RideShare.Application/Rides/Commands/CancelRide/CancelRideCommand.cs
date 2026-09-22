using MediatR;
using RideShare.Application.Common.Models;

namespace RideShare.Application.Rides.Commands.CancelRide;

public sealed record CancelRideCommand(Guid RideId, Guid RequestingApplicationUserId, string Reason) : IRequest<RideDetailsDto>;
