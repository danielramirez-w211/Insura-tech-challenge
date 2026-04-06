using InsuraTech.Application.HealthPlans.DTOs;
using MediatR;

namespace InsuraTech.Application.HealthPlans.Queries.CalculateHealthPlan;

public sealed record CalculateHealthPlanQuery : IRequest<HealthPlanCalculationDto>
{
    public string PlanId { get; init; } = null!;
    public DateOnly BirthDate { get; init; }
}
