using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideShare.Api.Contracts;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Ratings.Commands.SubmitRating;

namespace RideShare.Api.Controllers;

[Authorize]
[Route("api/ratings")]
public class RatingsController(IMediator mediator, ICurrentUserService currentUserService) : ApiControllerBase(currentUserService)
{
    [HttpPost]
    public async Task<IActionResult> SubmitRating(SubmitRatingRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new SubmitRatingCommand(request.RideId, CurrentUserId, request.Stars, request.Comment), cancellationToken);
        return NoContent();
    }
}
