using InsuraTech.Application.Users.DTOs;
using MediatR;

namespace InsuraTech.Application.Users.Commands.CreateAdvisor;

public sealed record CreateAdvisorCommand : IRequest<UserResponse>
{
    public string   Email          { get; init; } = null!;
    public string   FirstName      { get; init; } = null!;
    public string   LastName       { get; init; } = null!;
    public string   Nationality    { get; init; } = null!;
    public string   BirthDate      { get; init; } = null!;
    public int      YearsInCompany { get; init; }
    public string   OfficeLocation { get; init; } = null!;
    public string   WorkSchedule   { get; init; } = null!;

    /// <summary>Id del líder autenticado — se inyecta en el controller desde el JWT.</summary>
    public Guid LeaderId { get; init; }
}
