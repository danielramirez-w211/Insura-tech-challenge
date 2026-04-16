using InsuraTech.Application.Claims.DTOs;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Interfaces;
using MediatR;

namespace InsuraTech.Application.Claims.Commands.RejectPendingClaim;

public sealed class RejectPendingClaimHandler : IRequestHandler<RejectPendingClaimCommand, ClaimResponse>
{
    private readonly IClaimRepository _claims;
    private readonly IUnitOfWork      _unitOfWork;

    public RejectPendingClaimHandler(IClaimRepository claims, IUnitOfWork unitOfWork)
    {
        _claims     = claims;
        _unitOfWork = unitOfWork;
    }

    public async Task<ClaimResponse> Handle(RejectPendingClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await _claims.GetByIdAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException($"Siniestro '{request.ClaimId}' no encontrado.");

        claim.RejectPending(request.Reason, request.ResponsibleUser);

        await _claims.UpdateAsync(claim, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return claim.ToResponse();
    }
}
