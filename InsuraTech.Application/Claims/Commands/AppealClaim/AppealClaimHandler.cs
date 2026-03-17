using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Claims.DTOs;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using MediatR;

namespace InsuraTech.Application.Claims.Commands.AppealClaim
{
    public sealed class AppealClaimHandler : IRequestHandler<AppealClaimCommand, ClaimResponse>
    {
        public readonly IClaimRepository _claimRepository;
        public readonly IUnitOfWork _unitOfWork;

        public AppealClaimHandler(IClaimRepository claimRepository, IUnitOfWork unitOfWork)
        {
            _claimRepository = claimRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ClaimResponse> Handle(
            AppealClaimCommand request,
            CancellationToken cancellationToken)
        {
            var claim = await _claimRepository.GetByIdAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException($"Claim '{request.ClaimId}' was not found.");


            claim.Appeal(request.ResponsibleUser, request.Observations);

            await _claimRepository.UpdateAsync(claim, cancellationToken);
            await _unitOfWork.SaveChangesAsync();

            return claim.ToResponse();
        }

    }
}
