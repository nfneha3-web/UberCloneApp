using MediatR;

namespace RideShare.Application.Ratings.Commands.SubmitRating;

public sealed record SubmitRatingCommand(
    Guid RideId,
    Guid RaterApplicationUserId,
    int Stars,
    string? Comment) : IRequest;
