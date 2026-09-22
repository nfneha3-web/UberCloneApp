using RideShare.Domain.Enums;

namespace RideShare.Api.Contracts;

public sealed record RequestRideRequest(
    double PickupLatitude, double PickupLongitude, string? PickupAddress,
    double DropoffLatitude, double DropoffLongitude, string? DropoffAddress,
    VehicleType VehicleType);

public sealed record CancelRideRequest(string Reason);

public sealed record RegisterVehicleRequest(string Make, string Model, int Year, string Color, string PlateNumber, VehicleType VehicleType);

public sealed record UpdateLocationRequest(double Latitude, double Longitude, Guid? ActiveRideId);

public sealed record SubmitRatingRequest(Guid RideId, int Stars, string? Comment);

public sealed record SendCopilotMessageRequest(Guid? ConversationId, string Message);
