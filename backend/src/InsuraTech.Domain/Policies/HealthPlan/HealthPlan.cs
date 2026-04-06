using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Policies.HealthPlan;

public sealed class HealthPlan : ValueObject
{
    public string Id { get; }
    public string Name { get; }
    public decimal BaseAmount { get; }

    internal HealthPlan(string id, string name, decimal baseAmount)
    {
        Id = id;
        Name = name;
        BaseAmount = baseAmount;
    }

    protected override IEnumerable<object> GetEqualityComponets()
    {
        yield return Id;
    }

    public override string ToString() => $"{Name} (${BaseAmount:N0})";
}
