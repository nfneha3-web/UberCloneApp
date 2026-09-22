using Microsoft.Extensions.Options;
using RideShare.Application.Common.Interfaces;
using RideShare.Infrastructure.Services.Copilot;

namespace RideShare.Infrastructure.Services.Embeddings;

public class GitHubModelsEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public GitHubModelsEmbeddingService(IHttpClientFactory httpClientFactory, IOptions<CopilotSettings> settings)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(GitHubModelsEmbeddingService));
        _model = settings.Value.GitHubModels.EmbeddingModel;
    }

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default) =>
        OpenAiCompatibleEmbeddings.EmbedAsync(_httpClient, _model, text, cancellationToken);
}
