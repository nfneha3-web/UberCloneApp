using Microsoft.AspNetCore.Identity;

namespace RideShare.Identity.Entities;

/// <summary>
/// Pure authentication identity. All app-specific data (name, phone, rating, vehicles...)
/// lives in RiderProfile/DriverProfile in the Domain layer, linked back to this Id — Identity
/// stays a bounded concern the Domain/Application layers never reference directly.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
}
