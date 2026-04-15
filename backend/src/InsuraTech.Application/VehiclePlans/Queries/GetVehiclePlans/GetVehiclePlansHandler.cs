using InsuraTech.Application.VehiclePlans.DTOs;
using InsuraTech.Domain.Policies.VehiclePlan;
using MediatR;

namespace InsuraTech.Application.VehiclePlans.Queries.GetVehiclePlans;

public sealed class GetVehiclePlansHandler
    : IRequestHandler<GetVehiclePlansQuery, IReadOnlyList<VehiclePlanDto>>
{
    public Task<IReadOnlyList<VehiclePlanDto>> Handle(
        GetVehiclePlansQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<VehiclePlanDto> result = VehiclePlanCatalog.All
            .Select(p => new VehiclePlanDto
            {
                PlanId          = p.Id,
                PlanName        = p.Name,
                PriceMultiplier = p.PriceMultiplier,
                Coverages       = p.Coverages,
                Assistances     = p.Assistances,
            })
            .ToList();

        return Task.FromResult(result);
    }
}
