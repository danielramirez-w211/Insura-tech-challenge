using InsuraTech.Application.Users.DTOs;
using MediatR;

namespace InsuraTech.Application.Users.Commands.UpdateProfile;

public sealed record UpdateProfileCommand : IRequest<UserResponse>
{
    /// <summary>Id del usuario autenticado (del JWT).</summary>
    public Guid   UserId         { get; init; }
    public string FirstName      { get; init; } = null!;
    public string LastName       { get; init; } = null!;
    public string Nationality    { get; init; } = null!;
    public string BirthDate      { get; init; } = null!;
    public int    YearsInCompany { get; init; }
    public string PhotoUrl       { get; init; } = null!;
    public string OfficeLocation { get; init; } = null!;
    public string WorkSchedule   { get; init; } = null!;
}
