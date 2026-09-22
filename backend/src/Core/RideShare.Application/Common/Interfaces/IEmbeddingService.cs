namespace RideShare.Application.Common.Interfaces;

/// <summary>
/// Turns text into an embedding vector for semantic search. Mirrors IAiCopilotService's
/// provider-agnostic design — Azure OpenAI / GitHub Models / Ollama implementations are
/// selected by the same "Copilot:Provider" setting, so RAG stays free under the same rules
/// as the chat copilot (see docs/ROADMAP.md).
/// </summary>
public interface IEmbeddingService
{
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);
}
