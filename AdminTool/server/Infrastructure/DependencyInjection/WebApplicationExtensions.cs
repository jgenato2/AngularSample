using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Server.Application.Abstractions;
using Server.Infrastructure.Configuration;

namespace Server.Infrastructure.DependencyInjection;

public static class WebApplicationExtensions
{
    public static WebApplication InitializeData(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IUserStore>();
        var admin = scope.ServiceProvider.GetRequiredService<IOptions<SeedAdminSettings>>().Value;
        store.SeedAdmin(admin.Name, admin.Email, admin.Role, admin.Password);
        return app;
    }
}
