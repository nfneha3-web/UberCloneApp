using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RideShare.Application.Common.Interfaces;
using RideShare.Infrastructure.Persistence;
using RideShare.Infrastructure.Persistence.Dapper;
using RideShare.Infrastructure.Persistence.Repositories;
using RideShare.Infrastructure.Realtime;
using RideShare.Infrastructure.Services;
using RideShare.Infrastructure.Services.Copilot;
using RideShare.Infrastructure.Services.Embeddings;
using RideShare.Infrastructure.Services.Payments;

namespace RideShare.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<RideShareDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_RideShare")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IRideRepository, RideRepository>();
        services.AddScoped<IRiderProfileRepository, RiderProfileRepository>();
        services.AddScoped<IDriverProfileRepository, DriverProfileRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<ICopilotConversationRepository, CopilotConversationRepository>();

        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IRideQueryService, RideQueryService>();
        services.AddScoped<IDriverSearchQueryService, DriverSearchQueryService>();

        services.AddSingleton<IFareCalculator, FareCalculator>();

        services.Configure<StripeSettings>(configuration.GetSection(StripeSettings.SectionName));
        services.AddScoped<IPaymentService, StripePaymentService>();

        services.AddScoped<IRideRealtimeNotifier, RideRealtimeNotifier>();

        services.AddScoped<IKnowledgeBaseRepository, KnowledgeBaseRepository>();
        services.AddScoped<IKnowledgeBaseSeeder, KnowledgeBaseSeeder>();

        AddCopilotProvider(services, configuration);

        return services;
    }

    private static void AddCopilotProvider(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CopilotSettings>(configuration.GetSection(CopilotSettings.SectionName));
        var copilotSettings = configuration.GetSection(CopilotSettings.SectionName).Get<CopilotSettings>() ?? new CopilotSettings();

        services.AddHttpClient(nameof(GitHubModelsCopilotService), client =>
        {
            client.BaseAddress = new Uri(copilotSettings.GitHubModels.BaseUrl);
            if (!string.IsNullOrWhiteSpace(copilotSettings.GitHubModels.ApiKey))
                client.DefaultRequestHeaders.Authorization = new("Bearer", copilotSettings.GitHubModels.ApiKey);
        });

        services.AddHttpClient(nameof(OllamaCopilotService), client =>
        {
            client.BaseAddress = new Uri(copilotSettings.Ollama.BaseUrl);
        });

        // Same base addresses/auth as the chat clients above, reused for the "/embeddings" endpoint.
        services.AddHttpClient(nameof(GitHubModelsEmbeddingService), client =>
        {
            client.BaseAddress = new Uri(copilotSettings.GitHubModels.BaseUrl);
            if (!string.IsNullOrWhiteSpace(copilotSettings.GitHubModels.ApiKey))
                client.DefaultRequestHeaders.Authorization = new("Bearer", copilotSettings.GitHubModels.ApiKey);
        });

        services.AddHttpClient(nameof(OllamaEmbeddingService), client =>
        {
            client.BaseAddress = new Uri(copilotSettings.Ollama.BaseUrl);
        });

        switch (copilotSettings.Provider)
        {
            case "GitHubModels":
                services.AddScoped<IAiCopilotService, GitHubModelsCopilotService>();
                services.AddScoped<IEmbeddingService, GitHubModelsEmbeddingService>();
                break;
            case "Ollama":
                services.AddScoped<IAiCopilotService, OllamaCopilotService>();
                services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();
                break;
            default:
                services.AddScoped<IAiCopilotService, AzureOpenAiCopilotService>();
                services.AddScoped<IEmbeddingService, AzureOpenAiEmbeddingService>();
                break;
        }
    }
}
