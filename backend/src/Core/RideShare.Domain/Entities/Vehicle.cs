using RideShare.Domain.Common;
using RideShare.Domain.Enums;
using RideShare.Domain.Exceptions;

namespace RideShare.Domain.Entities;

public class Vehicle : BaseEntity
{
    public Guid DriverProfileId { get; private set; }
    public string Make { get; private set; } = default!;
    public string Model { get; private set; } = default!;
    public int Year { get; private set; }
    public string Color { get; private set; } = default!;
    public string PlateNumber { get; private set; } = default!;
    public VehicleType VehicleType { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Vehicle() { }

    public Vehicle(Guid driverProfileId, string make, string model, int year, string color, string plateNumber, VehicleType vehicleType)
    {
        if (driverProfileId == Guid.Empty)
            throw new DomainException("A vehicle must belong to a driver.");
        if (string.IsNullOrWhiteSpace(make) || string.IsNullOrWhiteSpace(model))
            throw new DomainException("Vehicle make and model are required.");
        if (year < 1980 || year > DateTime.UtcNow.Year + 1)
            throw new DomainException("Vehicle year is not valid.");
        if (string.IsNullOrWhiteSpace(plateNumber))
            throw new DomainException("Plate number is required.");

        DriverProfileId = driverProfileId;
        Make = make;
        Model = model;
        Year = year;
        Color = color;
        PlateNumber = plateNumber.ToUpperInvariant();
        VehicleType = vehicleType;
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }
}
