using System.Security.Claims;
using RideShare.Application.Common.Interfaces;

namespace RideShare.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? ApplicationUserId
    {
        get
        {
            var claim = User?.FindFirst("uid")?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.FindFirst("email")?.Value;

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
