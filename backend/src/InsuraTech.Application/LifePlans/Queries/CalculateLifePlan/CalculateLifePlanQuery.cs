using InsuraTech.Application.LifePlans.DTOs;
using MediatR;

namespace InsuraTech.Application.LifePlans.Queries.CalculateLifePlan;

public sealed record CalculateLifePlanQuery : IRequest<LifePlanCalculationDto>
{
    public string PlanId { get; init; } = null!;
    public DateOnly BirthDate { get; init; }
}
