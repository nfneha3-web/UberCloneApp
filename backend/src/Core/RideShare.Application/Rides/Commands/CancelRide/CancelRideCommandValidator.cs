using FluentValidation;

namespace RideShare.Application.Rides.Commands.CancelRide;

public class CancelRideCommandValidator : AbstractValidator<CancelRideCommand>
{
    public CancelRideCommandValidator()
    {
        RuleFor(x => x.RideId).NotEmpty();
        RuleFor(x => x.RequestingApplicationUserId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
    }
}
