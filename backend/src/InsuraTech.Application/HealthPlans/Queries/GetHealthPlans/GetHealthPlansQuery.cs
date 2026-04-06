using InsuraTech.Application.HealthPlans.DTOs;
using MediatR;

namespace InsuraTech.Application.HealthPlans.Queries.GetHealthPlans;

public sealed record GetHealthPlansQuery : IRequest<IReadOnlyList<HealthPlanDto>>;
