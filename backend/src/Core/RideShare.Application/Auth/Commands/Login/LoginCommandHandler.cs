using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;

namespace RideShare.Application.Auth.Commands.Login;

public class LoginCommandHandler(IIdentityService identityService, IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);

        if (!result.Succeeded || result.ApplicationUserId is null)
            throw new UnauthorizedException("Invalid email or password.");

        var token = jwtTokenGenerator.GenerateToken(result.ApplicationUserId.Value, result.Email!, result.Roles);

        return new AuthResult(token, result.ApplicationUserId.Value, result.Email!, result.Roles);
    }
}
