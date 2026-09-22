using global::Dapper;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;
using RideShare.Domain.Enums;
using RideShare.Domain.ValueObjects;

namespace RideShare.Infrastructure.Persistence.Dapper;

/// <summary>
/// SQL Server has no spatial index configured here (keeping the free-tier setup simple), so this
/// pre-filters with a cheap lat/lon bounding box in SQL, then applies the exact haversine distance
/// (via the same GeoPoint value object the Domain uses) and sorts/truncates in memory. Fine at the
/// city-block driver counts this app targets; a real spatial index is a Phase 2/3 optimization.
/// </summary>
public class DriverSearchQueryService(ISqlConnectionFactory connectionFactory) : IDriverSearchQueryService
{
    private const double KmPerDegreeLatitude = 111.0;

    public async Task<IReadOnlyList<NearbyDriverDto>> FindNearbyAvailableDriversAsync(
        double latitude, double longitude, double radiusKm, VehicleType? vehicleType, CancellationToken cancellationToken = default)
    {
        var latDelta = radiusKm / KmPerDegreeLatitude;
        var lonDelta = radiusKm / (KmPerDegreeLatitude * Math.Max(Math.Cos(latitude * Math.PI / 180.0), 0.1));

        const string sql = """
            SELECT d.Id AS DriverProfileId, d.FullName, d.AverageRating, d.LastKnownLatitude AS Latitude, d.LastKnownLongitude AS Longitude,
                   v.Id AS VehicleId, v.VehicleType, v.Color + ' ' + v.Make + ' ' + v.Model + ' (' + v.PlateNumber + ')' AS VehicleDescription
            FROM DriverProfiles d
            INNER JOIN Vehicles v ON v.DriverProfileId = d.Id AND v.IsActive = 1
            WHERE d.AvailabilityStatus = 'Online'
              AND d.LastKnownLatitude BETWEEN @MinLat AND @MaxLat
              AND d.LastKnownLongitude BETWEEN @MinLon AND @MaxLon
              AND (@VehicleType IS NULL OR v.VehicleType = @VehicleType)
            """;

        using var connection = connectionFactory.CreateConnection();
        var candidates = await connection.QueryAsync<CandidateRow>(sql, new
        {
            MinLat = latitude - latDelta,
            MaxLat = latitude + latDelta,
            MinLon = longitude - lonDelta,
            MaxLon = longitude + lonDelta,
            VehicleType = vehicleType?.ToString()
        });

        var origin = new GeoPoint(latitude, longitude);

        return candidates
            .Select(c => new { c, DistanceKm = origin.DistanceInKmTo(new GeoPoint(c.Latitude, c.Longitude)) })
            .Where(x => x.DistanceKm <= radiusKm)
            .OrderBy(x => x.DistanceKm)
            .Select(x => new NearbyDriverDto(
                x.c.DriverProfileId, x.c.FullName, x.c.AverageRating, x.c.Latitude, x.c.Longitude,
                Math.Round(x.DistanceKm, 2), x.c.VehicleId, Enum.Parse<VehicleType>(x.c.VehicleType), x.c.VehicleDescription))
            .ToList();
    }

    private sealed record CandidateRow(
        Guid DriverProfileId, string FullName, double AverageRating, double Latitude, double Longitude,
        Guid VehicleId, string VehicleType, string VehicleDescription);
}
