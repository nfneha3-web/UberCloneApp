using MediatR;
using Microsoft.AspNetCore.Mvc;
using RideShare.Application.Auth.Commands.Login;
using RideShare.Application.Auth.Commands.RegisterDriver;
using RideShare.Application.Auth.Commands.RegisterRider;
using RideShare.Application.Common.Models;

namespace RideShare.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register/rider")]
    public async Task<ActionResult<AuthResult>> RegisterRider(RegisterRiderCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [HttpPost("register/driver")]
    public async Task<ActionResult<AuthResult>> RegisterDriver(RegisterDriverCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login(LoginCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));
}
