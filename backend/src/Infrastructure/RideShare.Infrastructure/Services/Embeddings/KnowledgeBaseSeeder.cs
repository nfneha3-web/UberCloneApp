using Microsoft.Extensions.Logging;
using RideShare.Application.Common.Interfaces;

namespace RideShare.Infrastructure.Services.Embeddings;

public class KnowledgeBaseSeeder(
    IKnowledgeBaseRepository repository,
    IEmbeddingService embeddingService,
    ILogger<KnowledgeBaseSeeder> logger) : IKnowledgeBaseSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await repository.CountAsync(cancellationToken) > 0)
            return;

        var seeded = 0;
        foreach (var (title, category, content) in KnowledgeBaseSeedData.Articles)
        {
            var embedding = await embeddingService.EmbedAsync($"{title}\n\n{content}", cancellationToken);
            await repository.AddAsync(Guid.NewGuid(), title, content, category, embedding, cancellationToken);
            seeded++;
        }

        logger.LogInformation("Seeded {Count} knowledge base articles for the copilot's RAG search.", seeded);
    }
}
