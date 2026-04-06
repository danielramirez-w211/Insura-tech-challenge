using InsuraTech.Application.Claims.DTOs;
using InsuraTech.Application.Common.Models;
using InsuraTech.Domain.Claims;
using MediatR;

namespace InsuraTech.Application.Claims.Queries.GetClaims;

public sealed record GetClaimsQuery : IRequest<PagedResult<ClaimResponse>>
{
    public ClaimStatus? Status { get; init; }
    public Guid? PolicyId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
