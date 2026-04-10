using InsuraTech.Application.VehiclePlans.DTOs;
using MediatR;

namespace InsuraTech.Application.VehiclePlans.Queries.CalculateVehiclePlans;

public sealed record CalculateVehiclePlansQuery : IRequest<VehicleQuotationDto>
{
    public decimal CommercialValue { get; init; }
    public int     VehicleYear     { get; init; }
    public string  Brand           { get; init; } = null!;
}
