using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies.ValueObjects;
using MediatR;

namespace InsuraTech.Application.Policies.Commands.RenewPolicy
{
    public sealed class RenewPolicyHandler : IRequestHandler<RenewPolicyCommand, PolicyResponse>
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RenewPolicyHandler(IPolicyRepository policyRepository, IUnitOfWork unitOfWork)
        {
            _policyRepository = policyRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<PolicyResponse> Handle(
        RenewPolicyCommand request,
        CancellationToken cancellationToken)
        {

            var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken)
                ?? throw new NotFoundException($"Policy '{request.PolicyId}' was not found. ");

            var sequence = await _policyRepository.GetNextSequenceAsync(cancellationToken);
            var newNumber = PolicyNumber.Create(DateTime.UtcNow.Year, sequence);
            var newCoverage = CoveragePeriod.Create(
            request.NewCoverageStartDate,
            request.NewCoverageEndDate);

            var renewal = policy.Renew(newNumber, newCoverage);



            await _policyRepository.AddAsync(renewal, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return renewal.ToResponse();
        }

    }
}
