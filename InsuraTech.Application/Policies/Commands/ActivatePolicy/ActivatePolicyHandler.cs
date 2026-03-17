namespace InsuraTech.Application.Policies.Commands.ActivatePolicy;

using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using MediatR;

public sealed class ActivatePolicyHandler : IRequestHandler<ActivatePolicyCommand, PolicyResponse>
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivatePolicyHandler(IPolicyRepository policyRepository, IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PolicyResponse> Handle(
        ActivatePolicyCommand request,
        CancellationToken cancellationToken)
    {
        var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken)
            ?? throw new NotFoundException($"Policy '{request.PolicyId}' was not found.");

        policy.Activate();

        await _policyRepository.UpdateAsync(policy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return policy.ToResponse();
    }
}