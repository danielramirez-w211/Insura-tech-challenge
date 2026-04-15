namespace InsuraTech.Domain.Exceptions;

public sealed class InvalidVehiclePlanException : DomainException
{
    public InvalidVehiclePlanException(string planId)
        : base("INVALID_VEHICLE_PLAN",
               $"El plan vehicular '{planId}' no existe. " +
               "Planes válidos: standard, complete, premium.")
    { }
}
