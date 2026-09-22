using RideShare.Domain.Common;
using RideShare.Domain.Exceptions;

namespace RideShare.Domain.Entities;

public enum RatingDirection
{
    RiderToDriver = 0,
    DriverToRider = 1
}

public class Rating : BaseEntity
{
    public Guid RideId { get; private set; }
    public Guid RaterProfileId { get; private set; }
    public Guid RateeProfileId { get; private set; }
    public RatingDirection Direction { get; private set; }
    public int Stars { get; private set; }
    public string? Comment { get; private set; }

    private Rating() { }

    public Rating(Guid rideId, Guid raterProfileId, Guid rateeProfileId, RatingDirection direction, int stars, string? comment)
    {
        if (rideId == Guid.Empty)
            throw new DomainException("A rating must be linked to a ride.");
        if (stars is < 1 or > 5)
            throw new DomainException("Rating must be between 1 and 5 stars.");

        RideId = rideId;
        RaterProfileId = raterProfileId;
        RateeProfileId = rateeProfileId;
        Direction = direction;
        Stars = stars;
        Comment = comment;
    }
}
