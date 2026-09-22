using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RideShare.Application.Common.Interfaces;

namespace RideShare.Infrastructure.Realtime;

/// <summary>
/// On connect, a user is auto-joined to "rider-{riderProfileId}" and/or "driver-{driverProfileId}"
/// groups based on their JWT identity, so the server can push to "whoever is logged in as this
/// person" without the client doing anything. Once a ride is assigned, both sides additionally
/// call JoinRideGroup so live location/status pushes for that specific ride reach exactly them.
/// </summary>
[Authorize]
public class RideHub(IDriverProfileRepository driverProfileRepository, IRiderProfileRepository riderProfileRepository) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userIdClaim = Context.User?.FindFirst("uid")?.Value;

        if (Guid.TryParse(userIdClaim, out var applicationUserId))
        {
            var driverProfile = await driverProfileRepository.GetByApplicationUserIdAsync(applicationUserId);
            if (driverProfile is not null)
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.Driver(driverProfile.Id));

            var riderProfile = await riderProfileRepository.GetByApplicationUserIdAsync(applicationUserId);
            if (riderProfile is not null)
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.Rider(riderProfile.Id));
        }

        await base.OnConnectedAsync();
    }

    public Task JoinRideGroup(Guid rideId) => Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.Ride(rideId));

    public Task LeaveRideGroup(Guid rideId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNames.Ride(rideId));
}

internal static class GroupNames
{
    public static string Driver(Guid driverProfileId) => $"driver-{driverProfileId}";
    public static string Rider(Guid riderProfileId) => $"rider-{riderProfileId}";
    public static string Ride(Guid rideId) => $"ride-{rideId}";
}
