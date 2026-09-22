using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideShare.Api.Contracts;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;
using RideShare.Application.Copilot.Commands.SendMessage;
using RideShare.Application.Copilot.Queries.GetConversation;

namespace RideShare.Api.Controllers;

[Authorize]
[Route("api/copilot")]
public class CopilotController(IMediator mediator, ICurrentUserService currentUserService) : ApiControllerBase(currentUserService)
{
    [HttpPost("messages")]
    public async Task<ActionResult<CopilotReplyDto>> SendMessage(SendCopilotMessageRequest request, CancellationToken cancellationToken)
    {
        var role = currentUserService.IsInRole("Driver") ? "Driver" : "Rider";
        var command = new SendCopilotMessageCommand(CurrentUserId, role, request.ConversationId, request.Message);
        return Ok(await mediator.Send(command, cancellationToken));
    }

    [HttpGet("conversations/{conversationId:guid?}")]
    public async Task<ActionResult<IReadOnlyList<CopilotMessageDto>>> GetConversation(Guid? conversationId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetCopilotConversationQuery(CurrentUserId, conversationId), cancellationToken));
}
