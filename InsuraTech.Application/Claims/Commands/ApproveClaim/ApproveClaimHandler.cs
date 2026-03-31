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

namespace InsuraTech.Application.Claims.Commands.ApproveClaim
{
    public sealed class ApproveClaimHandler : IRequestHandler<ApproveClaimCommand, ClaimResponse>
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ApproveClaimHandler(IClaimRepository claimRepository, IPolicyRepository policyRepository, IUnitOfWork unitOfWork)
        {
            _claimRepository = claimRepository;
            _policyRepository = policyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ClaimResponse>Handle(
            ApproveClaimCommand request,
            CancellationToken cancellationToken )
        {
            var claim = await _claimRepository.GetByIdAsync(request.ClaimId, cancellationToken)
                ?? throw new NotFoundException($"Claim '{request.ClaimId}' was not found.");

            var policy = await _policyRepository.GetByIdAsync(claim.PolicyId, cancellationToken)
                ?? throw new NotFoundException($"Policy '{claim.PolicyId}' was not found.");

            claim.Approve(request.ApprovedAmount, request.ResponsibleUser, request.Observations);

            policy.DeductInsuredAmount(request.ApprovedAmount);

            await _claimRepository.UpdateAsync(claim, cancellationToken);
            await _policyRepository.UpdateAsync(policy, cancellationToken);            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return claim.ToResponse();

        }
    }
}
