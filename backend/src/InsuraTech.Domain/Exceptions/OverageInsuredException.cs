namespace InsuraTech.Domain.Exceptions;

public sealed class OverageInsuredException : DomainException
{
    public OverageInsuredException()
        : base("OVERAGE_INSURED",
               "Los asegurados de 74 años o más requieren evaluación especial de preexistencias. " +
               "El flujo estándar de creación de pólizas de salud no está disponible para este rango etario.")
    { }
}
