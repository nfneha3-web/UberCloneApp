using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Application.Payments.Commands.ConfirmPayment;

public class ConfirmPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IPaymentService paymentService,
    IUnitOfWork unitOfWork) : IRequestHandler<ConfirmPaymentCommand, bool>
{
    public async Task<bool> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByStripePaymentIntentIdAsync(request.PaymentIntentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Payment), request.PaymentIntentId);

        var succeeded = await paymentService.ConfirmPaymentAsync(request.PaymentIntentId, cancellationToken);

        if (succeeded)
            payment.MarkSucceeded();
        else
            payment.MarkFailed("Stripe reported the payment as not succeeded.");

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return succeeded;
    }
}
