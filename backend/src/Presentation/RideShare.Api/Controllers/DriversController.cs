using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideShare.Api.Contracts;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Drivers.Commands.GoOffline;
using RideShare.Application.Drivers.Commands.GoOnline;
using RideShare.Application.Drivers.Commands.RegisterVehicle;
using RideShare.Application.Drivers.Commands.UpdateLocation;

namespace RideShare.Api.Controllers;

[Authorize(Roles = "Driver")]
[Route("api/drivers")]
public class DriversController(IMediator mediator, ICurrentUserService currentUserService) : ApiControllerBase(currentUserService)
{
    [HttpPost("vehicles")]
    public async Task<ActionResult<Guid>> RegisterVehicle(RegisterVehicleRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterVehicleCommand(CurrentUserId, request.Make, request.Model, request.Year, request.Color, request.PlateNumber, request.VehicleType);
        return Ok(await mediator.Send(command, cancellationToken));
    }

    [HttpPost("online")]
    public async Task<IActionResult> GoOnline(CancellationToken cancellationToken)
    {
        await mediator.Send(new GoOnlineCommand(CurrentUserId), cancellationToken);
        return NoContent();
    }

    [HttpPost("offline")]
    public async Task<IActionResult> GoOffline(CancellationToken cancellationToken)
    {
        await mediator.Send(new GoOfflineCommand(CurrentUserId), cancellationToken);
        return NoContent();
    }

    [HttpPost("location")]
    public async Task<IActionResult> UpdateLocation(UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateDriverLocationCommand(CurrentUserId, request.Latitude, request.Longitude, request.ActiveRideId), cancellationToken);
        return NoContent();
    }
}
