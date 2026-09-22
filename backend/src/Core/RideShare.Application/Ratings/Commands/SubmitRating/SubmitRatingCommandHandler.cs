using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;
using RideShare.Domain.Enums;

namespace RideShare.Application.Ratings.Commands.SubmitRating;

public class SubmitRatingCommandHandler(
    IRideRepository rideRepository,
    IRiderProfileRepository riderProfileRepository,
    IDriverProfileRepository driverProfileRepository,
    IRatingRepository ratingRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SubmitRatingCommand>
{
    public async Task Handle(SubmitRatingCommand request, CancellationToken cancellationToken)
    {
        var ride = await rideRepository.GetByIdAsync(request.RideId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ride), request.RideId);

        if (ride.Status != RideStatus.Completed)
            throw new ConflictException("You can only rate a completed ride.");
        if (ride.DriverProfileId is null)
            throw new ConflictException("This ride has no assigned driver to rate.");

        var riderProfile = await riderProfileRepository.GetByIdAsync(ride.RiderProfileId, cancellationToken)
            ?? throw new NotFoundException(nameof(RiderProfile), ride.RiderProfileId);
        var driverProfile = await driverProfileRepository.GetByIdAsync(ride.DriverProfileId.Value, cancellationToken)
            ?? throw new NotFoundException(nameof(DriverProfile), ride.DriverProfileId.Value);

        RatingDirection direction;
        Guid raterProfileId, rateeProfileId;

        if (riderProfile.ApplicationUserId == request.RaterApplicationUserId)
        {
            direction = RatingDirection.RiderToDriver;
            raterProfileId = riderProfile.Id;
            rateeProfileId = driverProfile.Id;
        }
        else if (driverProfile.ApplicationUserId == request.RaterApplicationUserId)
        {
            direction = RatingDirection.DriverToRider;
            raterProfileId = driverProfile.Id;
            rateeProfileId = riderProfile.Id;
        }
        else
        {
            throw new ForbiddenAccessException("You are not a participant of this ride.");
        }

        if (await ratingRepository.ExistsAsync(request.RideId, raterProfileId, cancellationToken))
            throw new ConflictException("You have already rated this ride.");

        var rating = new Rating(request.RideId, raterProfileId, rateeProfileId, direction, request.Stars, request.Comment);
        ratingRepository.Add(rating);

        if (direction == RatingDirection.RiderToDriver)
            driverProfile.RecordRating(request.Stars);
        else
            riderProfile.RecordRating(request.Stars);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
