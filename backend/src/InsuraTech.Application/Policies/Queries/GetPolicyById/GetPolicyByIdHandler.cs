using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Domain.Exceptions;
using InsuraTech.Domain.Interfaces;
using MediatR;

namespace InsuraTech.Application.Policies.Queries.GetPolicyById
{
    public sealed class GetPolicyByIdHandler : IRequestHandler<GetPolicyByIdQuery, PolicyResponse>
    {
        private readonly IPolicyRepository _policyRepository;

        public GetPolicyByIdHandler(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository;
        }
        public async Task<PolicyResponse> Handle(GetPolicyByIdQuery request, CancellationToken cancellationToken)
        {
            var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken)
            ?? throw new NotFoundException($"Policy '{request.PolicyId}' was not found.");

            return policy.ToResponse();
        }

    }
}
