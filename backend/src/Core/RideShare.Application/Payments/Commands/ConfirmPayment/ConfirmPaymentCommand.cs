using MediatR;

namespace RideShare.Application.Payments.Commands.ConfirmPayment;

public sealed record ConfirmPaymentCommand(string PaymentIntentId) : IRequest<bool>;
