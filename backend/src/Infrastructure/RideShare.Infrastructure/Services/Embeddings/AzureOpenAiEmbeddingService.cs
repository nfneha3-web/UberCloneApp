using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;
using RideShare.Application.Common.Interfaces;
using RideShare.Infrastructure.Services.Copilot;

namespace RideShare.Infrastructure.Services.Embeddings;

public class AzureOpenAiEmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _embeddingClient;

    public AzureOpenAiEmbeddingService(IOptions<CopilotSettings> settings)
    {
        var options = settings.Value.AzureOpenAI;
        var azureClient = new AzureOpenAIClient(new Uri(options.Endpoint), new AzureKeyCredential(options.ApiKey));
        _embeddingClient = azureClient.GetEmbeddingClient(options.EmbeddingDeploymentName);
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        var result = await _embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return result.Value.ToFloats().ToArray();
    }
}
