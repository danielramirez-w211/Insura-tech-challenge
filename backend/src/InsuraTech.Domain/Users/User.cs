using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Users;

/// <summary>
/// Aggregate de autenticación. Contiene credenciales y rol.
/// Los datos de perfil de negocio (nombre, foto, etc.) están en UserProfile.
/// </summary>
public sealed class User : Entity
{
    public string Email        { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public Role   Role         { get; private set; }
    public bool   IsActive     { get; private set; }

    /// <summary>Código único del asesor (ej. "0001"). Null para Admin y Leader.</summary>
    public string? AdvisorCode { get; private set; }

    /// <summary>Id del líder al que pertenece el asesor. Null para Admin y Leader.</summary>
    public Guid? LeaderId { get; private set; }

    public UserProfile Profile    { get; private set; } = UserProfile.Empty;
    public DateTime?   LastLoginAt { get; private set; }

    // ── Factories ────────────────────────────────────────────────────────────

    public static User CreateAdmin(string email, string passwordHash) =>
        new()
        {
            Email        = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role         = Role.Admin,
            IsActive     = true,
            Profile      = UserProfile.Empty
        };

    public static User CreateLeader(string email, string passwordHash, UserProfile profile) =>
        new()
        {
            Email        = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role         = Role.Leader,
            IsActive     = true,
            Profile      = profile
        };

    public static User CreateAdvisor(
        string email, string passwordHash, UserProfile profile,
        string advisorCode, Guid leaderId) =>
        new()
        {
            Email        = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role         = Role.Advisor,
            IsActive     = true,
            AdvisorCode  = advisorCode,
            LeaderId     = leaderId,
            Profile      = profile
        };

    // ── Behaviour ────────────────────────────────────────────────────────────

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void UpdateProfile(UserProfile profile)
    {
        Profile = profile;
        MarkAsUpdated();
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        MarkAsUpdated();
    }

    // ── EF-style private constructor for MongoDB deserialization ─────────────
    private User() { }
}
