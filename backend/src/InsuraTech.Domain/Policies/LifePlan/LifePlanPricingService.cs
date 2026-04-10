using InsuraTech.Domain.Exceptions;

namespace InsuraTech.Domain.Policies.LifePlan;

public static class LifePlanPricingService
{
    public const int MinAge = 18;
    public const int MaxAge = 65;

    public static LifePlanSelection Calculate(string planId, DateOnly birthDate, DateOnly today)
    {
        var plan = LifePlanCatalog.FindById(planId)
            ?? throw new InvalidLifePlanException(planId);

        int age = CalculateAge(birthDate, today);

        if (age < MinAge)
            throw new UnderageInsuredException();

        if (age > MaxAge)
            throw new OverageLifeInsuredException();

        return new LifePlanSelection(
            plan.Id,
            plan.Name,
            plan.AnnualPremium,
            plan.MonthlyPremium,
            plan.DeathBenefit,
            plan.FuneralExpenses,
            plan.BurialExpenses,
            plan.BeneficiaryCompensation);
    }

    private static int CalculateAge(DateOnly birthDate, DateOnly today)
    {
        int age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;
        return age;
    }
}
