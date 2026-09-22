using FluentValidation;

namespace RideShare.Application.Drivers.Commands.RegisterVehicle;

public class RegisterVehicleCommandValidator : AbstractValidator<RegisterVehicleCommand>
{
    public RegisterVehicleCommandValidator()
    {
        RuleFor(x => x.DriverApplicationUserId).NotEmpty();
        RuleFor(x => x.Make).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Year).InclusiveBetween(1980, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Color).NotEmpty().MaximumLength(30);
        RuleFor(x => x.PlateNumber).NotEmpty().MaximumLength(15);
        RuleFor(x => x.VehicleType).IsInEnum();
    }
}
