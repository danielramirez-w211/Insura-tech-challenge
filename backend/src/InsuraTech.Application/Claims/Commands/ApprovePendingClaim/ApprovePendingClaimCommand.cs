using InsuraTech.Application.Claims.DTOs;
using MediatR;

namespace InsuraTech.Application.Claims.Commands.ApprovePendingClaim;

public sealed record ApprovePendingClaimCommand : IRequest<ClaimResponse>
{
    public Guid   ClaimId         { get; init; }
    /// <summary>Nombre/email del líder que aprueba — se inyecta desde el JWT en el controller.</summary>
    public string ResponsibleUser { get; init; } = null!;
}
