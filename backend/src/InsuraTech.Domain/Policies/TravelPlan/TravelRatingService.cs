using InsuraTech.Domain.Exceptions;

namespace InsuraTech.Domain.Policies.TravelPlan;

public static class TravelRatingService
{
    private const decimal NacionalBasePrice      = 2_200m;
    private const decimal NacionalDailyIncrement = 1_200m;
    private const int     NacionalCalculationCap = 30;
    private const int     MaxDurationDays        = 365;

    private const decimal InternacionalBasePriceUsd      = 30m;
    private const decimal InternacionalDailyIncrementUsd = 5m;

    /// <summary>
    /// Calcula el plan de viaje de forma determinística.
    /// Para Internacional, trmCop y trmDate deben ser no nulos (obtenidos de ITrmService).
    /// </summary>
    public static TravelPlanSelection Calculate(
        TripType tripType,
        Continent? continent,
        int durationDays,
        decimal? trmCop,
        DateOnly? trmDate,
        DateTime calculatedAt)
    {
        if (durationDays < 1)
            throw new TravelDurationInvalidException();
        if (durationDays > MaxDurationDays)
            throw new TravelDurationExceededException();

        return tripType == TripType.Internacional
            ? CalculateInternacional(continent, durationDays, trmCop, trmDate, calculatedAt)
            : CalculateNacional(durationDays, calculatedAt);
    }

    private static TravelPlanSelection CalculateNacional(int durationDays, DateTime calculatedAt)
    {
        var effectiveDays = Math.Min(durationDays, NacionalCalculationCap);
        var total         = NacionalBasePrice + (effectiveDays - 1) * NacionalDailyIncrement;

        return new TravelPlanSelection(
            tripType:         TripType.Nacional,
            continent:        null,
            durationDays:     durationDays,
            basePriceUsd:     null,
            basePriceCop:     NacionalBasePrice,
            dailyIncrementCop: NacionalDailyIncrement,
            totalPriceCop:    total,
            trmUsed:          null,
            trmDate:          null,
            calculatedAt:     calculatedAt);
    }

    private static TravelPlanSelection CalculateInternacional(
        Continent? continent,
        int durationDays,
        decimal? trmCop,
        DateOnly? trmDate,
        DateTime calculatedAt)
    {
        if (continent is null)
            throw new InvalidContinentException("null");

        if (trmCop is null || trmCop <= 0)
            throw new TrmUnavailableException();

        var totalUsd     = InternacionalBasePriceUsd + (durationDays - 1) * InternacionalDailyIncrementUsd;
        var totalCop     = Math.Round(totalUsd * trmCop.Value, 0, MidpointRounding.AwayFromZero);
        var baseCop      = Math.Round(InternacionalBasePriceUsd * trmCop.Value, 0, MidpointRounding.AwayFromZero);
        var incrementCop = Math.Round(InternacionalDailyIncrementUsd * trmCop.Value, 0, MidpointRounding.AwayFromZero);

        return new TravelPlanSelection(
            tripType:         TripType.Internacional,
            continent:        continent,
            durationDays:     durationDays,
            basePriceUsd:     InternacionalBasePriceUsd,
            basePriceCop:     baseCop,
            dailyIncrementCop: incrementCop,
            totalPriceCop:    totalCop,
            trmUsed:          trmCop,
            trmDate:          trmDate,
            calculatedAt:     calculatedAt);
    }
}
