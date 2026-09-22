using RideShare.Domain.Enums;

namespace RideShare.Application.Common.Models;

public sealed record RideDetailsDto(
    Guid Id,
    Guid RiderProfileId,
    string RiderName,
    Guid? DriverProfileId,
    string? DriverName,
    string? VehicleDescription,
    double PickupLatitude,
    double PickupLongitude,
    string? PickupAddress,
    double DropoffLatitude,
    double DropoffLongitude,
    string? DropoffAddress,
    string Status,
    decimal EstimatedFareAmount,
    decimal? FinalFareAmount,
    string Currency,
    double EstimatedDistanceKm,
    DateTime RequestedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc);

public sealed record RideHistoryItemDto(
    Guid Id,
    string Status,
    double PickupLatitude,
    double PickupLongitude,
    double DropoffLatitude,
    double DropoffLongitude,
    decimal? FinalFareAmount,
    decimal EstimatedFareAmount,
    string Currency,
    DateTime RequestedAtUtc,
    DateTime? CompletedAtUtc);

public sealed record NearbyDriverDto(
    Guid DriverProfileId,
    string FullName,
    double AverageRating,
    double Latitude,
    double Longitude,
    double DistanceKm,
    Guid VehicleId,
    VehicleType VehicleType,
    string VehicleDescription);
