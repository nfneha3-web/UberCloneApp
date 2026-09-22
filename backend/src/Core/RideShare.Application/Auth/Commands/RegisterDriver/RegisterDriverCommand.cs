using MediatR;
using RideShare.Application.Common.Models;

namespace RideShare.Application.Auth.Commands.RegisterDriver;

public sealed record RegisterDriverCommand(
    string Email,
    string Password,
    string FullName,
    string PhoneNumber,
    string LicenseNumber) : IRequest<AuthResult>;
