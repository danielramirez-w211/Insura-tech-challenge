namespace InsuraTech.Application.TravelPlans.DTOs;

public sealed class TravelPlanCalculationDto
{
    public string TripType          { get; init; } = null!;
    public string? Continent        { get; init; }
    public int DurationDays         { get; init; }
    public decimal? BasePriceUsd    { get; init; }
    public decimal BasePriceCop     { get; init; }
    public decimal DailyIncrementCop { get; init; }
    public decimal TotalPriceCop    { get; init; }
    public decimal? TrmUsed         { get; init; }
    public DateOnly? TrmDate        { get; init; }
    public DateTime CalculatedAt    { get; init; }
}

public sealed class TravelPlanSelectionDto
{
    public string TripType           { get; init; } = null!;
    public string? Continent         { get; init; }
    public int DurationDays          { get; init; }
    public decimal? BasePriceUsd     { get; init; }
    public decimal BasePriceCop      { get; init; }
    public decimal DailyIncrementCop { get; init; }
    public decimal TotalPriceCop     { get; init; }
    public decimal? TrmUsed          { get; init; }
    public DateOnly? TrmDate         { get; init; }
    public DateTime CalculatedAt     { get; init; }
}
