namespace RideShare.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid applicationUserId, string email, IEnumerable<string> roles);
}
