using  InsuraTech.Domain.Exceptions;


public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base("NOT_FOUND", message) { }
}