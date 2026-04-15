using InsuraTech.Application.HomePlans.DTOs;
using InsuraTech.Domain.Policies.HomePlan;
using MediatR;

namespace InsuraTech.Application.HomePlans.Queries.GetHomePlans;

public sealed class GetHomePlansHandler
    : IRequestHandler<GetHomePlansQuery, IReadOnlyList<HomePlanPackageDto>>
{
    public Task<IReadOnlyList<HomePlanPackageDto>> Handle(
        GetHomePlansQuery request,
        CancellationToken cancellationToken)
    {
        var result = HomePlanPackageCatalog.All
            .Select(pkg => new HomePlanPackageDto
            {
                PackageId   = pkg.Id,
                PackageName = pkg.Name,
                Coverages   = pkg.Coverages
                                 .Select(c => c.ToString())
                                 .ToList()
                                 .AsReadOnly(),
            })
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<HomePlanPackageDto>>(result);
    }
}
