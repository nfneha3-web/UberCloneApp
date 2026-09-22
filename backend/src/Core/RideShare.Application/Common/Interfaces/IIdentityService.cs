namespace RideShare.Application.Common.Interfaces;

public sealed record RegisterUserResult(bool Succeeded, Guid? ApplicationUserId, IReadOnlyList<string> Errors);

public sealed record ValidateCredentialsResult(bool Succeeded, Guid? ApplicationUserId, string? Email, IReadOnlyList<string> Roles);

/// <summary>Abstracts ASP.NET Core Identity so the Application layer stays framework-agnostic.</summary>
public interface IIdentityService
{
    Task<RegisterUserResult> RegisterUserAsync(string email, string password, string role, CancellationToken cancellationToken = default);

    Task<ValidateCredentialsResult> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
}
