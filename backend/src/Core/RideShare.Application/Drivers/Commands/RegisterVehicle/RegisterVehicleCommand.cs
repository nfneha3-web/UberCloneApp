using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;
using RideShare.Domain.Enums;

namespace RideShare.Application.Drivers.Commands.RegisterVehicle;

public sealed record RegisterVehicleCommand(
    Guid DriverApplicationUserId,
    string Make,
    string Model,
    int Year,
    string Color,
    string PlateNumber,
    VehicleType VehicleType) : IRequest<Guid>;

public class RegisterVehicleCommandHandler(
    IDriverProfileRepository driverProfileRepository,
    IVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterVehicleCommand, Guid>
{
    public async Task<Guid> Handle(RegisterVehicleCommand request, CancellationToken cancellationToken)
    {
        var driverProfile = await driverProfileRepository.GetByApplicationUserIdAsync(request.DriverApplicationUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(DriverProfile), request.DriverApplicationUserId);

        var vehicle = new Vehicle(driverProfile.Id, request.Make, request.Model, request.Year, request.Color, request.PlateNumber, request.VehicleType);
        vehicleRepository.Add(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return vehicle.Id;
    }
}
