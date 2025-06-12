namespace HuntTheWumpus.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HuntTheWumpus.Application.Ports;
using HuntTheWumpus.Infrastructure.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register database connection
        services.AddScoped<IDatabaseConnection, MySqlDatabaseConnection>();
        
        // Register repositories
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGameStateRepository, GameStateRepository>();
        services.AddScoped<IHighScoreRepository, HighScoreRepository>();
        
        return services;
    }
}
