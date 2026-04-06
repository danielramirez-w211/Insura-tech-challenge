using InsuraTech.Domain.Policies;

namespace InsuraTech.API.Models;

public sealed record CreatePolicyRequest
{
    public PolicyType Type { get; init; }
    public InsuredRequest Insured { get; init; } = null!;
    public CoveragePeriodRequest CoveragePeriod { get; init; } = null!;
    public decimal InsuredAmount { get; init; }
    public decimal MonthlyPremium { get; init; }
}

public sealed record InsuredRequest
{
    public string Name { get; init; } = null!;
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
