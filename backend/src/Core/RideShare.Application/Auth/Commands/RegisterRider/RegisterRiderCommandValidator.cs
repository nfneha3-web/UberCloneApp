using FluentValidation;

namespace RideShare.Application.Auth.Commands.RegisterRider;

public class RegisterRiderCommandValidator : AbstractValidator<RegisterRiderCommand>
{
    public RegisterRiderCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
    }
}
