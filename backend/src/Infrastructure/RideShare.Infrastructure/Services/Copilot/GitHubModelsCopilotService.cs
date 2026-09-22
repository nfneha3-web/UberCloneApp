using Microsoft.Extensions.Options;
using RideShare.Application.Common.Interfaces;

namespace RideShare.Infrastructure.Services.Copilot;

/// <summary>
/// Free-tier fallback copilot provider — GitHub Models (github.com/marketplace/models) exposes
/// an OpenAI-compatible endpoint with a genuinely free, indefinite rate-limited tier for a
/// personal access token, no billing ever kicks in. Configure via "Copilot:GitHubModels" and set
/// "Copilot:Provider" to "GitHubModels" once the Azure OpenAI trial credit runs out.
/// </summary>
public class GitHubModelsCopilotService : IAiCopilotService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public GitHubModelsCopilotService(IHttpClientFactory httpClientFactory, IOptions<CopilotSettings> settings)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(GitHubModelsCopilotService));
        _model = settings.Value.GitHubModels.Model;
    }

    public Task<CopilotCompletionResult> GetCompletionAsync(
        IReadOnlyList<CopilotChatMessage> messages,
        IReadOnlyList<CopilotToolDefinition> availableTools,
        CancellationToken cancellationToken = default) =>
        OpenAiCompatibleChat.CompleteAsync(_httpClient, _model, messages, availableTools, cancellationToken);
}
