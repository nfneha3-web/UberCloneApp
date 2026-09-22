using Microsoft.Extensions.Options;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.ValueObjects;
using Stripe;

namespace RideShare.Infrastructure.Services.Payments;

/// <summary>
/// Talks to Stripe using a TEST MODE secret key configured in appsettings ("Stripe:SecretKey", "sk_test_...").
/// Every PaymentIntent created here is a Stripe test-mode object — no real card is charged, no real
/// money moves, ever — but the request/response shape is identical to a live Stripe integration.
/// </summary>
public class StripePaymentService : IPaymentService
{
    private readonly PaymentIntentService _paymentIntentService;

    public StripePaymentService(IOptions<StripeSettings> stripeSettings)
    {
        var client = new StripeClient(stripeSettings.Value.SecretKey);
        _paymentIntentService = new PaymentIntentService(client);
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(Money amount, string? stripeCustomerId, CancellationToken cancellationToken = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = ToSmallestCurrencyUnit(amount),
            Currency = amount.Currency.ToLowerInvariant(),
            Customer = stripeCustomerId,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
        };

        var intent = await _paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);
        return new PaymentIntentResult(intent.Id, intent.ClientSecret, intent.Status);
    }

    public async Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var intent = await _paymentIntentService.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
        return intent.Status == "succeeded";
    }

    public async Task RefundAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var refundService = new RefundService();
        await refundService.CreateAsync(new RefundCreateOptions { PaymentIntent = paymentIntentId }, cancellationToken: cancellationToken);
    }

    /// <summary>Stripe amounts are integers in the currency's smallest unit (cents for USD).</summary>
    private static long ToSmallestCurrencyUnit(Money amount) => (long)Math.Round(amount.Amount * 100, MidpointRounding.AwayFromZero);
}
