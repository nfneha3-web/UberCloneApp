using RideShare.Domain.Common;
using RideShare.Domain.Exceptions;

namespace RideShare.Domain.Entities;

/// <summary>
/// Rider-specific profile data. Linked to an Identity user by <see cref="ApplicationUserId"/>
/// without a direct reference, keeping the Domain layer free of any ASP.NET Identity dependency.
/// </summary>
public class RiderProfile : BaseEntity
{
    public Guid ApplicationUserId { get; private set; }
    public string FullName { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string? StripeCustomerId { get; private set; }
    public double AverageRating { get; private set; } = 5.0;
    public int RatingCount { get; private set; }

    private RiderProfile() { }

    public RiderProfile(Guid applicationUserId, string fullName, string phoneNumber)
    {
        if (applicationUserId == Guid.Empty)
            throw new DomainException("A rider profile must be linked to a user.");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Phone number is required.");

        ApplicationUserId = applicationUserId;
        FullName = fullName;
        PhoneNumber = phoneNumber;
    }

    public void AttachStripeCustomer(string stripeCustomerId)
    {
        StripeCustomerId = stripeCustomerId;
        Touch();
    }

    public void RecordRating(int stars)
    {
        if (stars is < 1 or > 5)
            throw new DomainException("Rating must be between 1 and 5 stars.");

        var totalScore = AverageRating * RatingCount + stars;
        RatingCount++;
        AverageRating = Math.Round(totalScore / RatingCount, 2);
        Touch();
    }
}
