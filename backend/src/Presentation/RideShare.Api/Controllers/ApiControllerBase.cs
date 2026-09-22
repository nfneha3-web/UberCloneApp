using Microsoft.AspNetCore.Mvc;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;

namespace RideShare.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase(ICurrentUserService currentUserService) : ControllerBase
{
    protected Guid CurrentUserId => currentUserService.ApplicationUserId
        ?? throw new UnauthorizedException("No authenticated user on this request.");
}
