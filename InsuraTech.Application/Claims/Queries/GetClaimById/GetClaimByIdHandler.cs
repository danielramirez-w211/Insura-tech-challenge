using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Claims.DTOs;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using MediatR;

namespace InsuraTech.Application.Claims.Queries.GetClaimById
{
    public sealed class GetClaimByIdHandler : IRequestHandler<GetClaimByIdQuery, ClaimResponse>
    {
        private readonly IClaimRepository _claimRepository;

        public GetClaimByIdHandler(IClaimRepository claimRepository)
        {
            _claimRepository = claimRepository;
        }
        public async Task<ClaimResponse> Handle(GetClaimByIdQuery request, CancellationToken cancellationToken)
        {
            var claim = await _claimRepository.GetByIdAsync(request.ClaimId, cancellationToken)
                ?? throw new NotFoundException($"Claim '{request.ClaimId}' was not found.");

            return claim.ToResponse();
        }
    }
}
