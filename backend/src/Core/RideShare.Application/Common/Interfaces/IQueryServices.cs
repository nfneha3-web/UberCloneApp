using RideShare.Application.Common.Models;
using RideShare.Domain.Enums;

namespace RideShare.Application.Common.Interfaces;

// Read-side, Dapper-backed. Hand-written SQL projections — no change tracking,
// no entity hydration cost — kept deliberately separate from the write-side repositories.

public interface IRideQueryService
{
    Task<RideDetailsDto?> GetByIdAsync(Guid rideId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RideHistoryItemDto>> GetHistoryForRiderAsync(Guid riderProfileId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RideHistoryItemDto>> GetHistoryForDriverAsync(Guid driverProfileId, CancellationToken cancellationToken = default);

    Task<RideDetailsDto?> GetActiveRideForRiderAsync(Guid riderProfileId, CancellationToken cancellationToken = default);

    Task<RideDetailsDto?> GetActiveRideForDriverAsync(Guid driverProfileId, CancellationToken cancellationToken = default);
}

public interface IDriverSearchQueryService
{
    Task<IReadOnlyList<NearbyDriverDto>> FindNearbyAvailableDriversAsync(
        double latitude,
        double longitude,
        double radiusKm,
        VehicleType? vehicleType,
        CancellationToken cancellationToken = default);
}
