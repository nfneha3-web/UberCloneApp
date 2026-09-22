namespace RideShare.Application.Common.Interfaces;

/// <summary>
/// Wraps the EF Core DbContext's change tracking + transaction so that command handlers
/// commit once, atomically, instead of each repository saving independently.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
