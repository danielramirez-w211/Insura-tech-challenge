using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Common;
using InsuraTech.Domain.Notifications;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.HealthPlan;
using InsuraTech.Domain.Policies.TravelPlan;
using InsuraTech.Domain.Policies.ValueObjects;
using InsuraTech.Domain.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace InsuraTech.Infrastructure.Persistence.BsonConfiguration;

public static class ClassMapRegistry
{
    private static bool _registered;
    private static readonly object _lock = new();

    public static void RegisterAll()
    {
        lock (_lock)
        {
            if (_registered) return;
            _registered = true;

            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
            };
            ConventionRegistry.Register("InsuraTech", pack, _ => true);

            BsonSerializer.RegisterSerializer(new DateOnlyBsonSerializer());

            BsonClassMap.RegisterClassMap<Entity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(e => e.Id).SetSerializer(new GuidSerializer(BsonType.String));
            });

            BsonClassMap.RegisterClassMap<AggregateRoot>(cm =>
            {
                cm.AutoMap();
                cm.UnmapField("_domainEvents");
            });

            BsonClassMap.RegisterClassMap<PolicyStatusHistory>(cm => cm.AutoMap());

            // SetElementName explícito: garantiza "value" (minúscula) sin depender de la convención
            BsonClassMap.RegisterClassMap<PolicyNumber>(cm =>
            {
                cm.MapProperty(pn => pn.Value).SetElementName("value");
                cm.MapCreator(pn => PolicyNumber.Parse(pn.Value));
            });

            // Campos nuevos (SPEC-011): documentos anteriores no los tienen,
            // SetDefaultValue garantiza que MongoDB use "" cuando el campo está ausente.
            BsonClassMap.RegisterClassMap<InsuredPerson>(cm =>
            {
                cm.MapProperty(p => p.FirstName);
                cm.MapProperty(p => p.LastName);
                cm.MapProperty(p => p.DocumentType);
                cm.MapProperty(p => p.DocumentId);
                cm.MapProperty(p => p.BirthDate);
                cm.MapProperty(p => p.Gender).SetDefaultValue(string.Empty);
                cm.MapProperty(p => p.Address).SetDefaultValue(string.Empty);
                cm.MapProperty(p => p.CityName).SetDefaultValue(string.Empty);
                cm.MapProperty(p => p.PostalCode).SetDefaultValue(string.Empty);
                cm.MapProperty(p => p.Department).SetDefaultValue(string.Empty);
                cm.MapCreator(p => InsuredPerson.Reconstitute(
                    p.FirstName, p.LastName, p.DocumentType, p.DocumentId, p.BirthDate,
                    p.Gender, p.Address, p.CityName, p.PostalCode, p.Department));
            });

            BsonClassMap.RegisterClassMap<CoveragePeriod>(cm =>
            {
                cm.MapProperty(c => c.StartDate);
                cm.MapProperty(c => c.EndDate);
                cm.MapCreator(c => new CoveragePeriod(c.StartDate, c.EndDate));
            });

            BsonClassMap.RegisterClassMap<HealthPlanSelection>(cm =>
            {
                cm.MapProperty(h => h.PlanId);
                cm.MapProperty(h => h.PlanName);
                cm.MapProperty(h => h.BaseAmount);
                cm.MapProperty(h => h.AgeFactorPercentage);
                cm.MapProperty(h => h.AgeFactorAmount);
                cm.MapProperty(h => h.FinalAmount);
                cm.MapCreator(h => new HealthPlanSelection(
                    h.PlanId, h.PlanName, h.BaseAmount,
                    h.AgeFactorPercentage, h.AgeFactorAmount, h.FinalAmount));
            });

            BsonClassMap.RegisterClassMap<TravelPlanSelection>(cm =>
            {
                cm.MapProperty(t => t.TripType);
                cm.MapProperty(t => t.Continent);
                cm.MapProperty(t => t.DurationDays);
                cm.MapProperty(t => t.BasePriceUsd);
                cm.MapProperty(t => t.BasePriceCop);
                cm.MapProperty(t => t.DailyIncrementCop);
                cm.MapProperty(t => t.TotalPriceCop);
                cm.MapProperty(t => t.TrmUsed);
                cm.MapProperty(t => t.TrmDate);
                cm.MapProperty(t => t.CalculatedAt);
                cm.MapCreator(t => new TravelPlanSelection(
                    t.TripType, t.Continent, t.DurationDays, t.BasePriceUsd,
                    t.BasePriceCop, t.DailyIncrementCop, t.TotalPriceCop,
                    t.TrmUsed, t.TrmDate, t.CalculatedAt));
            });

            BsonClassMap.RegisterClassMap<Policy>(cm =>
            {
                cm.AutoMap();
                cm.MapField("_statusHistory").SetElementName("statusHistory");
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<ClaimStatusHistory>(cm => cm.AutoMap());

            BsonClassMap.RegisterClassMap<Claim>(cm =>
            {
                cm.AutoMap();
                cm.MapField("_statusHistory").SetElementName("statusHistory");
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<Notification>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<UserProfile>(cm =>
            {
                cm.MapProperty(p => p.FirstName);
                cm.MapProperty(p => p.LastName);
                cm.MapProperty(p => p.Nationality);
                cm.MapProperty(p => p.BirthDate);
                cm.MapProperty(p => p.YearsInCompany);
                cm.MapProperty(p => p.PhotoUrl);
                cm.MapProperty(p => p.OfficeLocation);
                cm.MapProperty(p => p.WorkSchedule);
                cm.MapCreator(p => new UserProfile(
                    p.FirstName, p.LastName, p.Nationality, p.BirthDate,
                    p.YearsInCompany, p.PhotoUrl, p.OfficeLocation, p.WorkSchedule));
            });

            BsonClassMap.RegisterClassMap<User>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}
