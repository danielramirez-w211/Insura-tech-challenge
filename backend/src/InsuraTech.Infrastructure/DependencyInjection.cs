using InsuraTech.Application.Auth.Services;
using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Application.Policies.Commands.CreatePolicy.Strategies;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Users;
using InsuraTech.Infrastructure.Auth;
using InsuraTech.Infrastructure.ExternalServices;
using InsuraTech.Infrastructure.Persistence;
using InsuraTech.Infrastructure.Persistence.BsonConfiguration;
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
        ClassMapRegistry.RegisterAll();

        var connectionString = configuration.GetConnectionString("MongoDb")
            ?? throw new InvalidOperationException("Missing connection string 'MongoDb'.");

        var databaseName = configuration["MongoDb:DatabaseName"] ?? "InsuraTechDb";

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        services.AddScoped<MongoDbContext>(sp =>
        {
            var client    = sp.GetRequiredService<IMongoClient>();
            var publisher = sp.GetRequiredService<MediatR.IPublisher>();
            return new MongoDbContext(client, databaseName, publisher);
        });

        // Repositories
        services.AddScoped<IUnitOfWork,             MongoUnitOfWork>();
        services.AddScoped<IPolicyRepository,       PolicyRepository>();
        services.AddScoped<IClaimRepository,        ClaimRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ICityRepository,         CityRepository>();
        services.AddScoped<IUserRepository,         UserRepository>();

        // Policy creation strategies
        services.AddScoped<ICreatePolicyStrategy, CreateHealthPolicyStrategy>();
        services.AddScoped<ICreatePolicyStrategy, CreateLifePolicyStrategy>();
        services.AddScoped<ICreatePolicyStrategy, CreateVehiclePolicyStrategy>();
        services.AddScoped<ICreatePolicyStrategy, CreateHomePolicyStrategy>();
        services.AddScoped<ICreatePolicyStrategy, CreateTravelPolicyStrategy>();

        // Auth
        services.AddScoped<IJwtService,      JwtService>();
        services.AddScoped<IPasswordHasher,  BcryptPasswordHasher>();

        // TRM — integración con API Socrata
        services.AddMemoryCache();
        services.AddHttpClient<ITrmService, TrmService>();

        return services;
    }
}
