using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RideShare.Infrastructure.Services.Embeddings;

/// <summary>Shared HTTP call for any OpenAI-compatible "/embeddings" endpoint (GitHub Models, Ollama's OpenAI-compatible API).</summary>
internal static class OpenAiCompatibleEmbeddings
{
    public static async Task<float[]> EmbedAsync(HttpClient httpClient, string model, string text, CancellationToken cancellationToken)
    {
        var requestBody = new JsonObject { ["model"] = model, ["input"] = text };

        using var response = await httpClient.PostAsJsonAsync("embeddings", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
        var embeddingElement = payload.GetProperty("data")[0].GetProperty("embedding");

        var embedding = new float[embeddingElement.GetArrayLength()];
        var i = 0;
        foreach (var value in embeddingElement.EnumerateArray())
            embedding[i++] = value.GetSingle();

        return embedding;
    }
}
