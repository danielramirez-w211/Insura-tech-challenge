namespace InsuraTech.Domain.Exceptions;

public sealed class TravelDurationExceededException : DomainException
{
    public TravelDurationExceededException()
        : base("TRAVEL_DURATION_EXCEEDED",
               "La duración máxima de un seguro de viaje es de 180 días (6 meses). " +
               "Para continuidad de cobertura, contrate una nueva póliza a partir del día 181.") { }
}
