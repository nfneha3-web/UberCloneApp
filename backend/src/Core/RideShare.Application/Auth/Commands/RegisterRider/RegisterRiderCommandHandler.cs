using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;
using RideShare.Domain.Entities;

namespace RideShare.Application.Auth.Commands.RegisterRider;

public class RegisterRiderCommandHandler(
    IIdentityService identityService,
    IRiderProfileRepository riderProfileRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterRiderCommand, AuthResult>
{
    private const string RiderRole = "Rider";

    public async Task<AuthResult> Handle(RegisterRiderCommand request, CancellationToken cancellationToken)
    {
        var registerResult = await identityService.RegisterUserAsync(request.Email, request.Password, RiderRole, cancellationToken);

        if (!registerResult.Succeeded || registerResult.ApplicationUserId is null)
            throw new ConflictException(string.Join(" ", registerResult.Errors));

        var riderProfile = new RiderProfile(registerResult.ApplicationUserId.Value, request.FullName, request.PhoneNumber);
        riderProfileRepository.Add(riderProfile);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = jwtTokenGenerator.GenerateToken(registerResult.ApplicationUserId.Value, request.Email, [RiderRole]);

        return new AuthResult(token, registerResult.ApplicationUserId.Value, request.Email, [RiderRole]);
    }
}
