using InsuraTech.Application.HomePlans.DTOs;
using MediatR;

namespace InsuraTech.Application.HomePlans.Queries.CalculateHomeQuotation;

public sealed record CalculateHomeQuotationQuery : IRequest<HomeQuotationDto>
{
    public decimal                  PropertyValue       { get; init; }
    public int                      ConstructionYear    { get; init; }
    public int                      Stratum             { get; init; }
    public int                      Occupants           { get; init; }
    public string                   PropertyType        { get; init; } = null!;
    public IReadOnlyList<string>    SelectedCoverages   { get; init; } = [];
}
