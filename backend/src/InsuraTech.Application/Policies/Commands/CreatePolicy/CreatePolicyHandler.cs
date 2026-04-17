using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Application.Policies.Commands.CreatePolicy.Strategies;
using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using MediatR;

namespace InsuraTech.Application.Policies.Commands.CreatePolicy;

public sealed class CreatePolicyHandler : IRequestHandler<CreatePolicyCommand, PolicyResponse>
{
    private readonly IPolicyRepository              _policyRepository;
    private readonly IUnitOfWork                    _unitOfWork;
    private readonly IEnumerable<ICreatePolicyStrategy> _strategies;

    public CreatePolicyHandler(
        IPolicyRepository policyRepository,
        IUnitOfWork unitOfWork,
        IEnumerable<ICreatePolicyStrategy> strategies)
    {
        _policyRepository = policyRepository;
        _unitOfWork       = unitOfWork;
        _strategies       = strategies;
    }

    public async Task<PolicyResponse> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
    {
        var existing = await _policyRepository
            .GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);

        if (existing is not null)
            return existing.ToResponse();

        var sequence     = await _policyRepository.GetNextSequenceAsync(cancellationToken);
        var policyNumber = PolicyNumber.Create(DateTime.UtcNow.Year, sequence);

        var insured = InsuredPerson.Create(
            request.InsuredFirstName, request.InsuredLastName,
            request.InsuredDocumentType, request.InsuredDocumentId,
            request.InsuredBirthDate,
            request.InsuredGender, request.InsuredAddress,
            request.InsuredCityName, request.InsuredPostalCode,
            request.InsuredDepartment);

        var strategy = _strategies.FirstOrDefault(s => s.Type == request.Type && s.CanHandle(request));

        Policy policy;
        if (strategy is not null)
        {
            policy = await strategy.CreateAsync(request, policyNumber, insured, cancellationToken);
        }
        else
        {
            var coverage = CoveragePeriod.Create(request.CoverageStartDate, request.CoverageEndDate);
            policy = Policy.Create(policyNumber, request.Type, insured, coverage,
                request.MonthlyPremium, request.InsuredAmount);
        }

        if (request.CreatedByAdvisorId.HasValue)
            policy.SetCreatedByAdvisor(request.CreatedByAdvisorId.Value);

        await _policyRepository.AddAsync(policy, cancellationToken);
        _policyRepository.SetIdempotencyKey(policy, request.IdempotencyKey);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return policy.ToResponse();
    }
}
