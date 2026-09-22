using RideShare.Domain.Entities;

namespace RideShare.Application.Common.Interfaces;

// EF Core-backed write-side repositories, one per aggregate root.
// Reads for lists/projections/dashboards go through the Dapper query services instead
// (see IQueryServices.cs) — keeping repositories honest to CQRS rather than becoming
// a second, slower query API.

public interface IRideRepository
{
    Task<Ride?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Ride?> GetActiveRideForRiderAsync(Guid riderProfileId, CancellationToken cancellationToken = default);
    Task<Ride?> GetActiveRideForDriverAsync(Guid driverProfileId, CancellationToken cancellationToken = default);
    void Add(Ride ride);
}

public interface IRiderProfileRepository
{
    Task<RiderProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RiderProfile?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
    void Add(RiderProfile profile);
}

public interface IDriverProfileRepository
{
    Task<DriverProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DriverProfile?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
    void Add(DriverProfile profile);
}

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetActiveVehicleForDriverAsync(Guid driverProfileId, CancellationToken cancellationToken = default);
    void Add(Vehicle vehicle);
}

public interface IPaymentRepository
{
    Task<Payment?> GetByRideIdAsync(Guid rideId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    void Add(Payment payment);
}

public interface IRatingRepository
{
    Task<bool> ExistsAsync(Guid rideId, Guid raterProfileId, CancellationToken cancellationToken = default);
    void Add(Rating rating);
}

public interface ICopilotConversationRepository
{
    Task<CopilotConversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CopilotConversation?> GetMostRecentForUserAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
    void Add(CopilotConversation conversation);

    /// <summary>
    /// Explicitly stages a new message for insert, instead of relying solely on EF Core's
    /// collection-navigation change tracking. Needed because SendCopilotMessageCommandHandler can
    /// call SaveChangesAsync twice in one request (once directly, once via a nested tool-call
    /// command like RequestRideCommand) — after the first save, EF's snapshot of the conversation's
    /// Messages collection goes stale, so a message added afterward isn't reliably picked up as
    /// "Added" without this explicit call.
    /// </summary>
    void AddMessage(CopilotMessage message);
}
