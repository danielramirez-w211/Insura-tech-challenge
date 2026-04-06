using InsuraTech.Application.HealthPlans.DTOs;
using InsuraTech.Domain.Policies.HealthPlan;
using MediatR;

namespace InsuraTech.Application.HealthPlans.Queries.GetHealthPlans;

public sealed class GetHealthPlansHandler
    : IRequestHandler<GetHealthPlansQuery, IReadOnlyList<HealthPlanDto>>
{
    public Task<IReadOnlyList<HealthPlanDto>> Handle(
        GetHealthPlansQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<HealthPlanDto> result = HealthPlanCatalog.All
            .Select(p => new HealthPlanDto
            {
                PlanId     = p.Id,
                PlanName   = p.Name,
                BaseAmount = p.BaseAmount
            })
            .ToList();

        return Task.FromResult(result);
    }
}
