namespace InsuraTech.Application.Common.Interfaces;

public sealed record TrmResult(decimal ValueCop, DateOnly Date);

public interface ITrmService
{
    /// <summary>
    /// Retorna la TRM vigente del día.
    /// Lanza <see cref="InsuraTech.Domain.Exceptions.TrmUnavailableException"/> si la API falla.
    /// Resultado puede venir del caché (TTL 1 hora).
    /// </summary>
    Task<TrmResult> GetCurrentTrmAsync(CancellationToken cancellationToken = default);
}
