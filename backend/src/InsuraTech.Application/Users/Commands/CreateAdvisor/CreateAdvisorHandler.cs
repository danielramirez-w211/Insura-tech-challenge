using System.Security.Cryptography;
using InsuraTech.Application.Auth.Services;
using InsuraTech.Application.Users.DTOs;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Users;
using MediatR;

namespace InsuraTech.Application.Users.Commands.CreateAdvisor;

public sealed class CreateAdvisorHandler : IRequestHandler<CreateAdvisorCommand, UserResponse>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public CreateAdvisorHandler(IUserRepository users, IPasswordHasher hasher)
    {
        _users  = users;
        _hasher = hasher;
    }

    public async Task<UserResponse> Handle(CreateAdvisorCommand request, CancellationToken cancellationToken)
    {
        var existing = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new BusinessRuleException("EMAIL_DUPLICATE", "El email ya está registrado.");

        var leader = await _users.GetByIdAsync(request.LeaderId, cancellationToken)
            ?? throw new NotFoundException($"Líder '{request.LeaderId}' no encontrado.");

        if (leader.Role != Domain.Users.Role.Leader)
            throw new BusinessRuleException("INVALID_ROLE", "El usuario especificado no es un Líder.");

        DateOnly.TryParse(request.BirthDate, out var birthDate);

        var profile = new UserProfile(
            FirstName:      request.FirstName,
            LastName:       request.LastName,
            Nationality:    request.Nationality,
            BirthDate:      birthDate,
            YearsInCompany: request.YearsInCompany,
            PhotoUrl:       string.Empty,
            OfficeLocation: request.OfficeLocation,
            WorkSchedule:   request.WorkSchedule);

        var advisorCode  = await _users.GetNextAdvisorCodeAsync(cancellationToken);
        var tempPassword = GenerateTempPassword();
        var hash         = _hasher.Hash(tempPassword);

        var advisor = User.CreateAdvisor(request.Email, hash, profile, advisorCode, request.LeaderId);

        await _users.AddAsync(advisor, cancellationToken);

        Console.WriteLine(
            $"[InsuraTech] Asesor {advisorCode} creado. Password temporal: {tempPassword}");

        return advisor.ToResponse();
    }

    private static string GenerateTempPassword()
    {
        const string upper   = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower   = "abcdefghijklmnopqrstuvwxyz";
        const string digits  = "0123456789";
        const string symbols = "!@#$%^&*";

        var bytes = RandomNumberGenerator.GetBytes(8);

        var chars = new char[]
        {
            upper  [bytes[0] % upper.Length],
            digits [bytes[1] % digits.Length],
            symbols[bytes[2] % symbols.Length],
            lower  [bytes[3] % lower.Length],
            lower  [bytes[4] % lower.Length],
            lower  [bytes[5] % lower.Length],
            lower  [bytes[6] % lower.Length],
            upper  [bytes[7] % upper.Length],
        };

        return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
    }
}
