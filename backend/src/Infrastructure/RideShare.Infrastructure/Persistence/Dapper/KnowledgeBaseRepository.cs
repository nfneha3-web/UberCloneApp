using System.Globalization;
using global::Dapper;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;

namespace RideShare.Infrastructure.Persistence.Dapper;

/// <summary>
/// RAG storage and search, entirely via SQL Server 2025's native VECTOR type + VECTOR_DISTANCE —
/// no separate vector database, keeping this inside the project's "free services only" rule (see
/// docs/ROADMAP.md). Deliberately Dapper-only rather than an EF Core DbSet: the VECTOR type's EF
/// Core mapping support is brand new and not something this project depends on for correctness —
/// hand-written SQL is the reliable path here, same reasoning as the rest of the CQRS read side.
/// </summary>
public class KnowledgeBaseRepository(ISqlConnectionFactory connectionFactory) : IKnowledgeBaseRepository
{
    public async Task<IReadOnlyList<KnowledgeArticleMatch>> SearchByEmbeddingAsync(
        float[] queryEmbedding, int topK, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP (@TopK)
                Id, Title, Content, Category,
                VECTOR_DISTANCE('cosine', Embedding, CAST(@QueryVector AS VECTOR(1536))) AS Distance
            FROM KnowledgeArticles
            ORDER BY Distance ASC
            """;

        using var connection = connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<KnowledgeArticleMatch>(sql, new { TopK = topK, QueryVector = ToVectorLiteral(queryEmbedding) });
        return results.ToList();
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM KnowledgeArticles");
    }

    public async Task AddAsync(Guid id, string title, string content, string category, float[] embedding, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO KnowledgeArticles (Id, Title, Content, Category, Embedding, CreatedAtUtc)
            VALUES (@Id, @Title, @Content, @Category, CAST(@Embedding AS VECTOR(1536)), SYSUTCDATETIME())
            """;

        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, Title = title, Content = content, Category = category, Embedding = ToVectorLiteral(embedding) });
    }

    /// <summary>SQL Server accepts a JSON array string as an implicit/explicit source for VECTOR(n) via CAST.</summary>
    private static string ToVectorLiteral(float[] vector) =>
        "[" + string.Join(",", vector.Select(v => v.ToString("R", CultureInfo.InvariantCulture))) + "]";
}
