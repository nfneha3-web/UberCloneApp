namespace RideShare.Application.Common.Interfaces;

/// <summary>Resolved from the caller's JWT claims by the Api layer; consumed by handlers that need "who is asking".</summary>
public interface ICurrentUserService
{
    Guid? ApplicationUserId { get; }
    string? Email { get; }
    bool IsInRole(string role);
}
