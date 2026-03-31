using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Domain.Exceptions;

    public class InvalidClaimStateException : DomainException
{
    public InvalidClaimStateException(string currentState, string attemptedAction)
        : base("INVALID_CLAIM_STATE",
            $"Cannot perform '{attemptedAction}' on a claim in state '{currentState}'. ")

    { }
}

