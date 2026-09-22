using FluentValidation;

namespace RideShare.Application.Rides.Commands.RequestRide;

public class RequestRideCommandValidator : AbstractValidator<RequestRideCommand>
{
    public RequestRideCommandValidator()
    {
        RuleFor(x => x.RiderApplicationUserId).NotEmpty();

        RuleFor(x => x.PickupLatitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.PickupLongitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.DropoffLatitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.DropoffLongitude).InclusiveBetween(-180, 180);

        RuleFor(x => x.VehicleType).IsInEnum();

        RuleFor(x => x)
            .Must(x => Math.Abs(x.PickupLatitude - x.DropoffLatitude) > 0.0001 || Math.Abs(x.PickupLongitude - x.DropoffLongitude) > 0.0001)
            .WithMessage("Pickup and dropoff locations must be different.");
    }
}
