using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Server.Presentation.Authorization;

public sealed class SelfOrAdminHandler : AuthorizationHandler<SelfOrAdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SelfOrAdminRequirement requirement)
    {
        if (context.User.IsInRole("admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var routeId = context.Resource switch
        {
            AuthorizationFilterContext filterContext => filterContext.RouteData.Values["id"]?.ToString(),
            HttpContext httpContext => httpContext.GetRouteValue("id")?.ToString(),
            _ => null,
        };

        var subject = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(routeId) && subject == routeId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
