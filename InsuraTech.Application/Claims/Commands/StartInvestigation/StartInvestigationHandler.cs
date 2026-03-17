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

namespace InsuraTech.Application.Claims.Commands.StartInvestigation
{
    public sealed class StartInvestigationHandler : IRequestHandler<StartInvestigationCommand, ClaimResponse>
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IUnitOfWork _unitOfWork;

        public StartInvestigationHandler(IClaimRepository claimRepository, IUnitOfWork unitOfWork)
        {
            _claimRepository = claimRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ClaimResponse> Handle(
            StartInvestigationCommand request,
            CancellationToken cancellationToken)
        {
            var claim = await _claimRepository.GetByIdAsync(request.ClaimId, cancellationToken)
                ?? throw new NotFoundException($"Claim '{request.ClaimId}' was not found.");

            claim.StartInvestigation(request.ResponsibleUser, request.Observations);

            await _claimRepository.UpdateAsync(claim, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return claim.ToResponse();
        }
    }
}
