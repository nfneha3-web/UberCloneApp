using FluentValidation;

namespace RideShare.Application.Rides.Commands.AcceptRide;

public class AcceptRideCommandValidator : AbstractValidator<AcceptRideCommand>
{
    public AcceptRideCommandValidator()
    {
        RuleFor(x => x.RideId).NotEmpty();
        RuleFor(x => x.DriverApplicationUserId).NotEmpty();
    }
}
