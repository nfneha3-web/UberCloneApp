using FluentValidation;

namespace RideShare.Application.Rides.Commands.CompleteRide;

public class CompleteRideCommandValidator : AbstractValidator<CompleteRideCommand>
{
    public CompleteRideCommandValidator()
    {
        RuleFor(x => x.RideId).NotEmpty();
        RuleFor(x => x.DriverApplicationUserId).NotEmpty();
    }
}
