using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Policies.DTOs;
using MediatR;

namespace InsuraTech.Application.Policies.Commands.CancelPolicy
{
    public sealed record CancelPolicyCommand : IRequest<PolicyResponse>
    {
        public Guid PolicyId { get; init; }
        public string Reason { get; init; } = null!;
        public DateOnly EffectiveDate { get; init; }
    }
}
