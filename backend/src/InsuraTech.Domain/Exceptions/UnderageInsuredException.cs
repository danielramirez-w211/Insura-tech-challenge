namespace InsuraTech.Domain.Exceptions;

public sealed class UnderageInsuredException : DomainException
{
    public UnderageInsuredException()
        : base("UNDERAGE_INSURED",
               "El asegurado debe tener al menos 18 años para contratar un plan de salud.")
    { }
}
