using InsuraTech.Application.VehiclePlans.DTOs;
using MediatR;

namespace InsuraTech.Application.VehiclePlans.Queries.GetVehiclePlans;

public sealed record GetVehiclePlansQuery : IRequest<IReadOnlyList<VehiclePlanDto>>;
