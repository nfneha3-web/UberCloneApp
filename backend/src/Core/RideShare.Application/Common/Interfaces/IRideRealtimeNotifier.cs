using RideShare.Application.Common.Models;

namespace RideShare.Application.Common.Interfaces;

/// <summary>Abstracts the SignalR hub so Application handlers never reference SignalR types directly.</summary>
public interface IRideRealtimeNotifier
{
    Task NotifyRideStatusChangedAsync(RideDetailsDto ride, CancellationToken cancellationToken = default);

    Task NotifyDriverOfferAsync(Guid driverProfileId, RideDetailsDto ride, CancellationToken cancellationToken = default);

    Task NotifyDriverLocationAsync(Guid rideId, double latitude, double longitude, CancellationToken cancellationToken = default);
}
