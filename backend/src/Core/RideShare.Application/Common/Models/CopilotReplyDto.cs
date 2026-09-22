namespace RideShare.Application.Common.Models;

public sealed record CopilotReplyDto(Guid ConversationId, string Reply, string? ToolExecuted, IReadOnlyList<string> Sources);
