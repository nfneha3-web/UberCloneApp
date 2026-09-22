using RideShare.Domain.Common;
using RideShare.Domain.Enums;
using RideShare.Domain.Exceptions;

namespace RideShare.Domain.Entities;

public class CopilotConversation : BaseEntity
{
    public Guid ApplicationUserId { get; private set; }
    public string Title { get; private set; } = "New conversation";

    private readonly List<CopilotMessage> _messages = [];
    public IReadOnlyCollection<CopilotMessage> Messages => _messages.AsReadOnly();

    private CopilotConversation() { }

    public CopilotConversation(Guid applicationUserId)
    {
        if (applicationUserId == Guid.Empty)
            throw new DomainException("A conversation must belong to a user.");

        ApplicationUserId = applicationUserId;
    }

    public CopilotMessage AddMessage(CopilotMessageRole role, string content, string? toolCallJson = null)
    {
        if (string.IsNullOrWhiteSpace(content) && toolCallJson is null)
            throw new DomainException("A message must have content or a tool call.");

        var message = new CopilotMessage(Id, role, content, toolCallJson);
        _messages.Add(message);
        Touch();

        if (_messages.Count == 1 && role == CopilotMessageRole.User)
            Title = content.Length > 60 ? content[..60] + "…" : content;

        return message;
    }
}
