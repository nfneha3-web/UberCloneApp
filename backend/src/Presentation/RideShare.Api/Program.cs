using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using RideShare.Api.Middleware;
using RideShare.Api.Services;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.DependencyInjection;
using RideShare.Identity.DependencyInjection;
using RideShare.Infrastructure.DependencyInjection;
using RideShare.Infrastructure.Realtime;
using RideShare.Infrastructure.Services.Embeddings;

var builder = WebApplication.CreateBuilder(args);

const string AngularCorsPolicy = "AngularClient";

// ---- Dependency injection (composition root) ----
// Each layer's own AddXxx() extension keeps this file thin and keeps the wiring next to the code it wires.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddSignalR();

// "Cors:AllowedOrigins" is a comma-separated list — localhost for local dev, plus the deployed
// Static Web App's origin once it exists (set via App Service Configuration, no redeploy needed).
var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:4200")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "RideShare API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste a JWT access token (no \"Bearer \" prefix needed)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---- HTTP pipeline ----
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

app.UseCors(AngularCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<RideHub>("/hubs/ride");

// Best-effort: seeds the copilot's RAG knowledge base on first run. Never blocks startup —
// a misconfigured/unreachable embedding provider just means the copilot has no retrieved
// context yet, not a crashed API (see IKnowledgeBaseSeeder).
using (var scope = app.Services.CreateScope())
{
    try
    {
        await scope.ServiceProvider.GetRequiredService<IKnowledgeBaseSeeder>().SeedAsync();
    }
    catch (Exception ex)
    {
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>()
            .LogWarning(ex, "Could not seed the copilot knowledge base — check Copilot embedding settings.");
    }
}

app.Run();

public partial class Program;
