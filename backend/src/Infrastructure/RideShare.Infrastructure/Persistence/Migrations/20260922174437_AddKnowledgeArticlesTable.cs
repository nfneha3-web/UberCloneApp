using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideShare.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Hand-written raw-SQL migration (not EF-model-tracked — see KnowledgeBaseRepository) that
    /// creates the copilot's RAG storage using SQL Server 2025's native VECTOR type. Dimension is
    /// fixed at 1536 to match OpenAI-family embedding models (Azure OpenAI / GitHub Models
    /// text-embedding-3-small); see docs/ROADMAP.md if you're using a different-sized Ollama model.
    /// </summary>
    public partial class AddKnowledgeArticlesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DDL that touches database-scoped configuration can't run inside EF's migration transaction.
            migrationBuilder.Sql("ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES = ON;", suppressTransaction: true);

            migrationBuilder.Sql("""
                CREATE TABLE KnowledgeArticles (
                    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    Title NVARCHAR(200) NOT NULL,
                    Content NVARCHAR(MAX) NOT NULL,
                    Category NVARCHAR(60) NOT NULL,
                    Embedding VECTOR(1536) NOT NULL,
                    CreatedAtUtc DATETIME2 NOT NULL
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS KnowledgeArticles;");
        }
    }
}
