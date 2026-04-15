using InsuraTech.Application.HomePlans.DTOs;
using InsuraTech.Domain.Policies.HomePlan;
using MediatR;

namespace InsuraTech.Application.HomePlans.Queries.CalculateHomeQuotation;

public sealed class CalculateHomeQuotationHandler
    : IRequestHandler<CalculateHomeQuotationQuery, HomeQuotationDto>
{
    public Task<HomeQuotationDto> Handle(
        CalculateHomeQuotationQuery request,
        CancellationToken cancellationToken)
    {
        var propertyType = Enum.Parse<HomePropertyType>(request.PropertyType, ignoreCase: true);

        var coverages = request.SelectedCoverages
            .Select(c => Enum.Parse<HomeCoverage>(c, ignoreCase: true))
            .ToList()
            .AsReadOnly();

        var quotation = HomePricingService.Calculate(
            request.PropertyValue,
            request.ConstructionYear,
            request.Stratum,
            request.Occupants,
            propertyType,
            coverages,
            DateTime.UtcNow.Year);

        var dto = new HomeQuotationDto
        {
            PropertyValue       = quotation.PropertyValue,
            ConstructionYear    = quotation.ConstructionYear,
            PropertyAge         = quotation.PropertyAge,
            Stratum             = quotation.Stratum,
            Occupants           = quotation.Occupants,
            PropertyType        = quotation.PropertyType.ToString(),
            BaseMonthlyPremium  = quotation.BaseMonthlyPremium,
            SelectedCoverages   = quotation.SelectedCoverages
                                           .Select(c => c.ToString())
                                           .ToList()
                                           .AsReadOnly(),
            AppliedMultipliers  = quotation.AppliedMultipliers,
            FinalMonthlyPremium = quotation.FinalMonthlyPremium,
        };

        return Task.FromResult(dto);
    }
}
