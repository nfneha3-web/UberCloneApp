using Microsoft.EntityFrameworkCore;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Repositories;

public class CopilotConversationRepository(RideShareDbContext dbContext) : ICopilotConversationRepository
{
    public Task<CopilotConversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.CopilotConversations
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<CopilotConversation?> GetMostRecentForUserAsync(Guid applicationUserId, CancellationToken cancellationToken = default) =>
        dbContext.CopilotConversations
            .Include(x => x.Messages)
            .Where(x => x.ApplicationUserId == applicationUserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

    public void Add(CopilotConversation conversation) => dbContext.CopilotConversations.Add(conversation);

    public void AddMessage(CopilotMessage message) => dbContext.CopilotMessages.Add(message);
}
