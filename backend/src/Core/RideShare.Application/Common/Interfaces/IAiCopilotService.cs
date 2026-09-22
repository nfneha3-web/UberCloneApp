namespace RideShare.Application.Common.Interfaces;

public sealed record CopilotChatMessage(string Role, string Content);

public sealed record CopilotToolDefinition(string Name, string Description, string JsonSchema);

public sealed record CopilotCompletionResult(
    string? TextResponse,
    string? ToolCallName,
    string? ToolCallArgumentsJson);

/// <summary>
/// Provider-agnostic contract for the copilot's underlying language model.
/// Infrastructure ships three implementations selectable via appsettings ("Copilot:Provider"),
/// all free to run: Azure OpenAI (30-day trial credit), GitHub Models (free tier, no time limit),
/// and a local Ollama model (fully offline, zero cost ever). Swapping providers never touches
/// this interface or any Application-layer code.
/// </summary>
public interface IAiCopilotService
{
    Task<CopilotCompletionResult> GetCompletionAsync(
        IReadOnlyList<CopilotChatMessage> messages,
        IReadOnlyList<CopilotToolDefinition> availableTools,
        CancellationToken cancellationToken = default);
}
