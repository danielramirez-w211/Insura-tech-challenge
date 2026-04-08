using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Application.TravelPlans.DTOs;
using InsuraTech.Domain.Policies.TravelPlan;
using MediatR;

namespace InsuraTech.Application.TravelPlans.Queries.CalculateTravelPlan;

public sealed class CalculateTravelPlanHandler
    : IRequestHandler<CalculateTravelPlanQuery, TravelPlanCalculationDto>
{
    private readonly ITrmService _trmService;

    public CalculateTravelPlanHandler(ITrmService trmService) => _trmService = trmService;

    public async Task<TravelPlanCalculationDto> Handle(
        CalculateTravelPlanQuery request,
        CancellationToken cancellationToken)
    {
        decimal? trmValue = null;
        DateOnly? trmDate = null;

        if (request.TripType == TripType.Internacional)
        {
            var trm  = await _trmService.GetCurrentTrmAsync(cancellationToken);
            trmValue = trm.ValueCop;
            trmDate  = trm.Date;
        }

        var selection = TravelRatingService.Calculate(
            request.TripType,
            request.Continent,
            request.DurationDays,
            trmValue,
            trmDate,
            DateTime.UtcNow);

        return new TravelPlanCalculationDto
        {
            TripType          = selection.TripType.ToString(),
            Continent         = selection.Continent?.ToString(),
            DurationDays      = selection.DurationDays,
            BasePriceUsd      = selection.BasePriceUsd,
            BasePriceCop      = selection.BasePriceCop,
            DailyIncrementCop = selection.DailyIncrementCop,
            TotalPriceCop     = selection.TotalPriceCop,
            TrmUsed           = selection.TrmUsed,
            TrmDate           = selection.TrmDate,
            CalculatedAt      = selection.CalculatedAt
        };
    }
}
