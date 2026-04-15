namespace InsuraTech.Domain.Exceptions;

public sealed class OverageLifeInsuredException : DomainException
{
    public OverageLifeInsuredException()
        : base("OVERAGE_LIFE_INSURED",
               "Los planes de Vida no están disponibles para asegurados mayores de 65 años.")
    { }
}
