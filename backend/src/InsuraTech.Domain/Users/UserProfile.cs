namespace InsuraTech.Domain.Users;

/// <summary>
/// Datos de perfil de negocio del usuario. Embebido dentro del documento User en MongoDB.
/// </summary>
public sealed record UserProfile(
    string  FirstName,
    string  LastName,
    string  Nationality,
    DateOnly BirthDate,
    int     YearsInCompany,
    string  PhotoUrl,
    string  OfficeLocation,
    string  WorkSchedule)
{
    public static readonly UserProfile Empty = new(
        FirstName:      string.Empty,
        LastName:       string.Empty,
        Nationality:    string.Empty,
        BirthDate:      DateOnly.MinValue,
        YearsInCompany: 0,
        PhotoUrl:       string.Empty,
        OfficeLocation: string.Empty,
        WorkSchedule:   string.Empty);
}
