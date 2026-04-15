using InsuraTech.Application.LifePlans.DTOs;
using InsuraTech.Domain.Policies.HealthPlan;
using InsuraTech.Domain.Policies.LifePlan;
using MediatR;

namespace InsuraTech.Application.LifePlans.Queries.CalculateLifePlan;

public sealed class CalculateLifePlanHandler
    : IRequestHandler<CalculateLifePlanQuery, LifePlanCalculationDto>
{
    public Task<LifePlanCalculationDto> Handle(
        CalculateLifePlanQuery request,
        CancellationToken cancellationToken)
    {
        var today     = DateOnly.FromDateTime(DateTime.UtcNow);
        var selection = LifePlanPricingService.Calculate(request.PlanId, request.BirthDate, today);

        int age = today.Year - request.BirthDate.Year;
        if (request.BirthDate > today.AddYears(-age)) age--;

        var dto = new LifePlanCalculationDto
        {
            PlanId                  = selection.PlanId,
            PlanName                = selection.PlanName,
            InsuredAge              = age,
            AnnualPremium           = selection.AnnualPremium,
            MonthlyPremium          = selection.MonthlyPremium,
            DurationDays            = QuotationService.FixedDurationDays,
            DeathBenefit            = selection.DeathBenefit,
            FuneralExpenses         = selection.FuneralExpenses,
            BurialExpenses          = selection.BurialExpenses,
            BeneficiaryCompensation = selection.BeneficiaryCompensation,
        };

        return Task.FromResult(dto);
    }
}
