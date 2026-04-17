namespace InsuraTech.Application.Users.DTOs;

public sealed class UserProfileResponse
{
    public string   FirstName      { get; init; } = null!;
    public string   LastName       { get; init; } = null!;
    public string   Nationality    { get; init; } = null!;
    public string   BirthDate      { get; init; } = null!;
    public int      YearsInCompany { get; init; }
    public string   PhotoUrl       { get; init; } = null!;
    public string   OfficeLocation { get; init; } = null!;
    public string   WorkSchedule   { get; init; } = null!;
}

public sealed class UserResponse
{
    public Guid                Id          { get; init; }
    public string              Email       { get; init; } = null!;
    public string              Role        { get; init; } = null!;
    public bool                IsActive    { get; init; }
    public string?             AdvisorCode { get; init; }
    public Guid?               LeaderId    { get; init; }
    public UserProfileResponse Profile     { get; init; } = null!;
}

public sealed class AdvisorSummaryResponse
{
    public Guid   Id          { get; init; }
    public string Email       { get; init; } = null!;
    public string AdvisorCode { get; init; } = null!;
    public bool   IsActive    { get; init; }
    public string FirstName   { get; init; } = null!;
    public string LastName    { get; init; } = null!;
    public int    SalesCount  { get; init; }
}
