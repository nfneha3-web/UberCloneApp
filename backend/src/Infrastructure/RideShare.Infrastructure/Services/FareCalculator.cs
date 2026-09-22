using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Enums;
using RideShare.Domain.ValueObjects;

namespace RideShare.Infrastructure.Services;

public class FareCalculator : IFareCalculator
{
    private const decimal BaseFare = 2.50m;
    private const decimal PerKmRate = 1.20m;
    private const decimal MinimumFare = 5.00m;

    public Money EstimateFare(double distanceKm, VehicleType vehicleType)
    {
        var multiplier = VehicleMultiplier(vehicleType);
        var raw = BaseFare + (decimal)distanceKm * PerKmRate;
        var withMultiplier = raw * multiplier;

        return new Money(Math.Max(withMultiplier, MinimumFare));
    }

    private static decimal VehicleMultiplier(VehicleType vehicleType) => vehicleType switch
    {
        VehicleType.Economy => 1.0m,
        VehicleType.Comfort => 1.3m,
        VehicleType.Xl => 1.6m,
        VehicleType.Premium => 2.2m,
        _ => 1.0m
    };
}
