using RideShare.Domain.Common;
using RideShare.Domain.Enums;
using RideShare.Domain.Exceptions;
using RideShare.Domain.ValueObjects;

namespace RideShare.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid RideId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string StripePaymentIntentId { get; private set; } = default!;

    /// <summary>
    /// Stripe's client secret for this PaymentIntent. Not sensitive in the way an API key is —
    /// Stripe hands it to whichever client completes the payment — so persisting it here lets the
    /// rider fetch it later without an extra round trip to Stripe.
    /// </summary>
    public string ClientSecret { get; private set; } = default!;

    public string? FailureReason { get; private set; }

    private Payment() { }

    public Payment(Guid rideId, Money amount, string stripePaymentIntentId, string clientSecret)
    {
        if (rideId == Guid.Empty)
            throw new DomainException("A payment must be linked to a ride.");
        if (string.IsNullOrWhiteSpace(stripePaymentIntentId))
            throw new DomainException("A Stripe payment intent id is required.");

        RideId = rideId;
        Amount = amount;
        StripePaymentIntentId = stripePaymentIntentId;
        ClientSecret = clientSecret;
    }

    public void MarkSucceeded()
    {
        Status = PaymentStatus.Succeeded;
        Touch();
    }

    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        Touch();
    }

    public void MarkRefunded()
    {
        if (Status != PaymentStatus.Succeeded)
            throw new DomainException("Only a succeeded payment can be refunded.");

        Status = PaymentStatus.Refunded;
        Touch();
    }
}
