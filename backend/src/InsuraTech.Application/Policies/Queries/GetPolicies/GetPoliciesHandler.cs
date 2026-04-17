using InsuraTech.Application.Common.Models;
using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Domain.Interfaces;
using MediatR;

namespace InsuraTech.Application.Policies.Queries.GetPolicies
{
    public sealed class GetPoliciesHandler : IRequestHandler<GetPoliciesQuery, PagedResult<PolicyResponse>>
    {
        private readonly IPolicyRepository _policyRepository;

        public GetPoliciesHandler(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository;
        }
        public async Task<PagedResult<PolicyResponse>> Handle( GetPoliciesQuery request, CancellationToken cancellationToken)
        {
            var policies = await _policyRepository.GetAllAsync(
                request.Status,
                request.Type,
                request.DocumentId,
                request.StartDate,
                request.EndDate,
                request.InsuredSearch,
                request.InsuredDocumentType,
                request.Page,
                request.PageSize,
                cancellationToken
                );

            var totalCount = await _policyRepository.CountAsync(
                request.Status,
                request.Type,
                request.DocumentId,
                request.StartDate,
                request.EndDate,
                request.InsuredSearch,
                request.InsuredDocumentType,
                cancellationToken);

            var item = policies.Select(p => p.ToResponse());
            return new PagedResult<PolicyResponse>(item, totalCount, request.Page, request.PageSize);
        }
    }
}
