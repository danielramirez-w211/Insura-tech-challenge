using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Policies.TravelPlan;

public sealed class TravelPlanSelection : ValueObject
{
    public TripType TripType { get; }
    public Continent? Continent { get; }
    public int DurationDays { get; }
    public decimal? BasePriceUsd { get; }
    public decimal BasePriceCop { get; }
    public decimal DailyIncrementCop { get; }
    public decimal TotalPriceCop { get; }
    public decimal? TrmUsed { get; }
    public DateOnly? TrmDate { get; }
    public DateTime CalculatedAt { get; }

    public TravelPlanSelection(
        TripType tripType,
        Continent? continent,
        int durationDays,
        decimal? basePriceUsd,
        decimal basePriceCop,
        decimal dailyIncrementCop,
        decimal totalPriceCop,
        decimal? trmUsed,
        DateOnly? trmDate,
        DateTime calculatedAt)
    {
        TripType = tripType;
        Continent = continent;
        DurationDays = durationDays;
        BasePriceUsd = basePriceUsd;
        BasePriceCop = basePriceCop;
        DailyIncrementCop = dailyIncrementCop;
        TotalPriceCop = totalPriceCop;
        TrmUsed = trmUsed;
        TrmDate = trmDate;
        CalculatedAt = calculatedAt;
    }

    protected override IEnumerable<object> GetEqualityComponets()
    {
        yield return TripType;
        yield return DurationDays;
        yield return TotalPriceCop;
        yield return CalculatedAt;
    }
}
