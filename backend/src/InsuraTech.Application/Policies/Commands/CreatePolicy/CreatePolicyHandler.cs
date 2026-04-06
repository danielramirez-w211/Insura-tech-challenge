using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using MediatR;

namespace InsuraTech.Application.Policies.Commands.CreatePolicy
{
    public sealed class CreatePolicyHandler : IRequestHandler<CreatePolicyCommand, PolicyResponse>
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePolicyHandler(IPolicyRepository policyRepository, IUnitOfWork unitOfWork)
        {
            _policyRepository = policyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PolicyResponse> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
        {
            var existing = await _policyRepository
            .GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);

            if (existing is not null)
                return existing.ToResponse();

            // Generar número de póliza
            var sequence = await _policyRepository.GetNextSequenceAsync(cancellationToken);
            var policyNumber = PolicyNumber.Create(DateTime.UtcNow.Year, sequence);

            var insured = InsuredPerson.Create(
                request.InsuredFullName,
                request.InsuredDocumentId,
                request.InsuredBirthDate);

            var coverage = CoveragePeriod.Create(
                request.CoverageStartDate,
                request.CoverageEndDate);

            var policy = request.Type == PolicyType.Health && !string.IsNullOrWhiteSpace(request.HealthPlanId)
                ? Policy.CreateHealthPolicy(
                    policyNumber,
                    insured,
                    coverage,
                    request.MonthlyPremium,
                    request.HealthPlanId,
                    DateOnly.FromDateTime(DateTime.UtcNow))
                : Policy.Create(
                    policyNumber,
                    request.Type,
                    insured,
                    coverage,
                    request.MonthlyPremium,
                    request.InsuredAmount);

            await _policyRepository.AddAsync(policy, cancellationToken);
            _policyRepository.SetIdempotencyKey(policy, request.IdempotencyKey);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return policy.ToResponse();


        }
    }
}
