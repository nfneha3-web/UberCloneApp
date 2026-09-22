namespace RideShare.Infrastructure.Services.Embeddings;

/// <summary>Startup-only concern (Program.cs calls this once) — not part of the Application layer's CQRS surface.</summary>
public interface IKnowledgeBaseSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
