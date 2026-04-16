using InsuraTech.Application.Users.DTOs;
using MediatR;

namespace InsuraTech.Application.Users.Commands.UpdateUserStatus;

public sealed record UpdateUserStatusCommand : IRequest<UserResponse>
{
    public Guid   TargetUserId    { get; init; }
    public bool   IsActive        { get; init; }

    /// <summary>Id del usuario que realiza la acción (del JWT). El handler valida permisos de ownership.</summary>
    public Guid   RequesterId     { get; init; }
    public string RequesterRole   { get; init; } = null!;
    public Guid?  RequesterLeaderId { get; init; }
}
