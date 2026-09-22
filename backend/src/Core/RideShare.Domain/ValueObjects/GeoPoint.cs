using RideShare.Domain.Exceptions;

namespace RideShare.Domain.ValueObjects;

public sealed record GeoPoint
{
    public double Latitude { get; }
    public double Longitude { get; }
    public string? Address { get; }

    public GeoPoint(double latitude, double longitude, string? address = null)
    {
        if (latitude is < -90 or > 90)
            throw new DomainException("Latitude must be between -90 and 90.");
        if (longitude is < -180 or > 180)
            throw new DomainException("Longitude must be between -180 and 180.");

        Latitude = latitude;
        Longitude = longitude;
        Address = address;
    }

    /// <summary>Great-circle distance to another point, in kilometers (haversine formula).</summary>
    public double DistanceInKmTo(GeoPoint other)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = DegreesToRadians(other.Latitude - Latitude);
        var dLon = DegreesToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(Latitude)) * Math.Cos(DegreesToRadians(other.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
