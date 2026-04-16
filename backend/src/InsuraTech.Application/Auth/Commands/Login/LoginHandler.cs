using InsuraTech.Application.Auth.DTOs;
using InsuraTech.Application.Auth.Services;
using InsuraTech.Domain.Users;
using MediatR;

namespace InsuraTech.Application.Auth.Commands.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _users;
    private readonly IJwtService     _jwt;
    private readonly IPasswordHasher _hasher;

    public LoginHandler(IUserRepository users, IJwtService jwt, IPasswordHasher hasher)
    {
        _users  = users;
        _jwt    = jwt;
        _hasher = hasher;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("La cuenta está desactivada.");

        if (!_hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        user.RecordLogin();
        await _users.UpdateAsync(user, cancellationToken);

        var token     = _jwt.GenerateToken(user);
        var expiresAt = _jwt.GetExpiration();

        return new LoginResponse(
            Token:       token,
            Role:        user.Role.ToString(),
            UserId:      user.Id.ToString(),
            Email:       user.Email,
            FirstName:   user.Profile.FirstName,
            LastName:    user.Profile.LastName,
            AdvisorCode: user.AdvisorCode,
            ExpiresAt:   expiresAt);
    }
}
