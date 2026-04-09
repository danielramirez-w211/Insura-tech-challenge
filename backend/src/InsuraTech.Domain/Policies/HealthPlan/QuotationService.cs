namespace InsuraTech.Domain.Policies.HealthPlan;

/// <summary>
/// Centraliza la lógica de cotización para el periodo fijo de 12 meses.
/// Reutilizable por los módulos Health, Life y Vehicle.
/// </summary>
public static class QuotationService
{
    /// <summary>Duración fija de la cobertura en días (1 año).</summary>
    public const int FixedDurationDays = 365;

    /// <summary>
    /// Calcula la prima mensual dividiendo el monto anual entre 12,
    /// redondeando al entero más cercano (half-up).
    /// </summary>
    /// <param name="annualAmount">Monto total anual asegurado.</param>
    /// <returns>Prima mensual expresada en la misma moneda que <paramref name="annualAmount"/>.</returns>
    public static decimal CalculateMonthlyPremium(decimal annualAmount)
        => Math.Round(annualAmount / 12m, 0, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Calcula la fecha de fin de cobertura sumando <see cref="FixedDurationDays"/> días
    /// a la fecha de inicio (el último día de cobertura es inclusivo).
    /// </summary>
    public static DateOnly CalculateEndDate(DateOnly startDate)
        => startDate.AddDays(FixedDurationDays - 1);
}
