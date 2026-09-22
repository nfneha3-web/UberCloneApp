using MediatR;
using RideShare.Application.Common.Models;

namespace RideShare.Application.Rides.Commands.CompleteRide;

public sealed record CompleteRideResult(RideDetailsDto Ride, string PaymentIntentId, string PaymentClientSecret);

public sealed record CompleteRideCommand(Guid RideId, Guid DriverApplicationUserId) : IRequest<CompleteRideResult>;
