using InsuraTech.Application.TravelPlans.DTOs;
using InsuraTech.Domain.Policies.TravelPlan;
using MediatR;

namespace InsuraTech.Application.TravelPlans.Queries.CalculateTravelPlan;

public sealed record CalculateTravelPlanQuery : IRequest<TravelPlanCalculationDto>
{
    public TripType TripType    { get; init; }
    public Continent? Continent { get; init; }
    public int DurationDays     { get; init; }
}
