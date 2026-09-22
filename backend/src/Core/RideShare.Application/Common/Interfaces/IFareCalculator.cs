using RideShare.Domain.Enums;
using RideShare.Domain.ValueObjects;

namespace RideShare.Application.Common.Interfaces;

public interface IFareCalculator
{
    Money EstimateFare(double distanceKm, VehicleType vehicleType);
}
