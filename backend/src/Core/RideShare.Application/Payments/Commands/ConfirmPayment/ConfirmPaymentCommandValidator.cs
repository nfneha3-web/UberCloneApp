using FluentValidation;

namespace RideShare.Application.Payments.Commands.ConfirmPayment;

public class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
{
    public ConfirmPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentIntentId).NotEmpty();
    }
}
