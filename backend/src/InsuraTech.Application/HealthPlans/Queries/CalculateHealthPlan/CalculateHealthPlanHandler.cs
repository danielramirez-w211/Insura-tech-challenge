using InsuraTech.Application.HealthPlans.DTOs;
using InsuraTech.Domain.Policies.HealthPlan;
using MediatR;

namespace InsuraTech.Application.HealthPlans.Queries.CalculateHealthPlan;

public sealed class CalculateHealthPlanHandler
    : IRequestHandler<CalculateHealthPlanQuery, HealthPlanCalculationDto>
{
    public Task<HealthPlanCalculationDto> Handle(
        CalculateHealthPlanQuery request,
        CancellationToken cancellationToken)
    {
        var today     = DateOnly.FromDateTime(DateTime.UtcNow);
        var selection = HealthPlanPricingService.Calculate(request.PlanId, request.BirthDate, today);

        int age = today.Year - request.BirthDate.Year;
        if (request.BirthDate > today.AddYears(-age)) age--;

        var dto = new HealthPlanCalculationDto
        {
            PlanId              = selection.PlanId,
            PlanName            = selection.PlanName,
            BaseAmount          = selection.BaseAmount,
            AgeFactorPercentage = selection.AgeFactorPercentage,
            AgeFactorAmount     = selection.AgeFactorAmount,
            FinalAmount         = selection.FinalAmount,
            InsuredAge          = age,
            MonthlyPremium      = QuotationService.CalculateMonthlyPremium(selection.FinalAmount),
            DurationDays        = QuotationService.FixedDurationDays,
        };

        return Task.FromResult(dto);
    }
}
