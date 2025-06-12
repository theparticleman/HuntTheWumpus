namespace HuntTheWumpus.Application;

using Microsoft.Extensions.DependencyInjection;
using HuntTheWumpus.Application.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IGameService, GameService>();
        
        return services;
    }
}
