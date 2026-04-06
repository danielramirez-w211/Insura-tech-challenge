using InsuraTech.Application.Claims.DTOs;
using InsuraTech.Application.Common.Models;
using InsuraTech.Domain.Interfaces;
using MediatR;

namespace InsuraTech.Application.Claims.Queries.GetClaims;

public sealed class GetClaimsHandler : IRequestHandler<GetClaimsQuery, PagedResult<ClaimResponse>>
{
    private readonly IClaimRepository _repository;

    public GetClaimsHandler(IClaimRepository repository) => _repository = repository;

    public async Task<PagedResult<ClaimResponse>> Handle(GetClaimsQuery request, CancellationToken cancellationToken)
    {
        var total = await _repository.CountAsync(request.Status, request.PolicyId, cancellationToken);
        if (total == 0)
            return PagedResult<ClaimResponse>.Empty(request.Page, request.PageSize);

        var items = await _repository.GetAllAsync(
            request.Status, request.PolicyId, request.Page, request.PageSize, cancellationToken);

        return new PagedResult<ClaimResponse>(
            items.Select(c => c.ToResponse()),
            total,
            request.Page,
            request.PageSize);
    }
}
