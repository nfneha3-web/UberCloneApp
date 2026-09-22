using MediatR;
using RideShare.Application.Common.Models;

namespace RideShare.Application.Copilot.Commands.SendMessage;

public sealed record SendCopilotMessageCommand(
    Guid ApplicationUserId,
    string Role,
    Guid? ConversationId,
    string Message) : IRequest<CopilotReplyDto>;
