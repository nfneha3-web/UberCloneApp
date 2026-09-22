using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using RideShare.Application.Common.Interfaces;

namespace RideShare.Infrastructure.Services.Copilot;

/// <summary>
/// Default copilot provider, running against the Azure OpenAI free 30-day trial credit
/// ("Copilot:AzureOpenAI" in appsettings). See docs/ROADMAP.md — swap "Copilot:Provider" to
/// "GitHubModels" or "Ollama" before that trial lapses to keep the copilot at $0 forever.
/// </summary>
public class AzureOpenAiCopilotService : IAiCopilotService
{
    private readonly ChatClient _chatClient;

    public AzureOpenAiCopilotService(IOptions<CopilotSettings> settings)
    {
        var options = settings.Value.AzureOpenAI;
        var azureClient = new AzureOpenAIClient(new Uri(options.Endpoint), new AzureKeyCredential(options.ApiKey));
        _chatClient = azureClient.GetChatClient(options.DeploymentName);
    }

    public async Task<CopilotCompletionResult> GetCompletionAsync(
        IReadOnlyList<CopilotChatMessage> messages,
        IReadOnlyList<CopilotToolDefinition> availableTools,
        CancellationToken cancellationToken = default)
    {
        var chatMessages = messages.Select(ToChatMessage).ToList();

        var chatOptions = new ChatCompletionOptions();
        foreach (var tool in availableTools)
            chatOptions.Tools.Add(ChatTool.CreateFunctionTool(tool.Name, tool.Description, BinaryData.FromString(tool.JsonSchema)));

        var completion = await _chatClient.CompleteChatAsync(chatMessages, chatOptions, cancellationToken);
        var result = completion.Value;

        if (result.FinishReason == ChatFinishReason.ToolCalls && result.ToolCalls.Count > 0)
        {
            var toolCall = result.ToolCalls[0];
            return new CopilotCompletionResult(null, toolCall.FunctionName, toolCall.FunctionArguments.ToString());
        }

        var text = result.Content.Count > 0 ? result.Content[0].Text : null;
        return new CopilotCompletionResult(text, null, null);
    }

    private static ChatMessage ToChatMessage(CopilotChatMessage message) => message.Role switch
    {
        "system" => new SystemChatMessage(message.Content),
        "assistant" => new AssistantChatMessage(message.Content),
        _ => new UserChatMessage(message.Content)
    };
}
