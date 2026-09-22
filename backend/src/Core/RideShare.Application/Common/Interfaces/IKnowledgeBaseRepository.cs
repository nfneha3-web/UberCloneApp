using RideShare.Application.Common.Models;

namespace RideShare.Application.Common.Interfaces;

/// <summary>
/// Backed by SQL Server 2025's native VECTOR type + VECTOR_DISTANCE (see
/// RideShare.Infrastructure.Persistence.Dapper.KnowledgeBaseRepository) — no separate vector
/// database service, so this stays inside the project's "free services only" constraint.
/// </summary>
public interface IKnowledgeBaseRepository
{
    Task<IReadOnlyList<KnowledgeArticleMatch>> SearchByEmbeddingAsync(float[] queryEmbedding, int topK, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Guid id, string title, string content, string category, float[] embedding, CancellationToken cancellationToken = default);
}
