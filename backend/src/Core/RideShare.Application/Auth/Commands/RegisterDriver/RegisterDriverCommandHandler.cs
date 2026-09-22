using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Application.Common.Models;
using RideShare.Domain.Entities;

namespace RideShare.Application.Auth.Commands.RegisterDriver;

public class RegisterDriverCommandHandler(
    IIdentityService identityService,
    IDriverProfileRepository driverProfileRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterDriverCommand, AuthResult>
{
    private const string DriverRole = "Driver";

    public async Task<AuthResult> Handle(RegisterDriverCommand request, CancellationToken cancellationToken)
    {
        var registerResult = await identityService.RegisterUserAsync(request.Email, request.Password, DriverRole, cancellationToken);

        if (!registerResult.Succeeded || registerResult.ApplicationUserId is null)
            throw new ConflictException(string.Join(" ", registerResult.Errors));

        var driverProfile = new DriverProfile(registerResult.ApplicationUserId.Value, request.FullName, request.PhoneNumber, request.LicenseNumber);
        driverProfileRepository.Add(driverProfile);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = jwtTokenGenerator.GenerateToken(registerResult.ApplicationUserId.Value, request.Email, [DriverRole]);

        return new AuthResult(token, registerResult.ApplicationUserId.Value, request.Email, [DriverRole]);
    }
}
