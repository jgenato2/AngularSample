using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Server.Presentation.Authorization;
using Server.Presentation.Configuration;

namespace Server.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        var docs = configuration.GetRequiredSection("ApiDocs").Get<ApiDocsSettings>() ?? throw new InvalidOperationException("ApiDocs settings are required.");
        if (string.IsNullOrWhiteSpace(docs.Title) || string.IsNullOrWhiteSpace(docs.Version))
        {
            throw new InvalidOperationException("ApiDocs:Title and ApiDocs:Version are required.");
        }

        services.Configure<ApiDocsSettings>(configuration.GetRequiredSection("ApiDocs"));
        services.AddSingleton<IAuthorizationHandler, SelfOrAdminHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(PresentationPolicies.AdminOnly, policy => policy.RequireRole("admin"));
            options.AddPolicy(PresentationPolicies.SelfOrAdmin, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new SelfOrAdminRequirement()));
        });
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = docs.Title,
                Version = docs.Version,
            });

            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter a JWT as: Bearer {token}",
            };

            options.AddSecurityDefinition("Bearer", scheme);
            options.AddSecurityRequirement(doc =>
            {
                var schemeRef = new OpenApiSecuritySchemeReference("Bearer", doc, null);
                return new OpenApiSecurityRequirement
                {
                    [schemeRef] = new List<string>(),
                };
            });
        });

        return services;
    }
}
