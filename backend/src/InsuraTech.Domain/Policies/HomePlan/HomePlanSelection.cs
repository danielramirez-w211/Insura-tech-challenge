using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Policies.HomePlan;

/// <summary>
/// Snapshot inmutable de la cotización de hogar seleccionada en el momento de contratación (SPEC-012).
/// Se embebe dentro de Policy y no cambia aunque el catálogo o el motor evolucionen.
/// </summary>
public sealed class HomePlanSelection : ValueObject
{
    public string?                      PackageId           { get; }
    public string                       PackageName         { get; }
    public decimal                      PropertyValue       { get; }
    public int                          ConstructionYear    { get; }
    public int                          Stratum             { get; }
    public int                          Occupants           { get; }
    public HomePropertyType             PropertyType        { get; }
    public IReadOnlyList<HomeCoverage>  SelectedCoverages   { get; }
    public IReadOnlyList<string>        AppliedMultipliers  { get; }
    public decimal                      BaseMonthlyPremium  { get; }
    public decimal                      FinalMonthlyPremium { get; }

    public HomePlanSelection(
        string?                     packageId,
        string                      packageName,
        decimal                     propertyValue,
        int                         constructionYear,
        int                         stratum,
        int                         occupants,
        HomePropertyType            propertyType,
        IReadOnlyList<HomeCoverage> selectedCoverages,
        IReadOnlyList<string>       appliedMultipliers,
        decimal                     baseMonthlyPremium,
        decimal                     finalMonthlyPremium)
    {
        PackageId           = packageId;
        PackageName         = packageName;
        PropertyValue       = propertyValue;
        ConstructionYear    = constructionYear;
        Stratum             = stratum;
        Occupants           = occupants;
        PropertyType        = propertyType;
        SelectedCoverages   = selectedCoverages;
        AppliedMultipliers  = appliedMultipliers;
        BaseMonthlyPremium  = baseMonthlyPremium;
        FinalMonthlyPremium = finalMonthlyPremium;
    }

    protected override IEnumerable<object> GetEqualityComponets()
    {
        yield return PackageId ?? string.Empty;
        yield return PropertyValue;
        yield return FinalMonthlyPremium;
    }
}
