namespace InsuraTech.Domain.Exceptions;

public sealed class InvalidHealthPlanException : DomainException
{
    public InvalidHealthPlanException(string planId)
        : base("INVALID_HEALTH_PLAN",
               $"El plan de salud '{planId}' no existe. " +
               "Planes válidos: basic, salud-global, salud-premium, salud-vida-total.")
    { }
}
