namespace RideShare.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
