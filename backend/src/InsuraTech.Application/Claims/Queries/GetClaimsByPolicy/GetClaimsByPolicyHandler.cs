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

namespace InsuraTech.Application.Claims.Queries.GetClaimsByPolicy
{
    public sealed class GetClaimsByPolicyHandler : IRequestHandler<GetClaimsByPolicyQuery, IEnumerable<ClaimResponse>>
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IPolicyRepository _policyRepository;

        public GetClaimsByPolicyHandler(IClaimRepository claimRepository, IPolicyRepository policyRepository)
        {
            _claimRepository = claimRepository;
            _policyRepository = policyRepository;
        }

        public async Task<IEnumerable<ClaimResponse>> Handle(
            GetClaimsByPolicyQuery request,
            CancellationToken cancellationToken)
        {
            _ = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken)
                ?? throw new NotFoundException($"Policy '{request.PolicyId}' was not found. ");

            var claims = await _claimRepository.GetByPolicyIdAsync(request.PolicyId, cancellationToken);
            return claims.Select(c => c.ToResponse());
        }
    }
}
