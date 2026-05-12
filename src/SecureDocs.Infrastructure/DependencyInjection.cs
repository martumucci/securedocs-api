using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureDocs.Application.Common.Interfaces;
using SecureDocs.Application.Documents;
using SecureDocs.Infrastructure.Messaging;
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
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "securedocs")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

        services.AddSingleton<IPayloadStore, RedisPayloadStore>();

        var massTransitOptions = configuration
            .GetSection(MassTransitOptions.SectionName)
            .Get<MassTransitOptions>() ?? new MassTransitOptions();

        services.Configure<MassTransitOptions>(configuration.GetSection(MassTransitOptions.SectionName));

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(massTransitOptions.Outbox.QueryDelaySeconds);
                o.QueryMessageLimit = massTransitOptions.Outbox.QueryMessageLimit;
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMq"));
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
