using Microsoft.EntityFrameworkCore;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Repositories;

public class RatingRepository(RideShareDbContext dbContext) : IRatingRepository
{
    public Task<bool> ExistsAsync(Guid rideId, Guid raterProfileId, CancellationToken cancellationToken = default) =>
        dbContext.Ratings.AnyAsync(x => x.RideId == rideId && x.RaterProfileId == raterProfileId, cancellationToken);

    public void Add(Rating rating) => dbContext.Ratings.Add(rating);
}
