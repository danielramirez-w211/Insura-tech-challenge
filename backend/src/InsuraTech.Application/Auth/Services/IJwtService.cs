using InsuraTech.Domain.Users;

namespace InsuraTech.Application.Auth.Services;

public interface IJwtService
{
    string   GenerateToken(User user);
    DateTime GetExpiration();
}
