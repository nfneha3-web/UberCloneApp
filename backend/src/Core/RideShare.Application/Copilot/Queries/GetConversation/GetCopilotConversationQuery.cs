using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Application.Copilot.Queries.GetConversation;

public sealed record CopilotMessageDto(Guid Id, string Role, string Content, DateTime CreatedAtUtc);

public sealed record GetCopilotConversationQuery(Guid ApplicationUserId, Guid? ConversationId) : IRequest<IReadOnlyList<CopilotMessageDto>>;

public class GetCopilotConversationQueryHandler(ICopilotConversationRepository conversationRepository)
    : IRequestHandler<GetCopilotConversationQuery, IReadOnlyList<CopilotMessageDto>>
{
    public async Task<IReadOnlyList<CopilotMessageDto>> Handle(GetCopilotConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = request.ConversationId is not null
            ? await conversationRepository.GetByIdWithMessagesAsync(request.ConversationId.Value, cancellationToken)
            : await conversationRepository.GetMostRecentForUserAsync(request.ApplicationUserId, cancellationToken);

        if (conversation is null)
            return [];

        if (conversation.ApplicationUserId != request.ApplicationUserId)
            throw new ForbiddenAccessException("This conversation does not belong to you.");

        return conversation.Messages
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new CopilotMessageDto(m.Id, m.Role.ToString(), m.Content, m.CreatedAtUtc))
            .ToList();
    }
}
