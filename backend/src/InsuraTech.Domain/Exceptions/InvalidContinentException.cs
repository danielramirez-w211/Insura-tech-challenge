namespace InsuraTech.Domain.Exceptions;

public sealed class InvalidContinentException : DomainException
{
    public InvalidContinentException(string continentId)
        : base("INVALID_CONTINENT",
               $"El continente '{continentId}' no es válido. " +
               "Los continentes soportados son: America, Europe, Africa, Asia, Oceania.") { }
}
