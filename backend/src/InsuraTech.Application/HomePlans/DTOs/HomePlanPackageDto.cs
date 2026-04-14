namespace InsuraTech.Application.HomePlans.DTOs;

public sealed record HomePlanPackageDto
{
    public required string                  PackageId   { get; init; }
    public required string                  PackageName { get; init; }
    public required IReadOnlyList<string>   Coverages   { get; init; }
}
