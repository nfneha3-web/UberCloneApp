using Microsoft.EntityFrameworkCore;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Repositories;

public class VehicleRepository(RideShareDbContext dbContext) : IVehicleRepository
{
    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Vehicles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Vehicle?> GetActiveVehicleForDriverAsync(Guid driverProfileId, CancellationToken cancellationToken = default) =>
        dbContext.Vehicles.FirstOrDefaultAsync(x => x.DriverProfileId == driverProfileId && x.IsActive, cancellationToken);

    public void Add(Vehicle vehicle) => dbContext.Vehicles.Add(vehicle);
}
