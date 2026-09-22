using FluentValidation;

namespace RideShare.Application.Rides.Commands.StartRide;

public class StartRideCommandValidator : AbstractValidator<StartRideCommand>
{
    public StartRideCommandValidator()
    {
        RuleFor(x => x.RideId).NotEmpty();
        RuleFor(x => x.DriverApplicationUserId).NotEmpty();
    }
}
