using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Server.Application.Abstractions;
using Server.Infrastructure.Configuration;
using Server.Infrastructure.Persistence;
using Server.Infrastructure.Security;

namespace Server.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetRequiredSection("Jwt").Get<JwtSettings>() ?? throw new InvalidOperationException("Jwt settings are required.");
        var cors = configuration.GetRequiredSection("Cors").Get<CorsSettings>() ?? throw new InvalidOperationException("Cors settings are required.");
        var admin = configuration.GetRequiredSection("SeedAdmin").Get<SeedAdminSettings>() ?? throw new InvalidOperationException("SeedAdmin settings are required.");

        if (string.IsNullOrWhiteSpace(jwt.Key) || string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience))
        {
            throw new InvalidOperationException("Jwt:Key, Jwt:Issuer, and Jwt:Audience are required.");
        }

        if (cors.AllowedOrigins is null || cors.AllowedOrigins.Length == 0)
        {
            throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin.");
        }

        if (string.IsNullOrWhiteSpace(admin.Name) || string.IsNullOrWhiteSpace(admin.Email) ||
            string.IsNullOrWhiteSpace(admin.Role) || string.IsNullOrWhiteSpace(admin.Password))
        {
            throw new InvalidOperationException("SeedAdmin:Name, Email, Role, and Password are required.");
        }

        services.Configure<JwtSettings>(configuration.GetRequiredSection("Jwt"));
        services.Configure<CorsSettings>(configuration.GetRequiredSection("Cors"));
        services.Configure<SeedAdminSettings>(configuration.GetRequiredSection("SeedAdmin"));

        services.AddCors(options =>
        {
            options.AddPolicy("client", policy =>
            {
                policy.WithOrigins(cors.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });

        services.AddAuthorization();
        services.AddSingleton<IUserStore, UserStore>();
        services.AddSingleton<IClaimsStore, ClaimsStore>();
        services.AddSingleton<IClaimAuditLogStore, ClaimAuditLogStore>();
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }
}
