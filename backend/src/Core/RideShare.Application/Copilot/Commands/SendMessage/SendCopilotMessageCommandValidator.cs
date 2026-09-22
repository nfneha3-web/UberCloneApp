using FluentValidation;

namespace RideShare.Application.Copilot.Commands.SendMessage;

public class SendCopilotMessageCommandValidator : AbstractValidator<SendCopilotMessageCommand>
{
    public SendCopilotMessageCommandValidator()
    {
        RuleFor(x => x.ApplicationUserId).NotEmpty();
        RuleFor(x => x.Role).NotEmpty().Must(r => r is "Rider" or "Driver").WithMessage("Role must be 'Rider' or 'Driver'.");
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}
