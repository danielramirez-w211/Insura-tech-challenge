using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InsuraTech.Application.Auth.Services;
using InsuraTech.Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InsuraTech.Infrastructure.Auth;

public sealed class JwtService : IJwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int    _expirationHours;

    public JwtService(IConfiguration configuration)
    {
        _secret          = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret not configured.");
        _issuer          = configuration["Jwt:Issuer"]          ?? "InsuraTech";
        _audience        = configuration["Jwt:Audience"]        ?? "InsuraTech.Client";
        _expirationHours = int.TryParse(configuration["Jwt:ExpirationHours"], out var h) ? h : 8;
    }

    public string GenerateToken(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email,          user.Email),
            new(ClaimTypes.Role,           user.Role.ToString()),
            new("advisorCode",             user.AdvisorCode ?? string.Empty),
            new("leaderId",                user.LeaderId?.ToString() ?? string.Empty),
            new("firstName",               user.Profile.FirstName),
            new("lastName",                user.Profile.LastName),
        };

        var token = new JwtSecurityToken(
            issuer:             _issuer,
            audience:           _audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(_expirationHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetExpiration() => DateTime.UtcNow.AddHours(_expirationHours);
}
