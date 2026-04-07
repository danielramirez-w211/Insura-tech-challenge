namespace InsuraTech.Domain.Exceptions;

public sealed class TrmUnavailableException : DomainException
{
    public TrmUnavailableException()
        : base("TRM_UNAVAILABLE",
               "La Tasa de Cambio Representativa del Mercado (TRM) no está disponible en este momento. " +
               "Intente de nuevo más tarde.") { }
}
