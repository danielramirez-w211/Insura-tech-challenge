using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Common.Models;
using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Domain.Policies;
using MediatR;

namespace InsuraTech.Application.Policies.Queries.GetPolicies
{
    public sealed record GetPoliciesQuery : IRequest<PagedResult<PolicyResponse>>
    {
        public PolicyStatus? Status { get; init; }
        public PolicyType? Type { get; init; }
        public string? DocumentId { get; init; }
        public DateOnly? StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        public string? InsuredSearch { get; init; }
        public string? InsuredDocumentType { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
