using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Claims.DTOs;
using MediatR;

namespace InsuraTech.Application.Claims.Queries.GetClaimsByPolicy
{
    public sealed record GetClaimsByPolicyQuery : IRequest<IEnumerable<ClaimResponse>>
    {
        public Guid PolicyId { get; init; }

    }
}
