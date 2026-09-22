using MediatR;
using RideShare.Application.Common.Models;
using RideShare.Domain.Enums;

namespace RideShare.Application.Rides.Commands.RequestRide;

public sealed record RequestRideCommand(
    Guid RiderApplicationUserId,
    double PickupLatitude,
    double PickupLongitude,
    string? PickupAddress,
    double DropoffLatitude,
    double DropoffLongitude,
    string? DropoffAddress,
    VehicleType VehicleType) : IRequest<RideDetailsDto>;
