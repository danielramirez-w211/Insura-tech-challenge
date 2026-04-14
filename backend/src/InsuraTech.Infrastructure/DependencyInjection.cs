using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Infrastructure.ExternalServices;
using InsuraTech.Infrastructure.Persistence;
using InsuraTech.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        MongoDbContext.RegisterClassMaps();

        var connectionString = configuration.GetConnectionString("MongoDb")
            ?? throw new InvalidOperationException("Missing connection string 'MongoDb'.");

        var databaseName = configuration["MongoDb:DatabaseName"] ?? "InsuraTechDb";

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        services.AddScoped<MongoDbContext>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var publisher = sp.GetRequiredService<MediatR.IPublisher>();
            return new MongoDbContext(client, databaseName, publisher);
        });

        services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ICityRepository>(_ =>
            new CityRepository(
                _.GetRequiredService<IMongoClient>(),
                databaseName));

        // TRM — integración con API Socrata
        services.AddMemoryCache();
        services.AddHttpClient<ITrmService, TrmService>();

        return services;
    }
}
