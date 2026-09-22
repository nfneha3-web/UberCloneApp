using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideShare.Application.Payments.Commands.ConfirmPayment;
using RideShare.Application.Payments.Queries.GetPaymentForRide;

namespace RideShare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/payments")]
public class PaymentsController(IMediator mediator) : ControllerBase
{
    [HttpPost("confirm")]
    public async Task<ActionResult<bool>> Confirm([FromBody] ConfirmPaymentCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [HttpGet("ride/{rideId:guid}")]
    public async Task<ActionResult<PaymentInfoDto>> GetForRide(Guid rideId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetPaymentForRideQuery(rideId), cancellationToken));
}
