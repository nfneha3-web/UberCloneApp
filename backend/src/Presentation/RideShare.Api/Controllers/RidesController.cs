using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideShare.Api.Contracts;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;
using RideShare.Application.Rides.Commands.AcceptRide;
using RideShare.Application.Rides.Commands.CancelRide;
using RideShare.Application.Rides.Commands.CompleteRide;
using RideShare.Application.Rides.Commands.RequestRide;
using RideShare.Application.Rides.Commands.StartRide;
using RideShare.Application.Rides.Queries.FindNearbyDrivers;
using RideShare.Application.Rides.Queries.GetActiveRide;
using RideShare.Application.Rides.Queries.GetRideById;
using RideShare.Application.Rides.Queries.GetRideHistory;
using RideShare.Domain.Enums;

namespace RideShare.Api.Controllers;

[Authorize]
[Route("api/rides")]
public class RidesController(IMediator mediator, ICurrentUserService currentUserService) : ApiControllerBase(currentUserService)
{
    [HttpPost]
    [Authorize(Roles = "Rider")]
    public async Task<ActionResult<RideDetailsDto>> RequestRide(RequestRideRequest request, CancellationToken cancellationToken)
    {
        var command = new RequestRideCommand(
            CurrentUserId,
            request.PickupLatitude, request.PickupLongitude, request.PickupAddress,
            request.DropoffLatitude, request.DropoffLongitude, request.DropoffAddress,
            request.VehicleType);

        return Ok(await mediator.Send(command, cancellationToken));
    }

    [HttpPost("{rideId:guid}/accept")]
    [Authorize(Roles = "Driver")]
    public async Task<ActionResult<RideDetailsDto>> AcceptRide(Guid rideId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new AcceptRideCommand(rideId, CurrentUserId), cancellationToken));

    [HttpPost("{rideId:guid}/start")]
    [Authorize(Roles = "Driver")]
    public async Task<ActionResult<RideDetailsDto>> StartRide(Guid rideId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new StartRideCommand(rideId, CurrentUserId), cancellationToken));

    [HttpPost("{rideId:guid}/complete")]
    [Authorize(Roles = "Driver")]
    public async Task<ActionResult<CompleteRideResult>> CompleteRide(Guid rideId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new CompleteRideCommand(rideId, CurrentUserId), cancellationToken));

    [HttpPost("{rideId:guid}/cancel")]
    public async Task<ActionResult<RideDetailsDto>> CancelRide(Guid rideId, CancelRideRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new CancelRideCommand(rideId, CurrentUserId, request.Reason), cancellationToken));

    [HttpGet("{rideId:guid}")]
    public async Task<ActionResult<RideDetailsDto>> GetById(Guid rideId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetRideByIdQuery(rideId), cancellationToken));

    [HttpGet("active")]
    public async Task<ActionResult<RideDetailsDto?>> GetActive([FromQuery] bool asDriver, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetActiveRideQuery(CurrentUserId, asDriver), cancellationToken));

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<RideHistoryItemDto>>> GetHistory([FromQuery] bool asDriver, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetRideHistoryQuery(CurrentUserId, asDriver), cancellationToken));

    [HttpGet("nearby-drivers")]
    [Authorize(Roles = "Rider")]
    public async Task<ActionResult<IReadOnlyList<NearbyDriverDto>>> GetNearbyDrivers(
        [FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radiusKm = 8.0,
        [FromQuery] VehicleType? vehicleType = null, CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new FindNearbyDriversQuery(latitude, longitude, radiusKm, vehicleType), cancellationToken));
}
