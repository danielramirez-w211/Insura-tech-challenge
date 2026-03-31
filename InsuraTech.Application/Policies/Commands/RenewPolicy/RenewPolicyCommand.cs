using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Policies.DTOs;
using MediatR;

namespace InsuraTech.Application.Policies.Commands.RenewPolicy
{
    public sealed record RenewPolicyCommand : IRequest<PolicyResponse>
    {
        public Guid PolicyId { get; init; }
        public DateOnly NewCoverageStartDate { get; init; }
        public DateOnly NewCoverageEndDate { get; init; }
    }
}
