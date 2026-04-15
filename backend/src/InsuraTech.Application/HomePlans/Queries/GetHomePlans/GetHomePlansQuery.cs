using InsuraTech.Application.HomePlans.DTOs;
using MediatR;

namespace InsuraTech.Application.HomePlans.Queries.GetHomePlans;

public sealed record GetHomePlansQuery : IRequest<IReadOnlyList<HomePlanPackageDto>>;
