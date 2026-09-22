using global::Dapper;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;

namespace RideShare.Infrastructure.Persistence.Dapper;

public class RideQueryService(ISqlConnectionFactory connectionFactory) : IRideQueryService
{
    private const string BaseSelect = """
        SELECT
            r.Id, r.RiderProfileId, rp.FullName AS RiderName, r.DriverProfileId, dp.FullName AS DriverName,
            CASE WHEN v.Id IS NOT NULL THEN v.Color + ' ' + v.Make + ' ' + v.Model + ' (' + v.PlateNumber + ')' ELSE NULL END AS VehicleDescription,
            r.PickupLatitude, r.PickupLongitude, r.PickupAddress,
            r.DropoffLatitude, r.DropoffLongitude, r.DropoffAddress,
            r.Status, r.EstimatedFareAmount, r.FinalFareAmount, r.EstimatedFareCurrency AS Currency,
            r.EstimatedDistanceKm, r.RequestedAtUtc, r.StartedAtUtc, r.CompletedAtUtc
        FROM Rides r
        INNER JOIN RiderProfiles rp ON rp.Id = r.RiderProfileId
        LEFT JOIN DriverProfiles dp ON dp.Id = r.DriverProfileId
        LEFT JOIN Vehicles v ON v.Id = r.VehicleId
        """;

    public async Task<RideDetailsDto?> GetByIdAsync(Guid rideId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = $"{BaseSelect} WHERE r.Id = @RideId";
        return await connection.QueryFirstOrDefaultAsync<RideDetailsDto>(sql, new { RideId = rideId });
    }

    public async Task<IReadOnlyList<RideHistoryItemDto>> GetHistoryForRiderAsync(Guid riderProfileId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, Status, PickupLatitude, PickupLongitude, DropoffLatitude, DropoffLongitude,
                   FinalFareAmount, EstimatedFareAmount, EstimatedFareCurrency AS Currency, RequestedAtUtc, CompletedAtUtc
            FROM Rides
            WHERE RiderProfileId = @RiderProfileId
            ORDER BY RequestedAtUtc DESC
            """;
        var results = await connection.QueryAsync<RideHistoryItemDto>(sql, new { RiderProfileId = riderProfileId });
        return results.ToList();
    }

    public async Task<IReadOnlyList<RideHistoryItemDto>> GetHistoryForDriverAsync(Guid driverProfileId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
            SELECT Id, Status, PickupLatitude, PickupLongitude, DropoffLatitude, DropoffLongitude,
                   FinalFareAmount, EstimatedFareAmount, EstimatedFareCurrency AS Currency, RequestedAtUtc, CompletedAtUtc
            FROM Rides
            WHERE DriverProfileId = @DriverProfileId
            ORDER BY RequestedAtUtc DESC
            """;
        var results = await connection.QueryAsync<RideHistoryItemDto>(sql, new { DriverProfileId = driverProfileId });
        return results.ToList();
    }

    public async Task<RideDetailsDto?> GetActiveRideForRiderAsync(Guid riderProfileId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = $"""
            {BaseSelect}
            WHERE r.RiderProfileId = @RiderProfileId
              AND r.Status IN ('Requested', 'DriverAssigned', 'DriverArriving', 'InProgress')
            """;
        return await connection.QueryFirstOrDefaultAsync<RideDetailsDto>(sql, new { RiderProfileId = riderProfileId });
    }

    public async Task<RideDetailsDto?> GetActiveRideForDriverAsync(Guid driverProfileId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = $"""
            {BaseSelect}
            WHERE r.DriverProfileId = @DriverProfileId
              AND r.Status IN ('Requested', 'DriverAssigned', 'DriverArriving', 'InProgress')
            """;
        return await connection.QueryFirstOrDefaultAsync<RideDetailsDto>(sql, new { DriverProfileId = driverProfileId });
    }
}
