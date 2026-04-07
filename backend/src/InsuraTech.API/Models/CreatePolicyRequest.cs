using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.TravelPlan;

namespace InsuraTech.API.Models;

public sealed record CreatePolicyRequest
{
    public PolicyType Type { get; init; }
    public InsuredRequest Insured { get; init; } = null!;
    public CoveragePeriodRequest CoveragePeriod { get; init; } = null!;
    public decimal InsuredAmount { get; init; }
    public decimal MonthlyPremium { get; init; }
    /// <summary>Solo para Type = Health. Reemplaza InsuredAmount (se calcula automáticamente).</summary>
    public string? HealthPlanId { get; init; }
    /// <summary>Solo para Type = Travel. Nacional o Internacional.</summary>
    public TripType? TripType { get; init; }
    /// <summary>Solo para Type = Travel e Internacional.</summary>
    public Continent? Continent { get; init; }
    /// <summary>Solo para Type = Travel. Días de cobertura (1-180).</summary>
    public int? DurationDays { get; init; }
}

public sealed record InsuredRequest
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string DocumentType {get; init;} = null!;
    public string DocumentId { get; init; } = null!;
    public DateOnly BirthDate { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
}

public sealed record CoveragePeriodRequest
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}
