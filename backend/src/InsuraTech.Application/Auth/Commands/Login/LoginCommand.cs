using InsuraTech.Application.Auth.DTOs;
using MediatR;

namespace InsuraTech.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;
