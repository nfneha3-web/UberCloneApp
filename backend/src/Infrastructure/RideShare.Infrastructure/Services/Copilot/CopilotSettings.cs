namespace RideShare.Infrastructure.Services.Copilot;

public class CopilotSettings
{
    public const string SectionName = "Copilot";

    /// <summary>"AzureOpenAI" | "GitHubModels" | "Ollama" — swap providers with zero code changes.</summary>
    public string Provider { get; set; } = "AzureOpenAI";

    public AzureOpenAiOptions AzureOpenAI { get; set; } = new();
    public OpenAiCompatibleOptions GitHubModels { get; set; } = new();
    public OpenAiCompatibleOptions Ollama { get; set; } = new();
}

public class AzureOpenAiOptions
{
    public string Endpoint { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
    public string DeploymentName { get; set; } = default!;

    /// <summary>Embeddings deployment for RAG (e.g. "text-embedding-3-small", 1536 dimensions — matches the KnowledgeArticles.Embedding column).</summary>
    public string EmbeddingDeploymentName { get; set; } = default!;
}

public class OpenAiCompatibleOptions
{
    public string BaseUrl { get; set; } = default!;
    public string? ApiKey { get; set; }
    public string Model { get; set; } = default!;

    /// <summary>Embeddings model for RAG. Must produce 1536-dimensional vectors to match the KnowledgeArticles.Embedding column
    /// (true for OpenAI-compatible "text-embedding-3-small"; a local Ollama model like nomic-embed-text does NOT — see docs/ROADMAP.md).</summary>
    public string EmbeddingModel { get; set; } = default!;
}
