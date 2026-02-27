using Microsoft.AspNetCore.Authorization;

namespace Server.Presentation.Authorization;

public sealed class SelfOrAdminAttribute : AuthorizeAttribute
{
    public SelfOrAdminAttribute()
    {
        Policy = PresentationPolicies.SelfOrAdmin;
    }
}
