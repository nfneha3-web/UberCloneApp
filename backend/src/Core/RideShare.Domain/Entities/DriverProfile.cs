using RideShare.Domain.Common;
using RideShare.Domain.Enums;
using RideShare.Domain.Exceptions;
using RideShare.Domain.ValueObjects;

namespace RideShare.Domain.Entities;

public class DriverProfile : BaseEntity
{
    public Guid ApplicationUserId { get; private set; }
    public string FullName { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string LicenseNumber { get; private set; } = default!;
    public DriverAvailabilityStatus AvailabilityStatus { get; private set; } = DriverAvailabilityStatus.Offline;
    public double AverageRating { get; private set; } = 5.0;
    public int RatingCount { get; private set; }
    public string? StripeConnectedAccountId { get; private set; }

    public double? LastKnownLatitude { get; private set; }
    public double? LastKnownLongitude { get; private set; }
    public DateTime? LastLocationUpdateUtc { get; private set; }

    private DriverProfile() { }

    public DriverProfile(Guid applicationUserId, string fullName, string phoneNumber, string licenseNumber)
    {
        if (applicationUserId == Guid.Empty)
            throw new DomainException("A driver profile must be linked to a user.");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Phone number is required.");
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new DomainException("Driver's license number is required.");

        ApplicationUserId = applicationUserId;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        LicenseNumber = licenseNumber;
    }

    public void GoOnline()
    {
        if (AvailabilityStatus == DriverAvailabilityStatus.OnTrip)
            throw new DomainException("Cannot go online while already on a trip.");

        AvailabilityStatus = DriverAvailabilityStatus.Online;
        Touch();
    }

    public void GoOffline()
    {
        if (AvailabilityStatus == DriverAvailabilityStatus.OnTrip)
            throw new DomainException("Cannot go offline while on a trip.");

        AvailabilityStatus = DriverAvailabilityStatus.Offline;
        Touch();
    }

    public void MarkOnTrip() => AvailabilityStatus = DriverAvailabilityStatus.OnTrip;

    public void MarkAvailableAfterTrip() => AvailabilityStatus = DriverAvailabilityStatus.Online;

    public void UpdateLocation(GeoPoint location)
    {
        LastKnownLatitude = location.Latitude;
        LastKnownLongitude = location.Longitude;
        LastLocationUpdateUtc = DateTime.UtcNow;
    }

    public void AttachStripeAccount(string stripeConnectedAccountId)
    {
        StripeConnectedAccountId = stripeConnectedAccountId;
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
