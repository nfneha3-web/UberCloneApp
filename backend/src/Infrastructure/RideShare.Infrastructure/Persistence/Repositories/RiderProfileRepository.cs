using Microsoft.EntityFrameworkCore;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Repositories;

public class RiderProfileRepository(RideShareDbContext dbContext) : IRiderProfileRepository
{
    public Task<RiderProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.RiderProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<RiderProfile?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default) =>
        dbContext.RiderProfiles.FirstOrDefaultAsync(x => x.ApplicationUserId == applicationUserId, cancellationToken);

    public void Add(RiderProfile profile) => dbContext.RiderProfiles.Add(profile);
}
