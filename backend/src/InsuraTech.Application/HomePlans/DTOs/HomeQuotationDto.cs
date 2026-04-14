namespace InsuraTech.Application.HomePlans.DTOs;

public sealed record HomeQuotationDto
{
    public required decimal                 PropertyValue       { get; init; }
    public required int                     ConstructionYear    { get; init; }
    public required int                     PropertyAge         { get; init; }
    public required int                     Stratum             { get; init; }
    public required int                     Occupants           { get; init; }
    public required string                  PropertyType        { get; init; }
    public required decimal                 BaseMonthlyPremium  { get; init; }
    public required IReadOnlyList<string>   SelectedCoverages   { get; init; }
    public required IReadOnlyList<string>   AppliedMultipliers  { get; init; }
    public required decimal                 FinalMonthlyPremium { get; init; }
}
