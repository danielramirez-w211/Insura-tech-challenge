using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Claims.Commands.StartInvestigation;
using InsuraTech.Application.Claims.DTOs;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using MediatR;

namespace InsuraTech.Application.Claims.Commands.RejectClaim
{
    public sealed class RejectClaimHandler : IRequestHandler<RejectClaimCommand, ClaimResponse>
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RejectClaimHandler(IClaimRepository claimRepository, IUnitOfWork unitOfWork)
        {
            _claimRepository = claimRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ClaimResponse> Handle(
            RejectClaimCommand request,
            CancellationToken cancellationToken)
        {
            var claim = await _claimRepository.GetByIdAsync(request.ClaimId)
                ?? throw new NotFoundException($"Claim '{request.ClaimId}' was not found.");

            claim.Reject(request.Reason, request.ResponsibleUser);

            return claim.ToResponse();
        }
          



    }
}
