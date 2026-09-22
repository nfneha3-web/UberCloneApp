using MediatR;
using RideShare.Application.Common.Models;

namespace RideShare.Application.Auth.Commands.RegisterRider;

public sealed record RegisterRiderCommand(string Email, string Password, string FullName, string PhoneNumber) : IRequest<AuthResult>;
