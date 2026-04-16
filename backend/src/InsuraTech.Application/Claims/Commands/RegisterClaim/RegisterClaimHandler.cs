using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Claims.DTOs;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using MediatR;
using DomainClaim = InsuraTech.Domain.Claims.Claim;

namespace InsuraTech.Application.Claims.Commands.RegisterClaim
{
    public sealed class RegisterClaimHandler : IRequestHandler<RegisterClaimCommand, ClaimResponse>
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterClaimHandler(
            IClaimRepository claimRepository,
            IPolicyRepository policyRepository,
            IUnitOfWork unitOfWork)
        {
            _claimRepository = claimRepository;
            _policyRepository = policyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ClaimResponse> Handle(
            RegisterClaimCommand request,
            CancellationToken cancellationToken)
        {
            var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken)
                ?? throw new NotFoundException($"Policy '{request.PolicyId}' was not found.");

            if (!policy.IsActiveOn(DateOnly.FromDateTime(DateTime.UtcNow)))
                throw new BusinessRuleException("POLICY_NOT_ACTIVE",
                    $"Policy '{request.PolicyId}' is not active.");

            var openClaimsCount = await _claimRepository
                .CountOpenClaimsByPolicyIdAsync(request.PolicyId, cancellationToken);

            var claim = Claim.Register(
                request.PolicyId,
                request.Type,
                request.ClaimedAmount,
                request.IncidentDate,
                request.Description,
                policy.Coverage.StartDate,
                policy.Coverage.EndDate,
                policy.AvailableInsuredAmount,
                openClaimsCount,
                request.ResponsibleUser,
                request.CreatedByAdvisorId);

            await _claimRepository.AddAsync(claim, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return claim.ToResponse();
        }
    }
}
