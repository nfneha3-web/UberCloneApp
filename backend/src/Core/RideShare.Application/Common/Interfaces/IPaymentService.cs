using RideShare.Domain.ValueObjects;

namespace RideShare.Application.Common.Interfaces;

public sealed record PaymentIntentResult(string PaymentIntentId, string ClientSecret, string Status);

/// <summary>
/// Abstracts Stripe so the Application layer never references the Stripe SDK directly.
/// The Infrastructure implementation runs against Stripe TEST MODE keys only (see appsettings) —
/// no real money ever moves, but the integration code is identical to a production Stripe setup.
/// </summary>
public interface IPaymentService
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(Money amount, string? stripeCustomerId, CancellationToken cancellationToken = default);

    Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);

    Task RefundAsync(string paymentIntentId, CancellationToken cancellationToken = default);
}
