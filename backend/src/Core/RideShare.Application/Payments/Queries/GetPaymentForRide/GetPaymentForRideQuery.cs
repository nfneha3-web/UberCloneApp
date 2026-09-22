using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Application.Payments.Queries.GetPaymentForRide;

public sealed record PaymentInfoDto(string PaymentIntentId, string ClientSecret, string Status, decimal Amount, string Currency);

public sealed record GetPaymentForRideQuery(Guid RideId) : IRequest<PaymentInfoDto>;

public class GetPaymentForRideQueryHandler(IPaymentRepository paymentRepository) : IRequestHandler<GetPaymentForRideQuery, PaymentInfoDto>
{
    public async Task<PaymentInfoDto> Handle(GetPaymentForRideQuery request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByRideIdAsync(request.RideId, cancellationToken)
            ?? throw new NotFoundException(nameof(Payment), request.RideId);

        return new PaymentInfoDto(payment.StripePaymentIntentId, payment.ClientSecret, payment.Status.ToString(), payment.Amount.Amount, payment.Amount.Currency);
    }
}
