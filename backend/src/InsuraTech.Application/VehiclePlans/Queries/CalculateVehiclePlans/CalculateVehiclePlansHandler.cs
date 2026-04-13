using InsuraTech.Application.VehiclePlans.DTOs;
using InsuraTech.Domain.Policies.VehiclePlan;
using InsuraTech.Domain.Policies.HealthPlan;
using MediatR;

namespace InsuraTech.Application.VehiclePlans.Queries.CalculateVehiclePlans;

public sealed class CalculateVehiclePlansHandler
    : IRequestHandler<CalculateVehiclePlansQuery, VehicleQuotationDto>
{
    private const decimal AnnualDiscount = 0.95m;

    public Task<VehicleQuotationDto> Handle(
        CalculateVehiclePlansQuery request,
        CancellationToken cancellationToken)
    {
        var today     = DateTime.UtcNow;
        var quotation = VehiclePricingService.Calculate(
            request.CommercialValue,
            request.VehicleYear,
            request.Brand,
            today.Year);

        var plans = VehiclePlanCatalog.All.Select(plan =>
        {
            decimal monthly = plan.Id.ToLowerInvariant() switch
            {
                "standard" => quotation.BaseMonthlyPremium,
                "complete" => quotation.CompletePlanPremium,
                "premium"  => quotation.PremiumPlanPremium,
                _          => quotation.BaseMonthlyPremium
            };

            // Prima anual con 5% de descuento por pago único (RN-08, RN-18)
            decimal annual = plan.Id.ToLowerInvariant() switch
            {
                "standard" => quotation.AnnualBase,
                "complete" => quotation.AnnualComplete,
                "premium"  => quotation.AnnualPremium,
                _          => quotation.AnnualBase
            };

            return new VehiclePlanOptionDto
            {
                PlanId                    = plan.Id,
                PlanName                  = plan.Name,
                MonthlyPremium            = monthly,
                AnnualPremiumWithDiscount = annual * AnnualDiscount,
                Coverages                 = plan.Coverages,
                Assistances               = plan.Assistances,
            };
        }).ToList();

        var dto = new VehicleQuotationDto
        {
            CommercialValue    = quotation.CommercialValue,
            VehicleYear        = quotation.VehicleYear,
            Brand              = quotation.Brand,
            VehicleAge         = quotation.VehicleAge,
            AgeCategory        = quotation.AgeCategory,
            TechnicalRate      = quotation.TechnicalRate,
            HasBrandSurcharge  = quotation.HasBrandSurcharge,
            BaseMonthlyPremium = quotation.BaseMonthlyPremium,
            Plans              = plans,
        };

        return Task.FromResult(dto);
    }
}
