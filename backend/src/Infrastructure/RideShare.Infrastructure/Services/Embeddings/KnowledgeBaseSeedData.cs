namespace RideShare.Infrastructure.Services.Embeddings;

/// <summary>
/// Seed content for the copilot's RAG knowledge base. Deliberately scoped to what Phase 1 of
/// this app actually does (docs/ROADMAP.md) — the copilot should never sound more capable than
/// the product is, so nothing here describes a feature (driver background checks, 24/7 live
/// support, trip-sharing) that isn't actually built yet.
/// </summary>
public static class KnowledgeBaseSeedData
{
    public static readonly IReadOnlyList<(string Title, string Category, string Content)> Articles =
    [
        ("Cancellation policy", "Policy",
            "You can cancel a ride any time before it's marked in-progress — that covers the Requested, " +
            "DriverAssigned, and DriverArriving stages. Once a ride is InProgress or Completed it can no " +
            "longer be cancelled. Either the rider or the assigned driver can cancel; whoever cancels should " +
            "give a brief reason, which is recorded with the ride."),

        ("How fares are calculated", "Pricing",
            "Fares are estimated at request time from a base fare of $2.50 plus $1.20 per kilometer of " +
            "straight-line distance between pickup and dropoff, multiplied by a vehicle-type factor: Economy " +
            "1.0x, Comfort 1.3x, XL 1.6x, Premium 2.2x. There's a $5.00 minimum fare. In this version, the " +
            "fare shown at request time is also the final fare charged — live metering of actual distance " +
            "or time driven isn't implemented yet."),

        ("How payment works", "Payments",
            "Payment is handled through Stripe in TEST MODE only — no real card is ever charged, and no " +
            "real money moves. When the driver marks a ride complete, a Stripe PaymentIntent is created for " +
            "the ride's fare; the rider then confirms payment from the app using a Stripe test card (for " +
            "example 4242 4242 4242 4242, any future expiry date, any CVC)."),

        ("Vehicle types", "Rides",
            "Four vehicle types are available when requesting a ride: Economy (standard, cheapest), Comfort " +
            "(nicer car, 1.3x base pricing), XL (larger vehicle, 1.6x base pricing), and Premium (top tier, " +
            "2.2x base pricing). The rider picks a type when requesting; only drivers with an active vehicle " +
            "of a matching type are offered that ride."),

        ("Becoming a driver", "Driver",
            "After creating a driver account, you must register at least one active vehicle (make, model, " +
            "year, color, plate number, and vehicle type) before you're able to go online. Once online, your " +
            "device periodically reports your location so nearby ride requests can find you; going offline " +
            "stops new ride offers."),

        ("Ratings", "Trust & Safety",
            "After a ride is completed, both the rider and the driver can rate each other from 1 to 5 stars, " +
            "optionally with a comment. Each person's average rating updates immediately and is shown to the " +
            "other side before a ride is accepted, as a basic trust signal. A ride can only be rated once by " +
            "each participant."),

        ("Accounts and roles", "Account",
            "Rider and driver are separate account types tied to the email you register with — a single " +
            "sign-up creates one role. If you want to use the app as both a rider and a driver, register two " +
            "accounts with different email addresses."),

        ("What's not built yet", "Roadmap",
            "This is an early version of the product. Things like surge pricing, scheduled rides in advance, " +
            "promo codes, driver background-check/KYC verification, in-app chat between rider and driver, " +
            "and a full admin analytics dashboard are planned but not available yet — don't tell a user any " +
            "of those currently work.")
    ];
}
