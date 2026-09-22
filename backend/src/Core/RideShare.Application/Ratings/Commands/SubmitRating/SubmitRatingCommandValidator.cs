using FluentValidation;

namespace RideShare.Application.Ratings.Commands.SubmitRating;

public class SubmitRatingCommandValidator : AbstractValidator<SubmitRatingCommand>
{
    public SubmitRatingCommandValidator()
    {
        RuleFor(x => x.RideId).NotEmpty();
        RuleFor(x => x.RaterApplicationUserId).NotEmpty();
        RuleFor(x => x.Stars).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}
