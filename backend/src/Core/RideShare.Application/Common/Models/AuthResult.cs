namespace RideShare.Application.Common.Models;

public sealed record AuthResult(string Token, Guid ApplicationUserId, string Email, IReadOnlyList<string> Roles);
