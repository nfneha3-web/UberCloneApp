using System.Text.Json;
using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;
using RideShare.Application.Drivers.Commands.GoOffline;
using RideShare.Application.Drivers.Commands.GoOnline;
using RideShare.Application.Rides.Commands.CancelRide;
using RideShare.Application.Rides.Commands.RequestRide;
using RideShare.Application.Rides.Queries.GetActiveRide;
using RideShare.Domain.Entities;
using RideShare.Domain.Enums;
using RideShare.Domain.Exceptions;

namespace RideShare.Application.Copilot.Commands.SendMessage;

public class SendCopilotMessageCommandHandler(
    ICopilotConversationRepository conversationRepository,
    IAiCopilotService aiCopilotService,
    IEmbeddingService embeddingService,
    IKnowledgeBaseRepository knowledgeBaseRepository,
    IMediator mediator,
    IUnitOfWork unitOfWork) : IRequestHandler<SendCopilotMessageCommand, CopilotReplyDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private const int MaxHistoryMessages = 12;
    private const int MaxRetrievedArticles = 3;

    /// <summary>Cosine distance cutoff (0 = identical, 2 = opposite) — anything worse than this is noise, not context.</summary>
    private const double MaxRelevantDistance = 0.7;

    public async Task<CopilotReplyDto> Handle(SendCopilotMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await ResolveConversationAsync(request, cancellationToken);
        conversationRepository.AddMessage(conversation.AddMessage(CopilotMessageRole.User, request.Message));

        var retrieved = await RetrieveRelevantArticlesAsync(request.Message, cancellationToken);

        var chatHistory = BuildChatHistory(conversation, request.Role, retrieved);
        var tools = CopilotToolCatalog.ForRole(request.Role);

        var completion = await aiCopilotService.GetCompletionAsync(chatHistory, tools, cancellationToken);

        string reply;
        string? toolExecuted = null;
        var sources = Array.Empty<string>();

        if (!string.IsNullOrWhiteSpace(completion.ToolCallName))
        {
            (reply, toolExecuted) = await ExecuteToolAsync(completion.ToolCallName, completion.ToolCallArgumentsJson, request, cancellationToken);
        }
        else
        {
            reply = completion.TextResponse ?? "Sorry, I didn't quite catch that — could you rephrase?";
            // Only credit sources when the model actually answered from them (not when it took a tool action instead).
            sources = retrieved.Select(a => a.Title).ToArray();
        }

        conversationRepository.AddMessage(conversation.AddMessage(CopilotMessageRole.Assistant, reply, completion.ToolCallArgumentsJson));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CopilotReplyDto(conversation.Id, reply, toolExecuted, sources);
    }

    /// <summary>
    /// RAG step: embed the user's message and pull the nearest knowledge-base articles (SQL Server
    /// 2025 native VECTOR search, no separate vector DB — see docs/ROADMAP.md). Grounds answers about
    /// policy/FAQ topics in real content instead of the model improvising. Never blocks the chat —
    /// an empty knowledge base or a misconfigured embedding provider just means no extra context.
    /// </summary>
    private async Task<IReadOnlyList<KnowledgeArticleMatch>> RetrieveRelevantArticlesAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var queryEmbedding = await embeddingService.EmbedAsync(message, cancellationToken);
            var matches = await knowledgeBaseRepository.SearchByEmbeddingAsync(queryEmbedding, MaxRetrievedArticles, cancellationToken);
            return matches.Where(m => m.Distance <= MaxRelevantDistance).ToList();
        }
        catch
        {
            return [];
        }
    }

    private async Task<CopilotConversation> ResolveConversationAsync(SendCopilotMessageCommand request, CancellationToken cancellationToken)
    {
        if (request.ConversationId is not null)
        {
            var existing = await conversationRepository.GetByIdWithMessagesAsync(request.ConversationId.Value, cancellationToken);
            if (existing is not null)
                return existing;
        }

        var conversation = new CopilotConversation(request.ApplicationUserId);
        conversationRepository.Add(conversation);
        return conversation;
    }

    private static List<CopilotChatMessage> BuildChatHistory(
        CopilotConversation conversation, string role, IReadOnlyList<KnowledgeArticleMatch> retrievedArticles)
    {
        var systemPrompt = role == "Driver"
            ? "You are the RideShare copilot for a driver. You can put them online/offline and report their active ride's status. " +
              "Only call a tool when the driver clearly asked for that action. Keep replies short and friendly."
            : "You are the RideShare copilot for a rider. You can book a ride once you know both pickup and dropoff coordinates, " +
              "cancel their active ride, or report its status. Ask a clarifying question if you're missing pickup or dropoff details " +
              "before calling request_ride. Keep replies short and friendly.";

        var messages = new List<CopilotChatMessage> { new("system", systemPrompt) };

        if (retrievedArticles.Count > 0)
        {
            var context = string.Join(
                "\n\n",
                retrievedArticles.Select(a => $"### {a.Title}\n{a.Content}"));

            messages.Add(new CopilotChatMessage(
                "system",
                "Reference material from our own policy/FAQ knowledge base — use it to ground your answer when it's " +
                "relevant to the user's question, and don't contradict it. If it doesn't apply, ignore it and answer " +
                "normally or use a tool.\n\n" + context));
        }

        messages.AddRange(conversation.Messages
            .OrderBy(m => m.CreatedAtUtc)
            .TakeLast(MaxHistoryMessages)
            .Select(m => new CopilotChatMessage(m.Role == CopilotMessageRole.User ? "user" : "assistant", m.Content)));

        return messages;
    }

    private async Task<(string Reply, string? ToolExecuted)> ExecuteToolAsync(
        string toolName, string? argumentsJson, SendCopilotMessageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return toolName switch
            {
                CopilotToolCatalog.RequestRide => (await HandleRequestRideAsync(argumentsJson, request, cancellationToken), toolName),
                CopilotToolCatalog.CancelActiveRide => (await HandleCancelRideAsync(argumentsJson, request, cancellationToken), toolName),
                CopilotToolCatalog.GetActiveRideStatus => (await HandleGetActiveRideStatusAsync(request, cancellationToken), toolName),
                CopilotToolCatalog.GoOnline => (await HandleGoOnlineAsync(request, cancellationToken), toolName),
                CopilotToolCatalog.GoOffline => (await HandleGoOfflineAsync(request, cancellationToken), toolName),
                _ => ("I tried to take an action I don't actually support yet.", null)
            };
        }
        catch (Exception ex) when (ex is DomainException or ConflictException or NotFoundException or ValidationException or ForbiddenAccessException)
        {
            // Business-rule failures become a conversational reply instead of an HTTP error —
            // the copilot should say "you already have an active ride", not crash the chat.
            return (ex.Message, null);
        }
    }

    private async Task<string> HandleRequestRideAsync(string? argumentsJson, SendCopilotMessageCommand request, CancellationToken cancellationToken)
    {
        if (argumentsJson is null)
            return "I need a pickup and dropoff location before I can book a ride.";

        var args = JsonSerializer.Deserialize<RequestRideArgs>(argumentsJson, JsonOptions)
            ?? throw new ConflictException("I couldn't understand the ride details — could you try again?");

        if (!Enum.TryParse<VehicleType>(args.VehicleType, true, out var vehicleType))
            vehicleType = VehicleType.Economy;

        var command = new RequestRideCommand(
            request.ApplicationUserId,
            args.PickupLatitude, args.PickupLongitude, args.PickupAddress,
            args.DropoffLatitude, args.DropoffLongitude, args.DropoffAddress,
            vehicleType);

        var ride = await mediator.Send(command, cancellationToken);

        return $"Done! Your {vehicleType} ride is requested — estimated fare {ride.EstimatedFareAmount:0.00} {ride.Currency} " +
               $"over {ride.EstimatedDistanceKm:0.0} km. I'll let you know as soon as a driver accepts.";
    }

    private async Task<string> HandleCancelRideAsync(string? argumentsJson, SendCopilotMessageCommand request, CancellationToken cancellationToken)
    {
        var activeRide = await mediator.Send(new GetActiveRideQuery(request.ApplicationUserId, request.Role == "Driver"), cancellationToken);
        if (activeRide is null)
            return "You don't have an active ride to cancel right now.";

        var reason = "Cancelled via copilot";
        if (argumentsJson is not null)
        {
            var args = JsonSerializer.Deserialize<CancelRideArgs>(argumentsJson, JsonOptions);
            if (!string.IsNullOrWhiteSpace(args?.Reason))
                reason = args.Reason;
        }

        await mediator.Send(new CancelRideCommand(activeRide.Id, request.ApplicationUserId, reason), cancellationToken);
        return "Your ride has been cancelled.";
    }

    private async Task<string> HandleGetActiveRideStatusAsync(SendCopilotMessageCommand request, CancellationToken cancellationToken)
    {
        var activeRide = await mediator.Send(new GetActiveRideQuery(request.ApplicationUserId, request.Role == "Driver"), cancellationToken);

        if (activeRide is null)
            return "You don't have an active ride right now.";

        return activeRide.Status switch
        {
            "Requested" => "Your ride is requested — we're still looking for a nearby driver.",
            "DriverAssigned" or "DriverArriving" => $"{activeRide.DriverName} is on the way in a {activeRide.VehicleDescription}.",
            "InProgress" => "Your ride is in progress.",
            _ => $"Your ride status is: {activeRide.Status}."
        };
    }

    private async Task<string> HandleGoOnlineAsync(SendCopilotMessageCommand request, CancellationToken cancellationToken)
    {
        await mediator.Send(new GoOnlineCommand(request.ApplicationUserId), cancellationToken);
        return "You're online and ready to receive ride offers.";
    }

    private async Task<string> HandleGoOfflineAsync(SendCopilotMessageCommand request, CancellationToken cancellationToken)
    {
        await mediator.Send(new GoOfflineCommand(request.ApplicationUserId), cancellationToken);
        return "You're offline now — no more ride offers until you go online again.";
    }

    private sealed record RequestRideArgs(
        double PickupLatitude, double PickupLongitude, string? PickupAddress,
        double DropoffLatitude, double DropoffLongitude, string? DropoffAddress,
        string VehicleType);

    private sealed record CancelRideArgs(string? Reason);
}
