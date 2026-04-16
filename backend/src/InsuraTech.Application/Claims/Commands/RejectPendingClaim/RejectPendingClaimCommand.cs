using InsuraTech.Application.Claims.DTOs;
using MediatR;

namespace InsuraTech.Application.Claims.Commands.RejectPendingClaim;

public sealed record RejectPendingClaimCommand : IRequest<ClaimResponse>
{
    public Guid   ClaimId         { get; init; }
    public string Reason          { get; init; } = null!;
    /// <summary>Nombre/email del líder que rechaza — se inyecta desde el JWT en el controller.</summary>
    public string ResponsibleUser { get; init; } = null!;
}
