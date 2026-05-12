using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureDocs.Application.Common.Interfaces;
using SecureDocs.Application.Documents;
using SecureDocs.Infrastructure.Persistence;
using SecureDocs.Infrastructure.Persistence.Repositories;
using SecureDocs.Infrastructure.Redis;
using StackExchange.Redis;

namespace SecureDocs.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

        services.AddSingleton<IPayloadStore, RedisPayloadStore>();

        return services;
    }
}
