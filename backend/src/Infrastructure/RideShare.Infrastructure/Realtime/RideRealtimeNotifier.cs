using Microsoft.AspNetCore.SignalR;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;

namespace RideShare.Infrastructure.Realtime;

public class RideRealtimeNotifier(IHubContext<RideHub> hubContext) : IRideRealtimeNotifier
{
    public Task NotifyRideStatusChangedAsync(RideDetailsDto ride, CancellationToken cancellationToken = default)
    {
        var groups = new List<string> { GroupNames.Rider(ride.RiderProfileId), GroupNames.Ride(ride.Id) };
        if (ride.DriverProfileId is not null)
            groups.Add(GroupNames.Driver(ride.DriverProfileId.Value));

        return hubContext.Clients.Groups(groups).SendAsync("RideStatusChanged", ride, cancellationToken);
    }

    public Task NotifyDriverOfferAsync(Guid driverProfileId, RideDetailsDto ride, CancellationToken cancellationToken = default) =>
        hubContext.Clients.Group(GroupNames.Driver(driverProfileId)).SendAsync("RideOffer", ride, cancellationToken);

    public Task NotifyDriverLocationAsync(Guid rideId, double latitude, double longitude, CancellationToken cancellationToken = default) =>
        hubContext.Clients.Group(GroupNames.Ride(rideId)).SendAsync("DriverLocationUpdated", new { rideId, latitude, longitude }, cancellationToken);
}
