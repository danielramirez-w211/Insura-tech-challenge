using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Domain.Common.ActivatePolicy
{
    public sealed record ActivatePolicyCommand : IRequest<PolicyResponse>
    {
        public Guid PolicyId { get; init; }
    }
}
