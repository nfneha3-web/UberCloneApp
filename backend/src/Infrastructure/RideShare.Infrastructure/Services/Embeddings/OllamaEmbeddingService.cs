using Microsoft.Extensions.Options;
using RideShare.Application.Common.Interfaces;
using RideShare.Infrastructure.Services.Copilot;

namespace RideShare.Infrastructure.Services.Embeddings;

/// <summary>
/// Local, $0-forever embeddings via Ollama's OpenAI-compatible endpoint. IMPORTANT: most local
/// embedding models (e.g. nomic-embed-text, 768 dims) do NOT match the KnowledgeArticles table's
/// VECTOR(1536) column sized for OpenAI-family models — pick a 1536-dim Ollama model, or resize
/// the column in a new migration, before relying on this provider for RAG. See docs/ROADMAP.md.
/// </summary>
public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaEmbeddingService(IHttpClientFactory httpClientFactory, IOptions<CopilotSettings> settings)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(OllamaEmbeddingService));
        _model = settings.Value.Ollama.EmbeddingModel;
    }

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default) =>
        OpenAiCompatibleEmbeddings.EmbedAsync(_httpClient, _model, text, cancellationToken);
}
