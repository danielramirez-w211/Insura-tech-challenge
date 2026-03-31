using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Domain.Exceptions
{
    public class InvalidPolicyStateException : DomainException
    {
        public InvalidPolicyStateException(string currentState, string attemptedActiion)
            : base("INVALID_POLICY_STATE", $"Cannot perform '{attemptedActiion} on a policy in state'{currentState}'.") {}
    }
}
