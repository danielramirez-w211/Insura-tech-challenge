using InsuraTech.Application.Auth.Services;
using InsuraTech.Domain.Users;
using Microsoft.Extensions.Logging;

namespace InsuraTech.Infrastructure.Persistence.Seeds;

/// <summary>
/// Crea el usuario Admin inicial si no existe ningún usuario en la colección.
/// Credenciales: admin@insuratech.com / Admin123!
/// </summary>
public static class AdminSeed
{
    private const string AdminEmail    = "admin@insuratech.com";
    private const string AdminPassword = "Admin123!";

    public static async Task SeedAsync(IUserRepository repository, IPasswordHasher hasher, ILogger logger)
    {
        if (await repository.ExistsAnyAsync())
            return;

        var hash  = hasher.Hash(AdminPassword);
        var admin = User.CreateAdmin(AdminEmail, hash);

        await repository.AddAsync(admin);

        logger.LogInformation(
            "[InsuraTech] Admin seed creado. Email: {Email} | Password: {Password}",
            AdminEmail, AdminPassword);
    }
}
