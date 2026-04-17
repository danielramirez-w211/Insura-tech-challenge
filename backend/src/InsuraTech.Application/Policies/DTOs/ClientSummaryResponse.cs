namespace InsuraTech.Application.Policies.DTOs;

public sealed class ClientSummaryResponse
{
    public string DocumentId   { get; init; } = null!;
    public string DocumentType { get; init; } = null!;
    public string FirstName    { get; init; } = null!;
    public string LastName     { get; init; } = null!;
    public string CityName     { get; init; } = null!;
    public int    PolicyCount  { get; init; }
}