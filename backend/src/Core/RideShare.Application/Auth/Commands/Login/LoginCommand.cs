using MediatR;
using RideShare.Application.Common.Models;

namespace RideShare.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResult>;
