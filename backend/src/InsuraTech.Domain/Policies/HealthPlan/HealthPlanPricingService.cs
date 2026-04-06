using InsuraTech.Domain.Exceptions;

namespace InsuraTech.Domain.Policies.HealthPlan;

public static class HealthPlanPricingService
{
    public static HealthPlanSelection Calculate(string planId, DateOnly birthDate, DateOnly today)
    {
        var plan = HealthPlanCatalog.FindById(planId)
            ?? throw new InvalidHealthPlanException(planId);

        int age = CalculateAge(birthDate, today);

        if (age < 18)
            throw new UnderageInsuredException();

        if (age >= 74)
            throw new OverageInsuredException();

        int factorPercent = age switch
        {
            >= 18 and <= 35 => 0,
            >= 36 and <= 58 => 4,
            _               => 8   // 59–73
        };

        decimal addition = Math.Round(plan.BaseAmount * factorPercent / 100m, 0, MidpointRounding.AwayFromZero);
        decimal final    = plan.BaseAmount + addition;

        return new HealthPlanSelection(
            plan.Id,
            plan.Name,
            plan.BaseAmount,
            factorPercent,
            addition,
            final);
    }

    private static int CalculateAge(DateOnly birthDate, DateOnly today)
    {
        int age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;
        return age;
    }
}
