namespace RideShare.Application.Common.Models;

/// <summary>A knowledge-base snippet retrieved by semantic (vector) search, with its similarity distance (lower = closer match).</summary>
public sealed record KnowledgeArticleMatch(Guid Id, string Title, string Content, string Category, double Distance);
