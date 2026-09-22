using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using RideShare.Application.Common.Interfaces;

namespace RideShare.Infrastructure.Services.Copilot;

/// <summary>
/// Shared HTTP client for any provider that speaks the OpenAI-compatible "/chat/completions" API —
/// which covers both GitHub Models (free tier) and a local Ollama server (0.1.26+, fully offline).
/// Keeping this logic in one place means adding a third free-tier provider is a ~5-line subclass.
/// </summary>
internal static class OpenAiCompatibleChat
{
    public static async Task<CopilotCompletionResult> CompleteAsync(
        HttpClient httpClient,
        string model,
        IReadOnlyList<CopilotChatMessage> messages,
        IReadOnlyList<CopilotToolDefinition> tools,
        CancellationToken cancellationToken)
    {
        var requestBody = new JsonObject
        {
            ["model"] = model,
            ["messages"] = new JsonArray(messages.Select(m => (JsonNode)new JsonObject
            {
                ["role"] = m.Role,
                ["content"] = m.Content
            }).ToArray())
        };

        if (tools.Count > 0)
        {
            requestBody["tools"] = new JsonArray(tools.Select(t => (JsonNode)new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject
                {
                    ["name"] = t.Name,
                    ["description"] = t.Description,
                    ["parameters"] = JsonNode.Parse(t.JsonSchema)
                }
            }).ToArray());
            requestBody["tool_choice"] = "auto";
        }

        using var response = await httpClient.PostAsJsonAsync("chat/completions", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
        var message = payload.GetProperty("choices")[0].GetProperty("message");

        if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.GetArrayLength() > 0)
        {
            var function = toolCalls[0].GetProperty("function");
            var name = function.GetProperty("name").GetString();
            var arguments = function.GetProperty("arguments").GetString();
            return new CopilotCompletionResult(null, name, arguments);
        }

        var content = message.TryGetProperty("content", out var contentProp) ? contentProp.GetString() : null;
        return new CopilotCompletionResult(content, null, null);
    }
}
