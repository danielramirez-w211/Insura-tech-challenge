using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Domain.Exceptions
{
    public class ClaimLimitExceededException : DomainException
    {
        public ClaimLimitExceededException(Guid policyId) 
                : base("CLAIM_LIMIT_EXCEEDED", $"Policy '{policyId}'Already has 3 open claims, Cannot register a new one. ") { }
    }
}
