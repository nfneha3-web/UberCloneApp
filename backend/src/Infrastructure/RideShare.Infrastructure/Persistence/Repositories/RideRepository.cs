using Microsoft.EntityFrameworkCore;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;
using RideShare.Domain.Enums;

namespace RideShare.Infrastructure.Persistence.Repositories;

public class RideRepository(RideShareDbContext dbContext) : IRideRepository
{
    public Task<Ride?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Rides.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Ride?> GetActiveRideForRiderAsync(Guid riderProfileId, CancellationToken cancellationToken = default) =>
        dbContext.Rides.FirstOrDefaultAsync(x =>
            x.RiderProfileId == riderProfileId && ActiveStatuses.Contains(x.Status), cancellationToken);

    public Task<Ride?> GetActiveRideForDriverAsync(Guid driverProfileId, CancellationToken cancellationToken = default) =>
        dbContext.Rides.FirstOrDefaultAsync(x =>
            x.DriverProfileId == driverProfileId && ActiveStatuses.Contains(x.Status), cancellationToken);

    public void Add(Ride ride) => dbContext.Rides.Add(ride);

    private static readonly RideStatus[] ActiveStatuses =
    [
        RideStatus.Requested, RideStatus.DriverAssigned, RideStatus.DriverArriving, RideStatus.InProgress
    ];
}
