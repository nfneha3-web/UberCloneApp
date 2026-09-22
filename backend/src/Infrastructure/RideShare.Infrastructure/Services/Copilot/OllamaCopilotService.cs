using Microsoft.Extensions.Options;
using RideShare.Application.Common.Interfaces;

namespace RideShare.Infrastructure.Services.Copilot;

/// <summary>
/// Fully local, fully offline copilot provider — talks to a locally running Ollama server
/// (ollama.com, e.g. "ollama run llama3.1"). Zero cost forever, no API key, no rate limit,
/// no internet dependency once the model is pulled. Configure via "Copilot:Ollama" and set
/// "Copilot:Provider" to "Ollama" for a guaranteed-$0, no-account-needed setup.
/// </summary>
public class OllamaCopilotService : IAiCopilotService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaCopilotService(IHttpClientFactory httpClientFactory, IOptions<CopilotSettings> settings)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(OllamaCopilotService));
        _model = settings.Value.Ollama.Model;
    }

    public Task<CopilotCompletionResult> GetCompletionAsync(
        IReadOnlyList<CopilotChatMessage> messages,
        IReadOnlyList<CopilotToolDefinition> availableTools,
        CancellationToken cancellationToken = default) =>
        OpenAiCompatibleChat.CompleteAsync(_httpClient, _model, messages, availableTools, cancellationToken);
}
