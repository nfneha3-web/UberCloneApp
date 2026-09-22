using RideShare.Domain.Common;
using RideShare.Domain.Enums;

namespace RideShare.Domain.Entities;

public class CopilotMessage : BaseEntity
{
    public Guid CopilotConversationId { get; private set; }
    public CopilotMessageRole Role { get; private set; }
    public string Content { get; private set; } = default!;

    /// <summary>Serialized JSON describing a function/tool call the copilot made (command name + args), if any.</summary>
    public string? ToolCallJson { get; private set; }

    private CopilotMessage() { }

    public CopilotMessage(Guid copilotConversationId, CopilotMessageRole role, string content, string? toolCallJson)
    {
        CopilotConversationId = copilotConversationId;
        Role = role;
        Content = content;
        ToolCallJson = toolCallJson;
    }
}
