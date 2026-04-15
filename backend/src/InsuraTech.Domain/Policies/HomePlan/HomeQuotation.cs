namespace InsuraTech.Domain.Policies.HomePlan;

/// <summary>
/// Resultado transitorio del motor de cotización de hogar (SPEC-012).
/// No se persiste; se usa para presentar la prima calculada al agente.
/// </summary>
public sealed class HomeQuotation
{
    public decimal                      PropertyValue       { get; }
    public int                          ConstructionYear    { get; }
    public int                          PropertyAge         { get; }
    public int                          Stratum             { get; }
    public int                          Occupants           { get; }
    public HomePropertyType             PropertyType        { get; }
    public decimal                      BaseMonthlyPremium  { get; }
    public IReadOnlyList<HomeCoverage>  SelectedCoverages   { get; }
    public IReadOnlyList<string>        AppliedMultipliers  { get; }
    public decimal                      FinalMonthlyPremium { get; }

    public HomeQuotation(
        decimal                     propertyValue,
        int                         constructionYear,
        int                         propertyAge,
        int                         stratum,
        int                         occupants,
        HomePropertyType            propertyType,
        decimal                     baseMonthlyPremium,
        IReadOnlyList<HomeCoverage> selectedCoverages,
        IReadOnlyList<string>       appliedMultipliers,
        decimal                     finalMonthlyPremium)
    {
        PropertyValue       = propertyValue;
        ConstructionYear    = constructionYear;
        PropertyAge         = propertyAge;
        Stratum             = stratum;
        Occupants           = occupants;
        PropertyType        = propertyType;
        BaseMonthlyPremium  = baseMonthlyPremium;
        SelectedCoverages   = selectedCoverages;
        AppliedMultipliers  = appliedMultipliers;
        FinalMonthlyPremium = finalMonthlyPremium;
    }
}
