using Microsoft.AspNetCore.Authorization;

namespace Server.Presentation.Authorization;

public sealed class AdminOnlyAttribute : AuthorizeAttribute
{
    public AdminOnlyAttribute()
    {
        Policy = PresentationPolicies.AdminOnly;
    }
}
