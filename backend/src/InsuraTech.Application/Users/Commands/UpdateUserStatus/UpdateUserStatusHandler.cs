using InsuraTech.Application.Users.DTOs;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Users;
using MediatR;

namespace InsuraTech.Application.Users.Commands.UpdateUserStatus;

public sealed class UpdateUserStatusHandler : IRequestHandler<UpdateUserStatusCommand, UserResponse>
{
    private readonly IUserRepository _users;

    public UpdateUserStatusHandler(IUserRepository users) => _users = users;

    public async Task<UserResponse> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        var target = await _users.GetByIdAsync(request.TargetUserId, cancellationToken)
            ?? throw new NotFoundException($"Usuario '{request.TargetUserId}' no encontrado.");

        // Admin puede gestionar cualquier Leader.
        // Leader solo puede gestionar sus propios Asesores.
        if (request.RequesterRole == "Leader")
        {
            if (target.Role != Role.Advisor)
                throw new BusinessRuleException("FORBIDDEN",
                    "Un Líder solo puede activar/desactivar Asesores.");

            if (target.LeaderId != request.RequesterId)
                throw new BusinessRuleException("FORBIDDEN",
                    "No tiene permisos para gestionar este asesor.");
        }

        if (request.IsActive)
            target.Activate();
        else
            target.Deactivate();

        await _users.UpdateAsync(target, cancellationToken);

        return target.ToResponse();
    }
}
