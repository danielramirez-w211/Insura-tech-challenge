namespace InsuraTech.Application.VehiclePlans.DTOs;

/// <summary>DTO del catálogo de plan vehicular — respuesta de GET /api/v1/vehicle-plans.</summary>
public sealed class VehiclePlanDto
{
    public string  PlanId           { get; init; } = null!;
    public string  PlanName         { get; init; } = null!;
    public decimal PriceMultiplier  { get; init; }
    public IReadOnlyList<string> Coverages    { get; init; } = [];
    public IReadOnlyList<string> Assistances  { get; init; } = [];
}

/// <summary>Opción de plan dentro del resultado de cotización.</summary>
public sealed class VehiclePlanOptionDto
{
    public string  PlanId                    { get; init; } = null!;
    public string  PlanName                  { get; init; } = null!;
    public decimal MonthlyPremium            { get; init; }
    public decimal AnnualPremiumWithDiscount  { get; init; }
    public IReadOnlyList<string> Coverages   { get; init; } = [];
    public IReadOnlyList<string> Assistances { get; init; } = [];
}

/// <summary>Resultado completo de cotización vehicular — respuesta de GET /api/v1/vehicle-plans/calculate.</summary>
public sealed class VehicleQuotationDto
{
    public decimal CommercialValue    { get; init; }
    public int     VehicleYear        { get; init; }
    public string  Brand              { get; init; } = null!;
    public int     VehicleAge         { get; init; }
    public string  AgeCategory        { get; init; } = null!;
    public decimal TechnicalRate      { get; init; }
    public bool    HasBrandSurcharge  { get; init; }
    public decimal BaseMonthlyPremium { get; init; }
    public IReadOnlyList<VehiclePlanOptionDto> Plans { get; init; } = [];
}
