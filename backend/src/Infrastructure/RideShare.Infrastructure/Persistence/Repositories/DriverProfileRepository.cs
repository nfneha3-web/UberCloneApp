using Microsoft.EntityFrameworkCore;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Repositories;

public class DriverProfileRepository(RideShareDbContext dbContext) : IDriverProfileRepository
{
    public Task<DriverProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.DriverProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<DriverProfile?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default) =>
        dbContext.DriverProfiles.FirstOrDefaultAsync(x => x.ApplicationUserId == applicationUserId, cancellationToken);

    public void Add(DriverProfile profile) => dbContext.DriverProfiles.Add(profile);
}
