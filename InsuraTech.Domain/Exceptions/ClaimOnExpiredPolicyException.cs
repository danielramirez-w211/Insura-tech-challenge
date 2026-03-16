namespace InsuraTech.Domain.Exceptions;

public class ClaimOnExpiredPolicyException : DomainException
{
    public ClaimOnExpiredPolicyException(Guid policyId, DateOnly incidentDate)
        : base("CLAIM_ON_EXPIRED_POLICY",
               $"Cannot register a claim on policy '{policyId}' because incident date '{incidentDate}' is outside the coverage period.")
    { }
}