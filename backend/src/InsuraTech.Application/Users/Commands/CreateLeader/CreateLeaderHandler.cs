using System.Security.Cryptography;
using InsuraTech.Application.Auth.Services;
using InsuraTech.Application.Users.DTOs;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Users;
using MediatR;

namespace InsuraTech.Application.Users.Commands.CreateLeader;

public sealed class CreateLeaderHandler : IRequestHandler<CreateLeaderCommand, UserResponse>
{
    private readonly IUserRepository  _users;
    private readonly IPasswordHasher  _hasher;

    public CreateLeaderHandler(IUserRepository users, IPasswordHasher hasher)
    {
        _users  = users;
        _hasher = hasher;
    }

    public async Task<UserResponse> Handle(CreateLeaderCommand request, CancellationToken cancellationToken)
    {
        var existing = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new BusinessRuleException("EMAIL_DUPLICATE", "El email ya está registrado.");

        var tempPassword = GenerateTempPassword();
        var hash         = _hasher.Hash(tempPassword);

        var profile = new UserProfile(
            FirstName:      request.FirstName,
            LastName:        request.LastName,
            Nationality:     string.Empty,
            BirthDate:       DateOnly.MinValue,
            YearsInCompany:  0,
            PhotoUrl:        string.Empty,
            OfficeLocation:  request.OfficeLocation,
            WorkSchedule:    request.WorkSchedule);

        var leader = User.CreateLeader(request.Email, hash, profile);

        await _users.AddAsync(leader, cancellationToken);

        Console.WriteLine(
            $"[InsuraTech] Líder '{request.Email}' creado. Password temporal: {tempPassword}");

        return leader.ToResponse();
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

        // Mezcla los caracteres para que no haya un patrón predecible
        return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
    }
}
