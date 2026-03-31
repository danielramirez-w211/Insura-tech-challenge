using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Policies.DTOs;
using MediatR;

namespace InsuraTech.Application.Policies.Queries.GetPolicyById
{
    public sealed record GetPolicyByIdQuery : IRequest<PolicyResponse>
    {
        public Guid PolicyId { get; init; }
    }
}
