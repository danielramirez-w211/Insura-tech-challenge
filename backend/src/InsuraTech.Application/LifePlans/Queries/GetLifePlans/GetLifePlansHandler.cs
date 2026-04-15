using InsuraTech.Application.LifePlans.DTOs;
using InsuraTech.Domain.Policies.LifePlan;
using MediatR;

namespace InsuraTech.Application.LifePlans.Queries.GetLifePlans;

public sealed class GetLifePlansHandler
    : IRequestHandler<GetLifePlansQuery, IReadOnlyList<LifePlanDto>>
{
    public Task<IReadOnlyList<LifePlanDto>> Handle(
        GetLifePlansQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<LifePlanDto> result = LifePlanCatalog.All
            .Select(p => new LifePlanDto
            {
                PlanId                  = p.Id,
                PlanName                = p.Name,
                AnnualPremium           = p.AnnualPremium,
                MonthlyPremium          = p.MonthlyPremium,
                DeathBenefit            = p.DeathBenefit,
                FuneralExpenses         = p.FuneralExpenses,
                BurialExpenses          = p.BurialExpenses,
                BeneficiaryCompensation = p.BeneficiaryCompensation,
            })
            .ToList();

        return Task.FromResult(result);
    }
}
