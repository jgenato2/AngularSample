using Microsoft.Extensions.DependencyInjection;
using Server.Application.Abstractions;
using Server.Application.Services;

namespace Server.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthApplicationService, AuthApplicationService>();
        services.AddScoped<IUsersApplicationService, UsersApplicationService>();
        return services;
    }
}
