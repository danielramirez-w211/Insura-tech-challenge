namespace InsuraTech.Domain.Exceptions;

public sealed class InvalidLifePlanException : DomainException
{
    public InvalidLifePlanException(string planId)
        : base("INVALID_LIFE_PLAN",
               $"El plan de vida '{planId}' no existe. " +
               "Planes válidos: plan-vida, vida-familia, vida-premium.")
    { }
}
