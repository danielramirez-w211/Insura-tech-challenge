namespace InsuraTech.Domain.Exceptions;

public sealed class TravelDurationInvalidException : DomainException
{
    public TravelDurationInvalidException()
        : base("TRAVEL_DURATION_INVALID",
               "La duración del viaje debe ser de al menos 1 día.") { }
}
