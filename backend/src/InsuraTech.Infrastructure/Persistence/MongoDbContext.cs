using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Common;
using InsuraTech.Domain.Notifications;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.HealthPlan;
using InsuraTech.Domain.Policies.TravelPlan;
using InsuraTech.Domain.Policies.ValueObjects;
using MediatR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure.Persistence;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IPublisher _publisher;

    public IMongoCollection<Policy> Policies => _database.GetCollection<Policy>("policies");
    public IMongoCollection<Claim> Claims => _database.GetCollection<Claim>("claims");
    public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("notifications");
    public IMongoCollection<BsonDocument> PolicyIdempotencyKeys => _database.GetCollection<BsonDocument>("policy_idempotency_keys");
    public IMongoCollection<BsonDocument> Counters => _database.GetCollection<BsonDocument>("counters");

    public IMongoClient Client { get; }

    public MongoDbContext(IMongoClient client, string databaseName, IPublisher publisher)
    {
        Client = client;
        _database = client.GetDatabase(databaseName);
        _publisher = publisher;
    }

    public async Task PublishDomainEventsAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);

        aggregate.ClearDomainEvent();
    }

    public async Task EnsureIndexesAsync()
    {
        // Policies: unique index on number (camelCase by convention)
        await Policies.Indexes.CreateOneAsync(
            new CreateIndexModel<Policy>(
                Builders<Policy>.IndexKeys.Ascending("number.value"),
                new CreateIndexOptions { Unique = true }),
            cancellationToken: CancellationToken.None);

        // Notifications: index on status (camelCase by convention)
        await Notifications.Indexes.CreateOneAsync(
            new CreateIndexModel<Notification>(
                Builders<Notification>.IndexKeys.Ascending("status")),
            cancellationToken: CancellationToken.None);

        // IdempotencyKeys: unique index on key
        await PolicyIdempotencyKeys.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                new BsonDocument("key", 1),
                new CreateIndexOptions { Unique = true }),
            cancellationToken: CancellationToken.None);
    }

    // -----------------------------------------------------------------------
    // Static BSON registration — called once at startup
    // -----------------------------------------------------------------------
    private static bool _registered;
    private static readonly object _lock = new();

    public static void RegisterClassMaps()
    {
        lock (_lock)
        {
            if (_registered) return;
            _registered = true;

            // Conventions
            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
            };
            ConventionRegistry.Register("InsuraTech", pack, _ => true);

            // DateOnly serializer
            BsonSerializer.RegisterSerializer(new DateOnlySerializer());

            // ---- Entity base ----
            BsonClassMap.RegisterClassMap<Entity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(e => e.Id).SetSerializer(new GuidSerializer(BsonType.String));
            });

            // ---- AggregateRoot ----
            BsonClassMap.RegisterClassMap<AggregateRoot>(cm =>
            {
                cm.AutoMap();
                // domain events are transient, not persisted
                cm.UnmapField("_domainEvents");
            });

            // ---- PolicyStatusHistory ----
            BsonClassMap.RegisterClassMap<PolicyStatusHistory>(cm =>
            {
                cm.AutoMap();
            });

            // ---- PolicyNumber ----
            // SetElementName explícito: garantiza "value" (minúscula) sin depender de la convención
            BsonClassMap.RegisterClassMap<PolicyNumber>(cm =>
            {
                cm.MapProperty(pn => pn.Value).SetElementName("value");
                cm.MapCreator(pn => PolicyNumber.Parse(pn.Value));
            });

            // ---- InsuredPerson ----
            BsonClassMap.RegisterClassMap<InsuredPerson>(cm =>
            {
                cm.MapProperty(p => p.FirstName);
                cm.MapProperty(p => p.LastName);
                cm.MapProperty(p => p.DocumentType);
                cm.MapProperty(p => p.DocumentId);
                cm.MapProperty(p => p.BirthDate);
                cm.MapCreator(p => InsuredPerson.Create(p.FirstName, p.LastName, p.DocumentType, p.DocumentId, p.BirthDate));
            });

            // ---- CoveragePeriod ----
            BsonClassMap.RegisterClassMap<CoveragePeriod>(cm =>
            {
                cm.MapProperty(c => c.StartDate);
                cm.MapProperty(c => c.EndDate);
                cm.MapCreator(c => new CoveragePeriod(c.StartDate, c.EndDate));
            });

            // ---- HealthPlanSelection ----
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

            // ---- TravelPlanSelection ----
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

            // ---- Policy ----
            BsonClassMap.RegisterClassMap<Policy>(cm =>
            {
                cm.AutoMap();
                cm.MapField("_statusHistory").SetElementName("statusHistory");
                cm.SetIgnoreExtraElements(true);
            });

            // ---- ClaimStatusHistory ----
            BsonClassMap.RegisterClassMap<ClaimStatusHistory>(cm =>
            {
                cm.AutoMap();
            });

            // ---- Claim ----
            BsonClassMap.RegisterClassMap<Claim>(cm =>
            {
                cm.AutoMap();
                cm.MapField("_statusHistory").SetElementName("statusHistory");
                cm.SetIgnoreExtraElements(true);
            });

            // ---- Notification ----
            BsonClassMap.RegisterClassMap<Notification>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}

/// <summary>DateOnly ↔ "yyyy-MM-dd" string in BSON.</summary>
public sealed class DateOnlySerializer : SerializerBase<DateOnly>
{
    public override DateOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var str = context.Reader.ReadString();
        return DateOnly.ParseExact(str, "yyyy-MM-dd");
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateOnly value)
    {
        context.Writer.WriteString(value.ToString("yyyy-MM-dd"));
    }
}
