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


namespace InsuraTech.Application.Policies.Commands.SuspendPolicy
{
    public sealed class SuspendPolicyHandler : IRequestHandler<SuspendPolicyCommand, PolicyResponse>
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SuspendPolicyHandler(IPolicyRepository policyRepository, IUnitOfWork unitOfWork)
        {
            _policyRepository = policyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PolicyResponse> Handle(
            SuspendPolicyCommand request,
            CancellationToken cancellationToken)
        {
            var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken)
                            ?? throw new NotFoundException($"Policy '{request.PolicyId}' was not found.");
                                    policy.Suspend(request.Reason);
            await _policyRepository.UpdateAsync(policy, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return policy.ToResponse();
        }
    }
}
