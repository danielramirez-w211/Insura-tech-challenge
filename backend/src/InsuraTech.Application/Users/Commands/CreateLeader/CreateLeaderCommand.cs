using InsuraTech.Application.Users.DTOs;
using MediatR;

namespace InsuraTech.Application.Users.Commands.CreateLeader;

public sealed record CreateLeaderCommand : IRequest<UserResponse>
{
    public string  Email          { get; init; } = null!;
    public string  FirstName      { get; init; } = null!;
    public string  LastName       { get; init; } = null!;
    public string  OfficeLocation { get; init; } = null!;
    public string  WorkSchedule   { get; init; } = null!;
}
