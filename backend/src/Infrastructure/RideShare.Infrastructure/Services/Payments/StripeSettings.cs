namespace RideShare.Infrastructure.Services.Payments;

public class StripeSettings
{
    public const string SectionName = "Stripe";

    /// <summary>Use a TEST MODE secret key only (starts with "sk_test_") — this app never runs with live keys.</summary>
    public string SecretKey { get; set; } = default!;

    public string PublishableKey { get; set; } = default!;
}
