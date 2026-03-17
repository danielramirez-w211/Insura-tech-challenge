using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using MediatR;

namespace InsuraTech.Application.Policies.Commands.CancelPolicy
{
    public sealed class CancelPolicyHandler : IRequestHandler<CancelPolicyCommand, PolicyResponse>
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CancelPolicyHandler(IPolicyRepository policyRepository, IUnitOfWork unitOfWork)
        {
            _policyRepository = policyRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<PolicyResponse> Handle(
        CancelPolicyCommand request,
        CancellationToken cancellationToken)
        {
            var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken)
                ?? throw new NotFoundException($"Policy '{request.PolicyId}' was not found.");

            policy.Cancel(request.Reason, request.EffectiveDate);
            
            await _policyRepository.UpdateAsync(policy);
            await _unitOfWork.SaveChangesAsync();

            return policy.ToResponse();


        }
    }
}
